using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.Controls
{
    public class ColoredLineEntry: Entry
    {
        public static BindableProperty BottomLineColorProperty = BindableProperty.Create(nameof(BottomLineColor), typeof(Color), typeof(ColoredLineEntry), Color.Black);

        public Color BottomLineColor
        {
            get => (Color)GetValue(BottomLineColorProperty);
            set => SetValue(BottomLineColorProperty, value);
        }
    }
}
