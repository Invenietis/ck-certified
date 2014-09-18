using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CK.WPF.Controls
{
    public class TrimmingTextBlock : Label
    {
        TextBlock _textBlock;
        Span _span;
        int _trimLevel;
        FormattedText _lastFormattedText;
        double _trimCharWidth;
        string _trimChar = "..."; //todo property
        double _trimCharFontSize = 7;//todo property

        public TrimmingTextBlock()
            : base()
        {
            _trimCharWidth = new FormattedText(
                        _trimChar,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface( FontFamily,
                                      FontStyle,
                                      FontWeight,
                                      FontStretch ),
                        _trimCharFontSize,
                        Foreground
                    ).Width;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _textBlock = (TextBlock)GetTemplateChild( "PART_Text" );
            _textBlock.Inlines.Clear();
            _textBlock.Inlines.Add( _span );
        }

        protected override void OnRenderSizeChanged( SizeChangedInfo sizeInfo )
        {
            base.OnRenderSizeChanged( sizeInfo );
            if( _lastFormattedText == null ) TrimmingText( Text );
            else TrimmingText( _lastFormattedText.Text );
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( TrimmingTextBlock ), new FrameworkPropertyMetadata( string.Empty, new PropertyChangedCallback( TrimmingTextBlock.OnTextPropertyChanged ) ) );
        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent( "TextChangedEvent", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<string> ), typeof( TrimmingTextBlock ) );
        public event RoutedPropertyChangedEventHandler<string> TextChanged
        {
            add { AddHandler( TextChangedEvent, value ); }
            remove { RemoveHandler( TextChangedEvent, value ); }
        }

        private static void OnTextPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            TrimmingTextBlock trimmingTextBlock = (TrimmingTextBlock)d;
            if( trimmingTextBlock != null )
                trimmingTextBlock.OnTextChanged( (string)e.OldValue, (string)e.NewValue );
        }

        private void OnTextChanged( string oldValue, string newValue )
        {
            RoutedPropertyChangedEventArgs<string> args = new RoutedPropertyChangedEventArgs<string>( oldValue, newValue );
            args.RoutedEvent = TextChangedEvent;
            RaiseEvent( args );

            _lastFormattedText = CreateFormattedText(newValue);

            TrimmingText( newValue );
        }

        public string Text
        {
            get { return (string)GetValue( TextProperty ); }
            set { SetValue( TextProperty, value ); }
        }

        private void TrimmingText( string newValue )
        {
            _span = ProccessText();
            if( _textBlock != null )
            {
                _textBlock.Inlines.Clear();
                _textBlock.Inlines.Add(_span);
            }
        }

        private Span ProccessText()
        {
            if( Text.Length == 0 ) return new Span();

            if( _lastFormattedText == null )
            {
                _lastFormattedText = CreateFormattedText( Text );
            }

            if( _lastFormattedText.Width + _trimCharWidth < ActualWidth )
            {
                if( _trimLevel == 0 ) return new Span( new Run( _lastFormattedText.Text ) );

                return GetLessTrimmedText();
            }
            else if( _trimLevel != Text.Length )
            {
                return GetMostTrimmedText();
            }
            return new Span( new Run( _trimChar ) );
        }

        private Span GetMostTrimmedText()
        {
            Debug.Assert( _trimLevel != Text.Length );
            bool widthOK = true;
            string part1;
            string part2;
            FormattedText temp = null;

            do
            {
                if( ++_trimLevel == Text.Length )
                {
                    _lastFormattedText = CreateFormattedText( string.Empty );
                    return new Span( new Run( _trimChar ) );
                }

                part1 = GetFirstStringPart();
                part2 = GetLastStringPart();
                temp = CreateFormattedText( part1 + part2 );

                widthOK = temp.Width + _trimCharWidth < ActualWidth;
            } while( !widthOK );

            part1 = GetFirstStringPart();
            part2 = GetLastStringPart();

            var s = new Span();

            s.Inlines.Add( new Run( part1 ) );
            s.Inlines.Add( new Run( _trimChar ) );
            s.Inlines.Add( new Run( part2 ) );

            Debug.Assert( temp != null );
            _lastFormattedText = temp;

            return s;
        }

        private Span GetLessTrimmedText()
        {
            bool widthOK = true;
            FormattedText lastValidText = null;
            string part1;
            string part2;
            FormattedText temp;

            do
            {
                _trimLevel--;

                part1 = GetFirstStringPart();
                part2 = GetLastStringPart();
                temp = CreateFormattedText( part1 + part2 );

                widthOK = temp.Width + _trimCharWidth < ActualWidth;

                if( widthOK )
                { 
                    if(_trimLevel == 0 )
                    {
                        _lastFormattedText = CreateFormattedText(Text);
                        return new Span( new Run( Text ) );
                    }
                    lastValidText = temp;
                }
            }while(widthOK);
            _trimLevel++;

            part1 = GetFirstStringPart();
            part2 = GetLastStringPart();

            var s = new Span();

            s.Inlines.Add(new Run(part1));
            s.Inlines.Add(new Run(_trimChar));
            s.Inlines.Add(new Run(part2));

            _lastFormattedText = lastValidText;

            return s;
        }

        private string GetFirstStringPart()
        {
            return Text.Substring( 0, _trimLevel );
        }

        private string GetLastStringPart()
        {
            return Text.Substring( Text.Length - _trimLevel, _trimLevel );
        }

        private FormattedText CreateFormattedText( string stringFormat )
        {
                return new FormattedText(
                        stringFormat,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface( FontFamily,
                                      FontStyle,
                                      FontWeight,
                                      FontStretch ),
                        FontSize,
                        Foreground
                    );
        }
    }
}
