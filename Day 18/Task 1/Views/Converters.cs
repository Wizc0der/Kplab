using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Task_1.Views
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "booked":
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    case "available":
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                    case "maintenance":
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                    default:
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
                }
            }
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}