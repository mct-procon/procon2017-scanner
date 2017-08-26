using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PuzzleSupporter.Converters {
    public class DetectedStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch (value) {
                case bool v:
                    if (targetType == typeof(string))
                        return v ? "Detected" : "Not Detected";
                    else
                        return v ? Visibility.Visible : Visibility.Hidden;
                default:
                    if (targetType == typeof(string))
                        return "Something wrong...";
                    else
                        return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
