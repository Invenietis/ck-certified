using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows;
using CK.Windows.App;
using CK.WPF.Controls;
using CommonServices;
using HighlightModel;

namespace CK.WordPredictor.UI.ViewModels
{
    public class PredictedWordViewModel : VMBase, IHighlightableElement
    {
        #region Fields

        ICommandManagerService _commandManager;
        ILayout _layout;
        IPluginConfigAccessor _config;

        #endregion Fields

        #region Properties

        string _word;
        public string Word
        {
            get { return _word; }
            set
            {
                if( _word != value )
                {
                    _word = value;
                    SetCommand();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the command called when the user releases the left click on the key
        /// </summary>
        public ICommand KeyUpCommand
        {
            get { return _keyUpCmd; }
            set { _keyUpCmd = value; }
        }
        ICommand _keyUpCmd;

        #endregion Properties

        public PredictedWordViewModel( string word, ILayout layout, IPluginConfigAccessor config, ICommandManagerService commandManager )
        {
            _commandManager = commandManager;
            Word = word;
            _layout = layout;
            _config = config;
            _config.ConfigChanged += OnConfigChanged;
            NoFocusManager.Default.ExternalDispatcher.BeginInvoke( (Action)(() =>
                {
                    UpdateAllProperties();
                }) );
        }

        private void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            UpdateAllProperties();
        }

        void SetCommand()
        {
            _keyUpCmd = new VMCommand( () =>
            {
                NoFocusManager.Default.ExternalDispatcher.BeginInvoke( (Action)(() => _commandManager.SendCommand( this, CommandFromWord( _word ) )) );
            } );
        }

        protected virtual string CommandFromWord( string word )
        {
            return String.Format( @"{0}:{1}", "sendPredictedWord", word );
        }

        #region Design elements

        private void UpdateBackground()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            Background = _config[_layout].GetOrSet<Color>( "Background", Colors.White );
        }

        private void UpdateHighlightBackground()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            HighlightBackground = _config[_layout].GetOrSet<Color>( "HighlightBackground", Colors.White );
        }

        private void UpdateHighlightFontColor()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            HighlightFontColor = _config[_layout].GetOrSet<Color>( "HighlightFontColor", Colors.White );
        }

        private void UpdateLetterColor()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            LetterColor = _config[_layout].GetOrSet<Color>( "LetterColor", Colors.Black );
        }

        private void UpdateFontStyle()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            FontStyle = _config[_layout].GetOrSet<FontStyle>( "FontStyle", FontStyles.Normal );
        }

        private void UpdateFontWeight()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            FontWeight = _config[_layout].GetOrSet<FontWeight>( "FontWeight", FontWeights.Normal );
        }

        private void UpdateFontFamily()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            string v = _config[_layout].GetOrSet<string>( "FontFamily", "Arial" );
            if( v.Contains( "pack://" ) )
            {
                string[] split = v.Split( '|' );
                FontFamily = new System.Windows.Media.FontFamily( new Uri( split[0] ), split[1] );
            }
            else FontFamily = new System.Windows.Media.FontFamily( v );
        }

        private void UpdateFontSize()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            FontSize = _config[_layout].GetOrSet<double>( "FontSize", 15 );
        }

        private void UpdateTextDecorations()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            MemoryStream stream = new MemoryStream();
            TextDecorationCollection obj = _config[_layout].GetOrSet<TextDecorationCollection>( "TextDecorations", (TextDecorationCollection)null );
            if( obj != null ) System.Windows.Markup.XamlWriter.Save( obj, stream );
            stream.Seek( 0, SeekOrigin.Begin );
            if( stream.Length > 0 )
                TextDecorations = (TextDecorationCollection)System.Windows.Markup.XamlReader.Load( stream );
            else
                TextDecorations = null;

            stream.Dispose();
        }

        private void UpdateAllProperties()
        {
            UpdateBackground();
            UpdateFontFamily();
            UpdateFontSize();
            UpdateFontStyle();
            UpdateFontWeight();
            UpdateHighlightBackground();
            UpdateHighlightFontColor();
            UpdateLetterColor();
            UpdateTextDecorations();
        }

        Color _background;
        public Color Background
        {
            get { return _background; }
            set
            {
                if( _background != value )
                {
                    _background = value;
                    OnPropertyChanged();
                }
            }
        }

        Color _highlightBackground;
        public Color HighlightBackground
        {
            get { return _highlightBackground; }
            set
            {
                if( _highlightBackground != value )
                {
                    _highlightBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        Color _highlightFontColor;
        public Color HighlightFontColor
        {
            get { return _highlightFontColor; }
            set
            {
                if( _highlightFontColor != value )
                {
                    _highlightFontColor = value;
                    OnPropertyChanged();
                }
            }
        }

        Color _letterColor;
        public Color LetterColor
        {
            get { return _letterColor; }
            set
            {
                if( _letterColor != value )
                {
                    _letterColor = value;
                    OnPropertyChanged();
                }
            }
        }

        FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            get { return _fontStyle; }
            set
            {
                if( _fontStyle != value )
                {
                    _fontStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        FontWeight _fontWeight;
        public FontWeight FontWeight
        {
            get { return _fontWeight; }
            set
            {
                if( _fontWeight != value )
                {
                    _fontWeight = value;
                    OnPropertyChanged();
                }
            }
        }

        System.Windows.Media.FontFamily _fontFamily;
        public System.Windows.Media.FontFamily FontFamily
        {
            get { return _fontFamily; }
            set
            {
                if( _fontFamily != value )
                {
                    _fontFamily = value;
                    OnPropertyChanged();
                }
            }
        }

        double _fontSize;
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                if( _fontSize != value )
                {
                    _fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        TextDecorationCollection _textDecorations;
        public TextDecorationCollection TextDecorations
        {
            get { return _textDecorations; }
            set
            {
                if( _textDecorations != value )
                {
                    _textDecorations = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region IHighlightableElement Members

        bool _isHighlighting;
        public bool IsHighlighting
        {
            get { return _isHighlighting; }
            set
            {
                Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
                if( value != _isHighlighting )
                {
                    _isHighlighting = value;
                    OnPropertyChanged( "IsHighlighting" );
                }
            }
        }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighting = true;
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighting = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            scrollingDirective.NextActionType = ActionType.GoToRelativeRoot;

            if( KeyUpCommand.CanExecute( null ) )
            {
                KeyUpCommand.Execute( null );
            }
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #endregion

        /// <summary>
        /// Raises this object's <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected override void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            base.OnPropertyChanged( propertyName );
        }

        internal void OnCurrentLayoutChanged( ILayout layout )
        {
            _layout = layout;
            UpdateAllProperties();
        }
    }
}
