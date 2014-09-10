using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace CK.WPF.Controls
{
    //
    /// <summary>
    /// Elements which are not on the VisualTree don't have access to the DataContext, so you can't leverage the Binding ExtensionMarkup with them.
    /// By setting an instance of this class in a ResourceDictionary, with Data bound to the Property you wanted bound in the first place,
    /// The instance can then be set as Binding Source of the aforementionned Property.
    /// 
    /// see : http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
    /// </summary>
    public class BindingProxy : Freezable
    {
        public object Data
        {
            get
            {
                return (object)GetValue( DataProperty );
            }
            set
            {
                SetValue( DataProperty, value );
            }
        }
        public static readonly DependencyProperty DataProperty = 
            DependencyProperty.Register( "Data", typeof( object ), typeof( BindingProxy ) );

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}