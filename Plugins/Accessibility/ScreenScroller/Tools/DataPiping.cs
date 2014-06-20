#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\ScreenScroller\Tools\DataPiping.cs) is part of CiviKey. 
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

using System.Windows;

namespace ScreenScroller
{
    public class DataPiping
    {
        #region DataPipes (Attached DependencyProperty)

        public static readonly DependencyProperty DataPipesProperty =
            DependencyProperty.RegisterAttached( "DataPipes",
            typeof( DataPipeCollection ),
            typeof( DataPiping ),
            new UIPropertyMetadata( null ) );

        public static void SetDataPipes( DependencyObject o, DataPipeCollection value )
        {
            o.SetValue( DataPipesProperty, value );
        }

        public static DataPipeCollection GetDataPipes( DependencyObject o )
        {
            return (DataPipeCollection)o.GetValue( DataPipesProperty );
        }

        #endregion
    }

    public class DataPipeCollection : FreezableCollection<DataPipe>
    {

    }

    public class DataPipe : Freezable
    {
        #region Source (DependencyProperty)

        public object Source
        {
            get { return (object)GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source", typeof( object ), typeof( DataPipe ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnSourceChanged ) ) );

        private static void OnSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( (DataPipe)d ).OnSourceChanged( e );
        }

        protected virtual void OnSourceChanged( DependencyPropertyChangedEventArgs e )
        {
            Target = e.NewValue;
        }

        #endregion

        #region Target (DependencyProperty)

        public object Target
        {
            get { return (object)GetValue( TargetProperty ); }
            set { SetValue( TargetProperty, value ); }
        }
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register( "Target", typeof( object ), typeof( DataPipe ),
            new FrameworkPropertyMetadata( null ) );

        #endregion

        protected override Freezable CreateInstanceCore()
        {
            return new DataPipe();
        }
    }
}
