using XClientApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace XClientApp_Win.Converters
{
    public class ProductOrderAndRemainsStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ProductOrderAndRemainsState)value) switch
            {
                ProductOrderAndRemainsState.OneSupplierNullRemains => "ВНИМАНИЕ! Товар закончился у поставщика. Удалите эту позицию!",
                ProductOrderAndRemainsState.AllSuppliersNullRemains => "ВНИМАНИЕ! Товар закончился у всех поставщиков. Удалите эту позицию!",
                ProductOrderAndRemainsState.OneSupplierLessRemains => "ВНИМАНИЕ! Поставщик не может обеспечить указанное количество товара. Измение заявку!",
                ProductOrderAndRemainsState.OneOfSuppliersLessRemains => "ВНИМАНИЕ! Один из поставщиков не может обеспечить указанное количество товара. Измение заявку!",
                _ => "",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}