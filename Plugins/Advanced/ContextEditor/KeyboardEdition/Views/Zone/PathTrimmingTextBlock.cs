using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KeyboardEditor
{
    public class PathTrimmingTextBlock : TextBlock, INotifyPropertyChanged
    {

        FrameworkElement _container;


        public PathTrimmingTextBlock()
        {
            this.Loaded += new RoutedEventHandler( PathTrimmingTextBlock_Loaded );
        }

        void PathTrimmingTextBlock_Loaded( object sender, RoutedEventArgs e )
        {
            if( this.Parent == null ) throw new InvalidOperationException( "PathTrimmingTextBlock must have a container such as a Grid." );

            _container = (FrameworkElement)this.Parent;
            _container.SizeChanged += new SizeChangedEventHandler( container_SizeChanged );

            Text = GetTrimmedPath( _container.ActualWidth );
        }

        void container_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            Text = GetTrimmedPath( _container.ActualWidth );
        }

        public string Path
        {
            get { return (string)GetValue( PathProperty ); }
            set { SetValue( PathProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register( "Path", typeof( string ), typeof( PathTrimmingTextBlock ), new UIPropertyMetadata( "" ) );
        string GetTrimmedPath( double width )
        {
            if( Path.Length == 0 ) return Path;

            string filename = System.IO.Path.GetFileName( Path );
            string directory = System.IO.Path.GetDirectoryName( Path );
            FormattedText formatted;
            int dichotomieLength = directory.Length / 2;
            string startDirectory = string.Empty;
            string endDirectory = string.Empty;
            bool widthOK = false;
            double lastBestDifferenceWidth = double.MaxValue;

            formatted = new FormattedText(
                    "{0}...\\{1}".FormatWith( directory, filename ),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    FontFamily.GetTypefaces().First(),
                    FontSize,
                    Foreground
                    );

            do
            {
                if( formatted.Width < width )
                {
                    dichotomieLength += dichotomieLength / 2;
                    startDirectory = string.Empty;
                    for( int i = 1; i < dichotomieLength; i++ ) startDirectory += directory[i];
                    endDirectory = string.Empty;
                    for( int i = directory.Length - dichotomieLength; i < directory.Length; i++ ) endDirectory += directory[i];
                }

                if( formatted.Width >= width )
                {
                    dichotomieLength = dichotomieLength / 2;
                    startDirectory = string.Empty;
                    for( int i = 1; i < dichotomieLength; i++ ) startDirectory += directory[i];
                    endDirectory = string.Empty;
                    for( int i = directory.Length - dichotomieLength; i < directory.Length; i++ ) endDirectory += directory[i];

                    if( startDirectory.Length + endDirectory.Length == 0 ) return "...\\" + filename;
                }

                double lastWidth = width - formatted.Width;
                FormattedText lastFormatted = formatted;

                formatted = new FormattedText(
                    "{0}...{1}\\{2}".FormatWith( startDirectory, endDirectory, filename ),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    FontFamily.GetTypefaces().First(),
                    FontSize,
                    Foreground
                    );

                double newWidth = width - formatted.Width;
                double newDifferenceWidth = lastWidth - newWidth;

                newDifferenceWidth = (newDifferenceWidth < 0) ? -newDifferenceWidth : newDifferenceWidth;
                if( newDifferenceWidth < lastBestDifferenceWidth )
                {
                    widthOK = false;
                    lastBestDifferenceWidth = newDifferenceWidth;
                }
                else
                {
                    if( widthOK && lastFormatted.Width < width )
                        return lastFormatted.Text;
                    widthOK = true;
                }

                //widthOK = formatted.Width <= width;

            } while( true );
            return string.Empty;
        }

        //string GetTrimmedPath(double width)
        //{
        //    string filename = System.IO.Path.GetFileName(Path);
        //    string directory = System.IO.Path.GetDirectoryName(Path);
        //    FormattedText formatted;
        //    bool widthOK = false;
        //    bool changedWidth = false;

        //    do
        //    {
        //        formatted = new FormattedText(
        //            "{0}...\\{1}".FormatWith(directory, filename),
        //            CultureInfo.CurrentCulture,
        //            FlowDirection.LeftToRight,
        //            FontFamily.GetTypefaces().First(),
        //            FontSize,
        //            Foreground
        //            );

        //        widthOK = formatted.Width < width;

        //        if (!widthOK)
        //        {
        //            changedWidth = true;
        //            directory = directory.Substring(0, directory.Length - 1);

        //            if (directory.Length == 0) return "...\\" + filename;
        //        }

        //    } while (!widthOK);

        //    if (!changedWidth)
        //    {
        //        return Path;
        //    }
        //    return "{0}...{1}".FormatWith(directory, filename);
        //}

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged( string name )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }

        #endregion

    }

    static class Extensions
    {
        public static string FormatWith( this string s, params object[] args )
        {
            return string.Format( s, args );
        }
    }
}