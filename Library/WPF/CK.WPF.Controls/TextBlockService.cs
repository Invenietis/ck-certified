#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.Controls\TextBlockService.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CK.WPF.Controls
{
    public class TextBlockService
    {
        public static readonly DependencyPropertyKey IsTextTrimmedKey = 
            DependencyProperty.RegisterAttachedReadOnly( "IsTextTrimmed", typeof( bool ), typeof( TextBlockService ), new PropertyMetadata( false ) );

        public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        static TextBlockService()
        {
            EventManager.RegisterClassHandler( typeof( TextBlock ), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( OnTextBlockSizeChanged ) );
        }

        public static void OnTextBlockSizeChanged( object sender, SizeChangedEventArgs e )
        {
            var textBlock = sender as TextBlock;
            if( textBlock == null ) return;

            if( textBlock.TextTrimming == TextTrimming.None )
            {
                SetIsTextTrimmed( textBlock, false );
            }
            else
            {
                bool isTrimmed = CalculateIsTextTrimmed( textBlock );
                SetIsTextTrimmed( textBlock, isTrimmed );
            }
        }

        private static void SetIsTextTrimmed( TextBlock textBlock, bool isTrimmed )
        {
            textBlock.SetValue( IsTextTrimmedKey, isTrimmed );
        }

        [AttachedPropertyBrowsableForType( typeof( TextBlock ) )] //To show only in the IDE for designers...
        public static Boolean GetIsTextTrimmed( TextBlock target )
        {
            return (Boolean)target.GetValue( IsTextTrimmedProperty );
        }

        private static bool CalculateIsTextTrimmed( TextBlock textBlock )
        {
            if( !textBlock.IsArrangeValid )
            {
                return GetIsTextTrimmed( textBlock );
            }

            Typeface typeface = new Typeface( textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch );

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            FormattedText formattedText = new FormattedText( textBlock.Text, System.Threading.Thread.CurrentThread.CurrentCulture, textBlock.FlowDirection, typeface, textBlock.FontSize, textBlock.Foreground );

            //formattedText.MaxTextWidth = textBlock.ActualWidth;

            // When the maximum text width of the FormattedText instance is set to the actual
            // width of the textBlock, if the textBlock is being trimmed to fit then the formatted
            // text will report a larger height than the textBlock. Should work whether the
            // textBlock is single or multi-line.
            return (formattedText.Width > textBlock.ActualWidth);
        }

        public static readonly DependencyProperty AutomaticToolTipEnabledProperty = 
            DependencyProperty.RegisterAttached( "AutomaticToolTipEnabled", typeof( bool ), typeof( TextBlockService ), new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.Inherits ) );

        [AttachedPropertyBrowsableForType( typeof( DependencyObject ) )]
        public static bool GetAutomaticToolTipEnabled( DependencyObject element )
        {
            if( element == null ) throw new ArgumentNullException( "element" );
            return (bool)element.GetValue( AutomaticToolTipEnabledProperty );
        }

        public static void SetAutomaticToolTipEnabled( DependencyObject element, bool value )
        {
            if( element == null ) throw new ArgumentNullException( "element" );

            element.SetValue( AutomaticToolTipEnabledProperty, value );
        }
    }
}
