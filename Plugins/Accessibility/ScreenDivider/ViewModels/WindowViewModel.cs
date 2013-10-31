using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using ScreenDivider.Views;

namespace ScreenDivider.ViewModels
{
    public class WindowViewModel : INotifyPropertyChanged
    {
        bool _isOnlyOne = false;
        bool _isEnter = false;
        bool _isPause = false;

        public WindowViewModel()
        {
            IsActive = false;
            GridOwned = new GridViewModel( this );
        }

        public bool IsPause { get { return _isPause; } }

        public bool IsActive { get; set; }

        public bool IsEnter { get { return _isEnter; } }

        public bool IsOnlyOne { get { return _isOnlyOne; } }

        internal void Pause()
        {
            _isPause = !_isPause;
            OnPropertyChanged( "IsPause" );
        }

        public GridViewModel GridOwned { get; set; }

        public GridZone Enter( bool isOnlyOne = false )
        {
            if( !_isEnter )
            {
                _isOnlyOne = isOnlyOne;
                GridZone g = new GridZone( GridOwned.Current );
                Grid.SetColumn( g, 0 );
                Grid.SetRow( g, 0 );

                GridOwned.Switch();
                GridOwned.ExitNode += ExitGridNode;

                _isEnter = true;
                return g;
            }
            GridOwned.Enter();
            return null;
        }

        public void Switch()
        {
            GridOwned.Switch();
        }

        public void Exit()
        {
            GridOwned.ExitCommand();
        }

        void ExitGridNode( object sender, Events.ExitGridEventArgs e )
        {
            if( !IsOnlyOne )
                _isEnter = false;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string name )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( name ) );
            }
        }

        #endregion
    }
}
