using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScreenDivider.Converters;
using ScreenDivider.Events;
using ScreenDivider.ViewModels;
using ScreenDivider.Views.Controls;

namespace ScreenDivider.Views
{
    /// <summary>
    /// Interaction logic for GridZone.xaml
    /// </summary>
    public partial class GridZone : UserControl
    {
        readonly PanelViewModel _panel;
        IList<DockPanel> _dockPanels = new List<DockPanel>();

        public GridZone( PanelViewModel panel )
        {
            _panel = panel;
            DataContext = panel;
            InitializeComponent();

            if( panel != null )
            {
                panel.EnterNow += OnEnterNow;

                _panel.CreatePanels();
                CreateDefinitions();
                CreatePanels();
            }
        }

        void CreateDefinitions()
        {
            for( int i = 0; i < _panel.MaxColumnByRowProperty; i++ )
            {
                SplitGrid.ColumnDefinitions.Add( new ColumnDefinition() );
            }

            for( int i = 0; i < _panel.MaxRowProperty; i++ )
            {
                SplitGrid.RowDefinitions.Add( new RowDefinition() );
            }
        }

        void CreatePanels()
        {
            int nbPanels = _panel.MaxColumnByRowProperty * _panel.MaxRowProperty;

            int column = 0;
            int row = 0;
            bool rightDirection = true;
            for( int i = 0; i < nbPanels; i++ )
            {
                var dp = new DockPanel();
                dp.DataContext = _panel.Panels[i];
                dp.SetBinding( DockPanel.BackgroundProperty, new Binding( "IsActive" ) { Converter = new BooleanToColor() } );

                Grid.SetColumn( dp, column );
                Grid.SetRow( dp, row );

                SplitGrid.Children.Add( dp );

                if( rightDirection ) column++;
                else column--;

                bool onStartDirection = rightDirection;
                if( column >= _panel.MaxColumnByRowProperty && rightDirection )
                {
                    row++;
                    column--;
                    rightDirection = false;
                }
                else if( column == -1 && !rightDirection )
                {
                    row++;
                    rightDirection = true;
                    column++;
                }

                ArrowUserControl image = new ArrowUserControl( i == nbPanels - 1 && _panel.MaxRowProperty == 2 ? ScrollingDirection.Top : i == nbPanels - 1 ? ScrollingDirection.GoToParent : onStartDirection != rightDirection ? ScrollingDirection.Bottom : rightDirection ? ScrollingDirection.Right : ScrollingDirection.Left );
                _panel.Grid.LastNodeBeforeGoToWindow += image.GoToWindow;
                dp.Children.Add( image );

                _dockPanels.Add( dp );
            }
        }

        private void OnEnterNow( object sender, EnterNowEventArgs e )
        {
            var dp = _dockPanels[e.CurrentPosition];

            if( dp != null )
            {
                var grid = new GridZone( e.Panel );
                dp.Children.Add( grid );
                e.Panel.ExitNode += OnExit;
            }
        }

        private void OnExit( object sender, ExitPanelEventArgs args )
        {
            foreach( var dp in _dockPanels )
            {
                if( dp.Children.Count > 1 )
                    dp.Children.RemoveAt( 1 );
            }
        }
    }
}
