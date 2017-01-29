using MifareApp_2._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MifareApp_2._0.Converters
{
    class MultibindingConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is bool  && values[1] is bool && values[2] is string)
            {
                if (((bool)values[1] == true) && (values[2] != null))
                {
                    return (!(bool)values[0]);
                }
                else if ((bool)values[1] == false)
                {
                    return true;
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
