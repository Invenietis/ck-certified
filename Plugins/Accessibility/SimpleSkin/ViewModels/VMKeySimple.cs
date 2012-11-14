#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMKeySimple.cs) is part of CiviKey. 
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
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;
using HighlightModel;
using CK.Core;

namespace SimpleSkin.ViewModels
{
    internal class VMKeySimple : VMKey<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>, IHighlightableElement
    {
        public VMKeySimple( VMContextSimple ctx, IKey k ) 
            : base( ctx, k )
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "HighlightBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
                OnPropertyChanged( "ShowLabel" );
            } );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "HighlightBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
                OnPropertyChanged( "ShowLabel" );
            }
        }

        protected override void OnDispose()
        {
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            base.OnDispose();
        }

        public Image Image
        {
            get { return LayoutKeyMode.GetPropertyValue<Image>( Context.Config, "Image" ); }
        }

        public Color Background
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ); }
        }
        
        public Color HoverBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ); }
        }

        public Color HighlightBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightBackground", Background ); }
        }

        public Color PressedBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "PressedBackground", HoverBackground ); }
        }

        public Color LetterColor
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "LetterColor", Colors.Black ); }
        }

        public FontStyle FontStyle
        {
            get { return LayoutKeyMode.GetPropertyValue<FontStyle>( Context.Config, "FontStyle" ); }
        }

        public FontWeight FontWeight
        {
            get { return LayoutKeyMode.GetPropertyValue<FontWeight>( Context.Config, "FontWeight", FontWeights.Normal ); }
        }

        public double FontSize
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "FontSize", 15 ); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return LayoutKeyMode.GetPropertyValue<TextDecorationCollection>( Context.Config, "TextDecorations" ); }
        }

        public bool ShowLabel
        {
            get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        public double Opacity
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ); }
        }

        bool _isHighlighting;
        public bool IsHighlighting
        {
            get { return _isHighlighting; }
            set
            {
                if( value != _isHighlighting )
                {
                    _isHighlighting = value;
                    OnPropertyChanged( "IsHighlighting" );
                }
            }
        }

        public override VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple> Parent
        {
            get { return Context.Obtain( Model.Zone ); }
        }

        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout.Current; }
        }


        #region IHighlightableElement Members

        public IReadOnlyList<IHighlightableElement> Children
        {
            get { return ReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public SkippingBehavior Skip
        {
            get { return Visible != Visibility.Visible ? SkippingBehavior.Skip : SkippingBehavior.None; }
        }

        #endregion
    }
}
