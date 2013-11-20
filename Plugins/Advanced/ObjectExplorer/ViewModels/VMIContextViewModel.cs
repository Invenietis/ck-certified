#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMIContextViewModel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using CK.Plugin;
using CK.Plugins.ObjectExplorer.ViewModels.LogViewModels;
using CK.WPF.ViewModel;
using CK.Context;
using CK.Plugin.Hosting;
using CK.Core;
using Host.Services;
using CK.Plugin.Config;
using Caliburn.Micro;
using System.Windows;
using CommonServices;
using System.Windows.Controls;
using Help.Services;

namespace CK.Plugins.ObjectExplorer
{
    public class VMIContextViewModel : Screen, IDisposable
    {
        public bool ManualStop { get; set; }
        public bool Closing { get; private set; }

        Dictionary<IPluginInfo, VMIPlugin> _plugins;
        Dictionary<IServiceInfo, VMIService> _allServices; //There are two colections in order to hide non-dynamic services from the treeview
        Dictionary<IServiceInfo, VMIService> _dynamicServices;
        Dictionary<IServiceReferenceInfo, VMIService> _serviceRefs;
        Dictionary<IAssemblyInfo, VMIAssembly> _assemblies;

        VMLogConfig _vmLogConfig;

        ISelectableElement _selectedElement;
        ISelectableElement _previousSelectedElement;

        public IContext Context { get; private set; }
        public IPluginConfigAccessor Config { get; private set; }
        public IHelpViewerService HelpService { get; private set; }

        PluginRunner _pluginRunner;
        PluginRunner PluginRunner { get { return _pluginRunner; } }

        public VMCollection<VMIAssembly, IAssemblyInfo> Assemblies { get; private set; }
        public VMCollection<VMIPlugin, IPluginInfo> Plugins { get; private set; }
        public VMCollection<VMIService, IServiceInfo> AllServices { get; private set; }
        public VMCollection<VMIService, IServiceInfo> DynamicServices { get; private set; }
        public ObservableCollection<VMIFolder> Categories { get; private set; }
        public VMLogConfig VMLogConfig { get { return _vmLogConfig; } }
        public VMOSInfo OsInfo { get; private set; }
        public VMApplicationInfo VMApplicationInfo { get; private set; }

        public INotificationService NotificationService { get; private set; }

        #region Commands

        public ICommand StopObjectExplorer { get; private set; }

        public ICommand ShowLastReport { get; private set; }

        public ICommand Rediscover { get; private set; }

        public ICommand ShowHelp { get; private set; }

        #endregion

        public VMISelectableElement SelectedElement
        {
            get
            {
                return _selectedElement as VMISelectableElement;
            }
            set
            {
                if( _selectedElement != null )
                    _previousSelectedElement = _selectedElement;

                _selectedElement = value;
                _selectedElement.SelectedChanged();

                if( _previousSelectedElement != null )
                    _previousSelectedElement.SelectedChanged();

                NotifyOfPropertyChange( () => SelectedElement );
            }
        }

        public VMIContextViewModel( IContext context, IPluginConfigAccessor config, ILogService logService, IHelpViewerService helpService )
        {
            DisplayName = "Object explorer";

            Context = context;
            Config = config;
            HelpService = helpService;

            InitializeCommands();

            _plugins = new Dictionary<IPluginInfo, VMIPlugin>();
            _allServices = new Dictionary<IServiceInfo, VMIService>();
            _dynamicServices = new Dictionary<IServiceInfo, VMIService>();
            _serviceRefs = new Dictionary<IServiceReferenceInfo, VMIService>();
            _assemblies = new Dictionary<IAssemblyInfo, VMIAssembly>();

            _pluginRunner = Context.GetService<PluginRunner>( true );
            _pluginRunner.ApplyDone += new EventHandler<ApplyDoneEventArgs>( OnApplyDone );

            NotificationService = context.GetService<INotificationService>();

            Assemblies = new VMCollection<VMIAssembly, IAssemblyInfo>( PluginRunner.Discoverer.AllAssemblies, FindOrCreate );
            Plugins = new VMCollection<VMIPlugin, IPluginInfo>( PluginRunner.Discoverer.AllPlugins.OrderBy( p => p.PublicName ), FindOrCreate );
            AllServices = new VMCollection<VMIService, IServiceInfo>( PluginRunner.Discoverer.AllServices, FindOrCreate );
            DynamicServices = new VMCollection<VMIService, IServiceInfo>( PluginRunner.Discoverer.AllServices.Where( p => p.IsDynamicService ), FindOrCreateDynamic );
            Categories = new ObservableCollection<VMIFolder>();

            _vmLogConfig = new VMLogConfig( this, logService );
            _vmLogConfig.Initialize();

            OsInfo = new VMOSInfo( this );

            VMApplicationInfo = new VMApplicationInfo( this );


            Dictionary<string, List<IPluginInfo>> categoryFolders = new Dictionary<string, List<IPluginInfo>>();
            foreach( IPluginInfo plugin in PluginRunner.Discoverer.AllPlugins )
            {
                foreach( string categ in plugin.Categories )
                {
                    List<IPluginInfo> col;
                    if( !categoryFolders.TryGetValue( categ, out col ) )
                    {
                        col = new List<IPluginInfo>();
                        col.Add( plugin );
                        categoryFolders.Add( categ, col );
                    }
                    else
                    {
                        col.Add( plugin );
                    }
                }
            }
            foreach( KeyValuePair<string, List<IPluginInfo>> item in categoryFolders )
            {
                VMCollection<VMAlias<VMIPlugin>, IPluginInfo> collection = new VMCollection<VMAlias<VMIPlugin>, IPluginInfo>( item.Value, ( info ) => { return new VMAlias<VMIPlugin>( FindOrCreate( info ), null ); } );
                VMIFolder folder = new VMIFolder( collection, item.Key );
                Categories.Add( folder );
            }
        }

