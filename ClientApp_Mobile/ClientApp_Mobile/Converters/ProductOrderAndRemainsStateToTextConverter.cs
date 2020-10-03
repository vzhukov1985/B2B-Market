using ClientApp_Mobile.ViewModels.SubPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.Converters
{
    public class ProductOrderAndRemainsStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ProductOrderAndRemainsState)value)
            {
                case ProductOrderAndRemainsState.OneSupplierNullRemains: return "ВНИМАНИЕ! Товар закончился у поставщика. Удалите эту позицию!";
                case ProductOrderAndRemainsState.AllSuppliersNullRemains: return "ВНИМАНИЕ! Товар закончился у всех поставщиков. Удалите эту позицию!";
                case ProductOrderAndRemainsState.OneSupplierLessRemains: return "ВНИМАНИЕ! Поставщик не может обеспечить указанное кол-во товара. Измените заявку!";
                case ProductOrderAndRemainsState.OneOfSuppliersLessRemains: return "ВНИМАНИЕ! Поставщик не может обеспечить указанное кол-во товара. Измените заявку!";
                default: return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}