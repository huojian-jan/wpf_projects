// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1.Converters
{
    public class Color2SolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.ToString() == "Red")
                {
                    return new SolidColorBrush(Colors.Red);
                }else if(value.ToString() == "Green")
                {
                    return new SolidColorBrush(Colors.Green);
                }else if (value.ToString() == "Blue")
                {
                    return new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    return new SolidColorBrush(Colors.Black);
                }
            }
            return default;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
