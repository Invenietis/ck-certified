using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CK.Windows.App;
using MarkdownSharp;

namespace UpdateChecker.View
{
    public class NewerVersionViewModel
    {
        public NewerVersionViewModel( string releasenotes, string newVersion, Action<bool> CloseView, Action<string> SetBrowserContent )
        {
            Title = R.UpdateAvailableTitle;

            OkCommand = new VMCommand( () => CloseView( true ) );
            CancelCommand = new VMCommand( () => CloseView( false ) );

            Header = string.Format( R.UpdateAvailableContent, newVersion );

            if( !string.IsNullOrEmpty( releasenotes ) )
            {
                string cssSnipet = string.Empty;
                using( var rs = typeof( NewerVersionViewModel ).Assembly.GetManifestResourceStream( "UpdateChecker.View.Resources.MarkdownCssSnipet.html" ) )
                using( var rdr = new StreamReader( rs ) )
                    cssSnipet = rdr.ReadToEnd();

                ReleaseNotes = string.Concat( cssSnipet, new Markdown( new MarkdownOptions { AutoHyperlink = false } ).Transform( releasenotes ) );

                SetBrowserContent( ReleaseNotes );
            }
        }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string ReleaseNotes { get; private set; }
        public Visibility BrowserVisibility { get { return string.IsNullOrEmpty( ReleaseNotes ) ? Visibility.Collapsed : Visibility.Visible; } }
        public string Yes { get { return R.Yes; } }
        public string No { get { return R.No; } }
    }
}
