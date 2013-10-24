using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScreenDivider.Events;
using ScreenDivider.Views;

namespace ScreenDivider.ViewModels
{
    public class PanelViewModel : INotifyPropertyChanged
    {
        int _loop = 0;
        int _position = 0;
        PanelViewModel _parent = null;
        GridViewModel _grid = null;

        public int MaxColumnByRowProperty { get { return Int32.Parse( ConfigurationManager.AppSettings["MaxColumnByRow"] ); } }

        public int MaxRowProperty { get { return Int32.Parse( ConfigurationManager.AppSettings["MaxRow"] ); } }

        int SwitchLoop
        {
            get
            {
                int d = MaxColumnByRowProperty * MaxRowProperty * Int32.Parse( ConfigurationManager.AppSettings["Loop"] ) - 1;
                return d;
            }
        }

        IList<PanelViewModel> _panels = new List<PanelViewModel>();

        public PanelViewModel( GridViewModel owner, int position, PanelViewModel parent )
            : this( owner )
        {
            _parent = parent;
            if( position > 0 ) _position = position;
        }

        public PanelViewModel( GridViewModel owner )
        {
            _grid = owner;
        }

        public void CreatePanels()
        {
            _panels.Clear();
            int nbPanels = MaxColumnByRowProperty * MaxRowProperty;
            for( int i = 0; i < nbPanels; i++ )
            {
                PanelViewModel p = new PanelViewModel( _grid, _panels.Count - 1, this );
                if( i == 0 )
                {
                    p.IsActive = true;
                }
                _panels.Add( p );
            }
        }

        public event EventHandler<EnterNowEventArgs> EnterNow;

        private void OnEnterNow( EnterNowEventArgs e )
        {
            if( EnterNow != null ) EnterNow( this, e );
        }

        #region INotifyPropertyChanged Members

        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string name )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( name ) );
            }
        }

        #endregion

        public GridViewModel Grid { get { return _grid; } }

        public PanelViewModel Parent { get { return _parent; } }

        public PanelViewModel Current { get { return _panels.Where( a => a.IsActive ).SingleOrDefault() ?? _panels[0]; } }

        public PanelViewModel Next
        {
            get
            {
                int currentIndex = _panels.IndexOf( Current );
                int nextIndex = (currentIndex + 1) < _panels.Count ? currentIndex + 1 : 0;
                return _panels[nextIndex];
            }
        }

        public IList<PanelViewModel> Panels { get { return _panels; } }

        public void Switch()
        {
            var n = Next;

            if( _loop++ < SwitchLoop )
            {
                Current.IsActive = false;
                n.IsActive = true;
            }
            else
            {
                _loop = 0;
                if( Parent != null ) Parent.SetLoop( 0 - ((MaxColumnByRowProperty + MaxRowProperty) - _position) );

                if( _grid.Panels.Count() <= 1 )
                {
                    Current.IsActive = false;
                    n.IsActive = true;
                }
                _grid.ExitCommand();
            }
        }

        void SetLoop( int loop )
        {
            _loop = loop;
        }

        public PanelViewModel Enter()
        {
            _loop = 0;
            Current.IsCurrent = false;
            var newPanel = new PanelViewModel( _grid, _panels.IndexOf( Current ), this );
            OnEnterNow( new EnterNowEventArgs( newPanel, _panels.IndexOf( Current ) ) );
            return newPanel;
        }

        bool _isActive = false;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                _isActive = value;
                IsCurrent = value;
                OnPropertyChanged( "IsActive" );
            }
        }

        bool _isCurrent = false;
        public bool IsCurrent
        {
            get
            {
                return _isCurrent;
            }
            set
            {
                _isCurrent = value;
                OnPropertyChanged( "IsCurrent" );
            }
        }

        public event EventHandler<ExitPanelEventArgs> ExitNode;

        public void Exit()
        {
            if( Parent != null ) Parent.Current.IsCurrent = true;
            if( ExitNode != null )
                ExitNode( this, new ExitPanelEventArgs { Position = 0 } );
        }
    }
}
