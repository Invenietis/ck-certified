using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
