using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace HelloMemo.Converters
{
    //--------------------------------------------------------------------------------------------------
    // Для коллекции (ObservableCollection<string>)value возвращает элемент с индексом (int)parameter. По умолчанию parameter=0.
    public class GetErrorByIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i;
            if (parameter == null) i = 0;
            else i = System.Convert.ToInt32(parameter);
            return ((ObservableCollection<string>)value)?[i];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    //--------------------------------------------------------------------------------------------------
    public class CollectionOfSampleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            string str = "";
            ICollection<DataModel.Sample> samples = value as ICollection<DataModel.Sample>;
            foreach (DataModel.Sample sample in samples)
            {
                str = (String.IsNullOrEmpty(str) ? "" : (str + "\n")) + sample.Phrase + "  -  " + sample.Trans + "\n";
            }
            return str;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    //--------------------------------------------------------------------------------------------------
    // Если value!=null, то возвращает true. Иначе false. 
    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null)?false:true;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    //--------------------------------------------------------------------------------------------------
    // Если value==true, то возвращает 1. Иначе 0. 
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? 1 : 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------
}
