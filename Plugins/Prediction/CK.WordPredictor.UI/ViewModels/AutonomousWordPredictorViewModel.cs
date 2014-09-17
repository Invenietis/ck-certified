using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows;
using CK.WordPredictor.Model;
using CK.WPF.Controls;
using CommonServices;
using HighlightModel;
using CK.Core;

namespace CK.WordPredictor.UI.ViewModels
{
    public class AutonomousWordPredictorViewModel : VMBase, IHighlightableElement, IDisposable
    {
        #region fields

        IWordPredictorService _wordPredictor;
        ICommandManagerService _commandManager;
        ILayout _layout;
        IPluginConfigAccessor _config;
        ISharedData _sharedData;

        #endregion fields

        public AutonomousWordPredictorViewModel( ILayout layout, IPluginConfigAccessor config, ISharedData sharedData, IWordPredictorService wordPredictor, ICommandManagerService commandManager )
        {
            _commandManager = commandManager;
            _wordPredictor = wordPredictor;
            _layout = layout;
            _config = config;
            _sharedData = sharedData;

            _sharedData.SharedPropertyChanged += _sharedData_SharedPropertyChanged;

            _wordPredictor.Words.CollectionChanged += OnCollectionChanged;
            Words = _wordPredictor.Words;

            UpdateAllProperties();
        }

        void _sharedData_SharedPropertyChanged( object sender, SharedPropertyChangedEventArgs e )
        {
            UpdateAllProperties();
        }

        void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            Words = _wordPredictor.Words;
            RefreshVMWords();
        }

        IWordPredictedCollection _words;
        public IWordPredictedCollection Words
        {
            get { return _words; }
            set 
            {
                if( value != _words )
                {
                    _words = value;
                    RefreshVMWords();
                    OnPropertyChanged();
                }
            }
        }

        List<PredictedWordViewModel> _vmWords;
        public List<PredictedWordViewModel> VMWords
        {
            get { return _vmWords; }
            set
            {
                if( value != _vmWords )
                {
                    _vmWords = value;
                    OnPropertyChanged();
                }
            }
        }

        private void RefreshVMWords()
        {
            List<PredictedWordViewModel> vmWords = new List<PredictedWordViewModel>();
            foreach( var predictedWord in _words )
            {
                vmWords.Add( new PredictedWordViewModel( predictedWord.Word, _layout, _config, _commandManager ) );
            }
            VMWords = vmWords;
        }

        /// <summary>
        /// Raises this object's <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected override void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            base.OnPropertyChanged( propertyName );
        }

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
                    SafeSet<bool>( value, ( v ) => _isHighlighting = v );
                    OnPropertyChanged( "IsHighlighting" );
                    foreach( var vmWord in VMWords )
                    {
                        vmWord.IsHighlighting = value;
                    }
                }
            }
        }

        public Core.ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return VMWords.ToReadOnlyList(); }
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
            get { return VMWords.Count == 0 ? SkippingBehavior.Skip : SkippingBehavior.None; }
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
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        #endregion

        internal void OnCurrentKeyboardChanged( ILayout layout )
        {
            _layout = layout;
            foreach( var vmWord in _vmWords )
            {
                vmWord.OnCurrentLayoutChanged( layout );
            }
        }

        private void UpdateAllProperties()
        {
            SafeUpdateKeyboardBackgroundColor();
            SafeUpdateKeyboardBorderBrush();
            SafeUpdateKeyboardBorderThickness();
            SafeUpdateKeyboardOpacity();
        }

        private void SafeUpdateKeyboardOpacity()
        {
            var c = _sharedData.WindowOpacity;
            SafeSet<double>( c, ( v ) =>
            {
                KeyboardOpacity = v;
                OnPropertyChanged( "KeyboardOpacity" );
            } );
        }

        private void SafeUpdateKeyboardBackgroundColor()
        {
            Color c = _sharedData.WindowBackgroundColor;
            SafeSet<Color>( c, ( v ) =>
            {
                KeyboardBackgroundColor = v;
                OnPropertyChanged( "KeyboardBackgroundColor" );
            } );
        }

        private void SafeUpdateKeyboardBorderThickness()
        {
            int c = _sharedData.WindowBorderThickness;
            SafeSet<int>( c, ( v ) =>
            {
                KeyboardBorderThickness = v;
                OnPropertyChanged( "KeyboardBorderThickness" );
            } );
        }

        private void SafeUpdateKeyboardBorderBrush()
        {
            Color c = _sharedData.WindowBorderBrush;
            SafeSet<Color>( c, ( v ) =>
            {
                KeyboardBorderBrush = new SolidColorBrush( v );
                OnPropertyChanged( "KeyboardBorderBrush" );
            } );
        }

        Brush _keyboardBorderBrush;
        public Brush KeyboardBorderBrush
        {
            get { return _keyboardBorderBrush; }
            set
            {
                if( _keyboardBorderBrush != value )
                {
                    _keyboardBorderBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        int _keyboardBorderThickness;
        public int KeyboardBorderThickness
        {
            get { return _keyboardBorderThickness; }
            set
            {
                if( _keyboardBorderThickness != value )
                {
                    _keyboardBorderThickness = value;
                    OnPropertyChanged();
                }
            }
        }

        Color _keyboardBackgroundColor;
        public Color KeyboardBackgroundColor
        {
            get { return _keyboardBackgroundColor; }
            set
            {
                if( _keyboardBackgroundColor != value )
                {
                    _keyboardBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        double _keyboardOpacity;
        public double KeyboardOpacity
        {
            get { return _keyboardOpacity; }
            set
            {
                if( _keyboardOpacity != value )
                {
                    _keyboardOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        void SafeSet<T>( T value, Action<T> setter, bool synchronous = true )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            T val = value;
            if( synchronous )
                NoFocusManager.Default.NoFocusDispatcher.Invoke( setter, val );
            else
                NoFocusManager.Default.NoFocusDispatcher.BeginInvoke( setter, val );
        }

        #region IDisposable Members

        public void Dispose()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            _sharedData.SharedPropertyChanged -= _sharedData_SharedPropertyChanged;
            _wordPredictor.Words.CollectionChanged -= OnCollectionChanged;
        }

        #endregion
    }
}
