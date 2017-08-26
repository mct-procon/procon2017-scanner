using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PuzzleSupporter.Converters {
    public class MaximizeNormalizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is WindowState)
                return (WindowState)value == WindowState.Normal ? "1" : "2";
            else
                throw new System.FormatException("Bad Type. Need WindowState Typed Value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
