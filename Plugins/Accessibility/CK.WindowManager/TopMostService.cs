using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CK.WindowManager
{

    [Plugin( "{5EDDAD09-122A-4967-9A06-374B21E403FC}", Categories = new string[] { "Accessibility" }, PublicName = "CK.WindowManager.TopMostService", Version = "0.1.0" )]
    public class TopMostService : ITopMostService, IPlugin
    {

        const string PluginPublicName = "CK.WindowManager.TopMostService";
        const string PluginIdString = "{5EDDAD09-122A-4967-9A06-374B21E403FC}";
        const string PluginIdVersion = "0.1.0";

        Dictionary<Window,string> _windowToString;

        Dictionary<string,List<Window>> _stringToWindows;

        List<string> _indexList;

        public TopMostService()
        {
            _windowToString = new Dictionary<Window, string>();
            _stringToWindows = new Dictionary<string, List<Window>>();
            _indexList = new List<string>();
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion

        #region ITopMostService Members

        public bool RegisterTopMostElement( string levelName, Window window )
        {
            if( string.IsNullOrEmpty( levelName ) ) throw new ArgumentNullException( "levelName" );
            if( window == null ) throw new ArgumentNullException( "window" );

            if( _windowToString.ContainsKey( window ) ) return false;

            List<Window> windows;
            if( _stringToWindows.TryGetValue( levelName, out windows ) )
            {
                windows.Add( window );
                _windowToString.Add( window, levelName );
                window.Activated += window_Activated;
                return true;
            }
            else
            {
                int level;
                if( int.TryParse( levelName, out level ) )
                {
                    if( _indexList.Count == 0 )
                    {
                        AddNewIndexAndItem( 0, levelName, window );

                        return true;
                    }

                    int parseInt;
                    for( int i = 0; i < _indexList.Count; i++ )
                    {
                        if( int.TryParse( _indexList[0], out parseInt ) )
                        {
                            if( parseInt >= level )
                            {
                                AddNewIndexAndItem( i, levelName, window );

                                return true;
                            }
                        }
                    }

                    AddNewIndexAndItem( _indexList.Count, levelName, window );
                    return true;
                }
            }
            return false;
        }

        void AddNewIndexAndItem( int index, string levelName, Window window)
        {
            _indexList.Insert( index, levelName );

            _windowToString.Add( window, levelName );

            List<Window> list = new List<Window>();
            list.Add( window );
            _stringToWindows.Add( levelName, list );

            window.Activated += window_Activated;
        }
        int _nb = 0;
        void window_Activated( object sender, EventArgs e )
        {
            Window window = sender as Window;
            if( window != null )
            {
                Debug.Assert( _windowToString.ContainsKey( window ) );

                string levelName;
                if( _windowToString.TryGetValue( window, out levelName ) )
                {
                    Debug.Assert( _indexList.Contains( levelName ) );

                    List<Window> windows;
                    foreach( var s in _indexList )
                    {
                        Debug.Assert( _stringToWindows.ContainsKey( s ) );

                        if( _stringToWindows.TryGetValue( s, out windows ) )
                        {
                            foreach( var w in windows )
                            {
                                DispatchWhenRequired( w.Dispatcher, () =>
                                    {
                                        CK.Windows.Interop.Win.Functions.SetWindowPos( new WindowInteropHelper( w ).Handle, new IntPtr( (int)CK.Windows.Interop.Win.SpecialWindowHandles.TOPMOST ), 0, 0, 0, 0, Windows.Interop.Win.SetWindowPosFlags.IgnoreMove | Windows.Interop.Win.SetWindowPosFlags.IgnoreResize | Windows.Interop.Win.SetWindowPosFlags.DoNotActivate );
                                    } );
                            }
                        }
                    }
                }
            }
        }

        private void DispatchWhenRequired( Dispatcher d,  Action a )
        {
            if( d.CheckAccess() ) a();
            else d.Invoke( a );
        }

        public bool UnregisterTopMostElement( Window window )
        {
            if( window == null ) throw new ArgumentNullException( "window" );

            string levelName;
            if( _windowToString.TryGetValue( window, out levelName ) )
            {
                Debug.Assert( _stringToWindows.ContainsKey( levelName ) );
                List<Window> windows;
                if( _stringToWindows.TryGetValue( levelName, out windows ) )
                {
                    window.Activated -= window_Activated;
                    _windowToString.Remove( window );
                    windows.Remove( window );
                    if( windows.Count == 0 )
                    {
                        _stringToWindows.Remove( levelName );
                        _indexList.Remove( levelName );
                    }
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
