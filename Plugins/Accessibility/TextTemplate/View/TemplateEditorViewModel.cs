using CK.Core;
using CK.WPF.ViewModel;
using HighlightModel;
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;

namespace TextTemplate
{
    public class TemplateEditorViewModel : INotifyPropertyChanged
    {
        bool _isWindowHighlighted;

        HighlightableCommand _ValidateTemplate;

        HighlightableCommand _cancel;

        public event EventHandler TemplateValidated;

        public event EventHandler Canceled;

        public Template Template { get; set; }

        public bool IsWindowHighlighted 
        {
            get { return _isWindowHighlighted; }
            set
            {
                _isWindowHighlighted = value;
                NotifyPropertyChanged( "IsWindowHighlighted" );
            }
        }

        public Color HighlightColor { get { return Color.FromRgb(255, 255, 255); }  }

        public Color HighlightBackgroundColor { get { return Color.FromRgb(132, 200, 105); } }

        public Color PlaceholderColor { get { return Color.FromRgb( 132, 200, 105 ); } }

        public int FontSize { get { return 16; } }

        public HighlightableCommand ValidateTemplate
        {
            get
            {
                if (_ValidateTemplate == null) _ValidateTemplate = new HighlightableCommand(FireTemplateValidated);

                return _ValidateTemplate;
            }
        }

        public HighlightableCommand Cancel
        {
            get
            {
                if (_cancel == null) _cancel = new HighlightableCommand(FireCanceled);

                return _cancel;
            }
        }

        public void Window_KeyDown( object sender, KeyEventArgs e )
        {
            switch( e.Key )
            {
                case Key.Enter:
                    ValidateTemplate.Execute( null );
                    break;
                case Key.Escape:
                    Cancel.Execute( null );
                    break;
            }
        }

        internal void NotifyPropertyChanged( string peropertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( peropertyName ) );
        }

        void FireTemplateValidated()
        {
            if (TemplateValidated != null) TemplateValidated(this, new EventArgs());
        }

        void FireCanceled()
        {
            if (Canceled != null) Canceled(this, new EventArgs());
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class HighlightableCommand : VMCommand, IHighlightableElement, IHighlightable, INotifyPropertyChanged
    {
        bool _isHighlighted;
    
        public HighlightableCommand(Action action) : base(action)
        {

        }

        public bool IsHighlighted 
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                NotifyPropertyChanged("IsHighlighted");
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

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public ScrollingDirective BeginHighlight(BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective)
        {
            IsHighlighted = true;

            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight(EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective)
        {
            IsHighlighted = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement(ScrollingDirective scrollingDirective)
        {
            Execute(null);
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        public bool IsSelected
        {
            get { return IsHighlighted; }
            set { }
        }

    }
}
