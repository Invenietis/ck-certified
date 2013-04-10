using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using KeyboardEditor.ViewModels;

namespace KeyboardEditor.ViewModels
{
    public class ModeTypeToSelectedConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            ModeTypes type = ModeTypes.Mode;
            ModeTypes param = ModeTypes.Mode;

            if( !Enum.TryParse<ModeTypes>( value.ToString(), out type ) ) throw new ArgumentException( "Passing an object that is not of ModeTypes type into a ModeTypeToSelectedConverter converter value" );
            if( !Enum.TryParse<ModeTypes>( parameter.ToString(), out param ) ) throw new ArgumentException( "Passing an object that is not of ModeTypes type into a ModeTypeToSelectedConverter's converter parameter" );

            return type == param;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            ModeTypes param = ModeTypes.Mode;
            bool boolValue;

            if(!bool.TryParse(value.ToString(), out boolValue)) throw new ArgumentException( "Passing an object that is not of bool type into a ModeTypeToSelectedConverter's converter-back value" );
            if( !Enum.TryParse<ModeTypes>( parameter.ToString(), out param ) ) throw new ArgumentException( "Passing an object that is not of ModeTypes type into a ModeTypeToSelectedConverter's converter parameter" );

            if( param == ModeTypes.Mode )
            {
                if( boolValue ) return ModeTypes.Mode;
                else return ModeTypes.Layout;
            }
            else if( param == ModeTypes.Layout )
            {
                if( boolValue ) return ModeTypes.Layout;
                else return ModeTypes.Mode;
            }

            return Binding.DoNothing;
        }
    }
}