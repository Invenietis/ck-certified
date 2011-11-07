using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using SimpleSkin;
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Media;

namespace SimpleSkinEditor
{
    public partial class EditorViewModel
    {
        public Color Background
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "Background", Colors.White ).Value; }
            set
            {
                _config[ConfigHolder]["Background"] = value;
                NotifyOfPropertyChange( () => HoverBackground );
                NotifyOfPropertyChange( () => PressedBackground );
            }
        }

        public Color HoverBackground
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "HoverBackground", Background ).Value; }
            set 
            {
                _config[ConfigHolder]["HoverBackground"] = value;
                NotifyOfPropertyChange( () => PressedBackground );
            }
        }

        public Color PressedBackground
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "PressedBackground", HoverBackground ).Value; }
            set { _config[ConfigHolder]["PressedBackground"] = value; }
        }
    }
}
