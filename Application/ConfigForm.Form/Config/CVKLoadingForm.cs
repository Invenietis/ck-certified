#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\CVKLoadingForm.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CK.Kernel.Plugin;
using CK.Kernel;
using System.Drawing.Imaging;
using CK.Application.Config;
using CK.Model;
using CK.Plugin;
using SplashScreenCiviKey;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace CK.Config.UI
{
	public partial class CVKLoadingForm : PerPixelAlphaForm
	{
        IPluginManager _pluginManager;

        SplashScreen _splash;
        Storyboard _discoverAnimation;
        Queue<object[]> _animationQueue;

        public IPluginManager PluginManager
        {
            get { return _pluginManager; }
            set
            {
                _pluginManager = value;
                _pluginManager.Discoverer.DiscoverDone += OnPluginsDepencyEnded;
                _pluginManager.Discoverer.DiscoverBegin += OnPluginsDependencyStarted;
            }
        }
        /// <summary>
        /// Prints the discovered infos on the SplashScreen
        /// </summary>
        /// <param name="text"></param>
        private void PrintInfos( string text )
        {
            #if !DEBUG
            _animationQueue.Enqueue( new object[2] { text, "title" } );
            #endif
        }

        private void PrintInfos( IPluginInfo info )
        {
            #if !DEBUG
            _animationQueue.Enqueue( new object[2] { info.PublicName, info.IconUri } );
            #endif
        }

        void StartNextAnimation()
        {
            if( _animationQueue.Count > 0 )
            {
                object[] obj = _animationQueue.Dequeue();
                _splash.Txt.Content = (string)obj[0];
                if( obj[1] is string )
                    _splash.PluginLogo.Source = null;
                else
                    _splash.PluginLogo.Source = (ImageSource)_splash.FindResource( "puzzle" );
                _discoverAnimation.Begin();
            }
            else
                FadeOut();
        }

        public CVKLoadingForm( IContext context )
            : this()
        {
            #if !DEBUG
            _animationQueue = new Queue<object[]>();
            _splash = new SplashScreen();
            Storyboard startup = (Storyboard)_splash.FindResource( "Startup" );
            _discoverAnimation = (Storyboard)_splash.FindResource( "SomethinDiscovered" );
            startup.Completed += ( object sender, EventArgs e ) => StartNextAnimation();
            _discoverAnimation.Completed += ( object sender, EventArgs e ) => StartNextAnimation();
            _splash.Show();
            #endif

            PluginManager = context.PluginManager;
            context.Loading += OnCVKContextLoading;
            context.Loaded += OnCVKContextLoaded;
        }

        void FadeOut()
        {
            Storyboard close = (Storyboard)_splash.FindResource( "Close" );
            close.Completed += ( object s, EventArgs ev ) => _splash.Close();
            _splash.Txt.Content = "Chargement terminé";
            close.Begin();
        }

        CVKLoadingForm()
        {
            InitializeComponent();
        }

        void OnPluginsDependencyStarted( object sender, EventArgs e )
        {
            if( !IsDisposed )
            {
                PrintInfos( Resources.CVKLoaderDependencing );
            }
        }

        void OnPluginsDepencyEnded( object sender, EventArgs e )
        {
            if( !IsDisposed )
            {
                PrintInfos( Resources.CVKLoaderDependencingDone );
            }
        }

        void OnCVKContextLoading( object sender, EventArgs e )
        {
            if( !IsDisposed )
            {
                PrintInfos( Resources.CVKLoaderLoadingContext );
            }
        }

        void OnCVKContextLoaded( object sender, EventArgs e )
        {
            if( !IsDisposed )
            {
                PrintInfos( Resources.CVKLoaderLaunchingCVK );
            }
        }
	}
}
