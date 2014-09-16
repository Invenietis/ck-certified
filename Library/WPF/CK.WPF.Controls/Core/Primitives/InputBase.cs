#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.Controls\Core\Primitives\InputBase.cs) is part of CiviKey. 
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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls.Primitives
{
    public abstract class InputBase : Control
    {
        #region Properties

        #region CultureInfo

        public static readonly DependencyProperty CultureInfoProperty = DependencyProperty.Register("CultureInfo", typeof(CultureInfo), typeof(InputBase), new UIPropertyMetadata(CultureInfo.CurrentCulture));
        public CultureInfo CultureInfo
        {
            get { return (CultureInfo)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }

        #endregion //CultureInfo

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(InputBase), new PropertyMetadata(true));
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        #endregion //IsEditable

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InputBase), new UIPropertyMetadata(default(String), OnTextChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            InputBase inputBase = o as InputBase;
            if (inputBase != null)
                inputBase.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnTextChanged(string oldValue, string newValue)
        {

        }

        #endregion //Text

        #region TextAlignment

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(InputBase), new UIPropertyMetadata(TextAlignment.Left));
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }
        

        #endregion //TextAlignment

        #region Watermark

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(object), typeof(InputBase), new UIPropertyMetadata(null));
        public object Watermark
        {
            get { return (object)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        #endregion //Watermark

        #region WatermarkTemplate

        public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register("WatermarkTemplate", typeof(DataTemplate), typeof(InputBase), new UIPropertyMetadata(null));
        public DataTemplate WatermarkTemplate
        {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        #endregion //WatermarkTemplate

        #endregion //Properties
    }
}