        void OnDirtyChanged( object sender, EventArgs e )
        {
            if( _pluginRunner.IsDirty )
                _pluginRunner.Apply();
        }

        void OnApplyDone( object sender, ApplyDoneEventArgs e )
        {
            if( !e.Success && NotificationService != null )
                NotificationService.ShowNotification( Guid.Empty, R.ApplyDoneErrorCaption, R.ApplyDoneError, 4000, NotificationTypes.Warning );
        }

        public VMIPlugin FindOrCreate( IPluginInfo item )
        {
            VMIPlugin p = null;
            if( !_plugins.TryGetValue( item, out p ) )
            {
                p = new VMIPlugin( this, item, null );
                _plugins.Add( item, p );
            }
            return p;
        }

        public VMIService FindOrCreate( IServiceInfo item )
        {
            VMIService s = null;
            if( !_allServices.TryGetValue( item, out s ) )
            {
                s = new VMIService( this, item, null );
                _allServices.Add( item, s );
            }
            return s;
        }

        public VMIService FindOrCreateDynamic( IServiceInfo item )
        {
            VMIService s = null;
            if( !_dynamicServices.TryGetValue( item, out s ) && item.IsDynamicService )
            {
                s = new VMIService( this, item, null );
                _dynamicServices.Add( item, s );
            }
            return s;
        }

        public VMIAssembly FindOrCreate( IAssemblyInfo item )
        {
            VMIAssembly a = null;
            if( !_assemblies.TryGetValue( item, out a ) )
            {
                a = new VMIAssembly( this, item );
                _assemblies.Add( item, a );
            }
            return a;
        }

        public VMIService FindOrCreate( IServiceReferenceInfo item )
        {
            VMIService s = null;
            if( !_serviceRefs.TryGetValue( item, out s ) )
            {
                s = FindOrCreateDynamic( item.Reference );

                if( s == null )
                    s = new VMIService( this, item, null );

                _serviceRefs.Add( item, s );
            }
            return s;
        }

        private void InitializeCommands()
        {
            StopObjectExplorer = new VMCommand(
                () =>
                {
                    // TODO
                } );

            ShowLastReport = new VMCommand(
                () =>
                {
                    // TODO
                } );

            Rediscover = new VMCommand(
                () =>
                {
                    // TODO
                } );
            ShowHelp = new VMCommand(
                () =>
                {
                    HelpService.ShowHelpFor( ObjectExplorer.PluginId );
                } );

        }

        public void Dispose()
        {
            _pluginRunner.ApplyDone -= new EventHandler<ApplyDoneEventArgs>( OnApplyDone );
            VMApplicationInfo.Dispose();

            foreach( VMIBase vm in _plugins.Values )
                vm.Dispose();
            foreach( VMIBase vm in _allServices.Values )
                vm.Dispose();
            foreach( VMIBase vm in _dynamicServices.Values )
                vm.Dispose();
            foreach( VMIBase vm in _assemblies.Values )
                vm.Dispose();
        }

        public override void CanClose( Action<bool> callback )
        {
            Closing = true;

            if( !ManualStop )
            {
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( ObjectExplorer.PluginId.UniqueId, ConfigUserAction.Stopped );
                if( !Context.GetService<ISimplePluginRunner>( true ).Apply() )
                {
                    if( NotificationService != null )
                        NotificationService.ShowNotification( Guid.Empty, R.ApplyDoneErrorCaption, R.ApplyDoneError, 4000, NotificationTypes.Warning );
                    callback( false );
                }
                else
                    callback( true );
            }
            else
                callback( ManualStop );
        }

        protected override void OnDeactivate( bool close )
        {
            if( close ) Dispose();
            base.OnDeactivate( close );
        }
    }
}
