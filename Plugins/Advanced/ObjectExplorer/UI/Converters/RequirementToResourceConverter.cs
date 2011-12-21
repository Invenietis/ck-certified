using System;
using System.Windows.Data;
using System.Globalization;
using CK.Plugin;
using CK.StandardPlugins.ObjectExplorer.UI.Resources;

namespace CK.StandardPlugins.ObjectExplorer
{    
    public class RequirementToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RunningRequirement requirement = (RunningRequirement)value;
            switch( requirement )
            {
                case RunningRequirement.MustExist:
                    return R.MustExist;
                case RunningRequirement.MustExistAndRun:
                    return R.MustExistAndRun;
                case RunningRequirement.MustExistTryStart:
                    return R.MustExistTryStart;
                case RunningRequirement.Optional:
                    return R.Optional;
                case RunningRequirement.OptionalTryStart:
                    return R.OptionalTryStart;
                default:
                    return "Unknow Requirement";                    
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool param = bool.Parse(parameter.ToString());
            return !((bool)value ^ param);
        }
    }
}
