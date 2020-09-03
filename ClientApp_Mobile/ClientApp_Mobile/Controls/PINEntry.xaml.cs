using ClientApp_Mobile.ViewModels;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PINEntry : ContentView
    {
        public static readonly BindableProperty PINCodeProperty = BindableProperty.Create(nameof(PINCode), typeof(string), typeof(PINEntry), string.Empty, BindingMode.TwoWay, coerceValue: CoercePIN, propertyChanged: OnPINChanged);
        public static readonly BindableProperty UnfilledLineColorProperty = BindableProperty.Create(nameof(UnfilledLineColor), typeof(Color), typeof(PINEntry), Color.Black);
        public static readonly BindableProperty FilledLineColorProperty = BindableProperty.Create(nameof(FilledLineColor), typeof(Color), typeof(PINEntry), Color.Black);
        public static readonly BindableProperty IsPINWrongProperty = BindableProperty.Create(nameof(IsPINWrong), typeof(bool), typeof(PINEntry), false, propertyChanged: PINWrongStartAnimation);

        public bool IsPINWrong
        {
            get => (bool)GetValue(IsPINWrongProperty);
            set => SetValue(IsPINWrongProperty, value);
        }

        public string PINCode
        {
            get => (string)GetValue(PINCodeProperty);
            set => SetValue(PINCodeProperty, value);
        }

        public Color UnfilledLineColor
        {
            get => (Color)GetValue(UnfilledLineColorProperty);
            set => SetValue(UnfilledLineColorProperty, value);
        }

        public Color FilledLineColor
        {
            get => (Color)GetValue(FilledLineColorProperty);
            set => SetValue(FilledLineColorProperty, value);
        }

        private static object CoercePIN(BindableObject bindable, object value)
        {
            if (value == null) return string.Empty;
            if (value.ToString().Length > 4)
                return value.ToString().Substring(0, 4);
            else
                return value;
        }

        private static void OnPINChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var pinobj = (PINEntry)bindable;
            if (newValue.ToString().Length == 0)
            {
                pinobj.Entry1.Text = "";
                pinobj.Entry1.BottomLineColor = pinobj.UnfilledLineColor;
                pinobj.Entry2.Text = "";
                pinobj.Entry2.BottomLineColor = pinobj.UnfilledLineColor;
                pinobj.Entry3.Text = "";
                pinobj.Entry3.BottomLineColor = pinobj.UnfilledLineColor;
                pinobj.Entry4.Text = "";
                pinobj.Entry4.BottomLineColor = pinobj.UnfilledLineColor;
            }
            if (newValue.ToString().Length == 1)
            {
                pinobj.Entry1.Text = ((string)newValue).Substring(0, 1);
                pinobj.Entry1.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry2.Text = "";
                pinobj.Entry2.BottomLineColor = pinobj.UnfilledLineColor;
                pinobj.Entry3.Text = "";
                pinobj.Entry3.BottomLineColor = pinobj.UnfilledLineColor;
                pinobj.Entry4.Text = "";
                pinobj.Entry4.BottomLineColor = pinobj.UnfilledLineColor;
            }
            if (newValue.ToString().Length == 2)
            {
                pinobj.Entry1.Text = ((string)newValue).Substring(0, 1);
                pinobj.Entry1.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry2.Text = ((string)newValue).Substring(1, 1);
                pinobj.Entry2.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry3.Text = "";
                pinobj.Entry3.BottomLineColor = pinobj.UnfilledLineColor;
                pinobj.Entry4.Text = "";
                pinobj.Entry4.BottomLineColor = pinobj.UnfilledLineColor;
            }
            if (newValue.ToString().Length == 3)
            {
                pinobj.Entry1.Text = ((string)newValue).Substring(0, 1);
                pinobj.Entry1.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry2.Text = ((string)newValue).Substring(1, 1);
                pinobj.Entry2.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry3.Text = ((string)newValue).Substring(2, 1);
                pinobj.Entry3.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry4.Text = "";
                pinobj.Entry4.BottomLineColor = pinobj.UnfilledLineColor;
            }
            if (newValue.ToString().Length == 4)
            {
                pinobj.Entry1.Text = ((string)newValue).Substring(0, 1);
                pinobj.Entry1.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry2.Text = ((string)newValue).Substring(1, 1);
                pinobj.Entry2.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry3.Text = ((string)newValue).Substring(2, 1);
                pinobj.Entry3.BottomLineColor = pinobj.FilledLineColor;
                pinobj.Entry4.Text = ((string)newValue).Substring(3, 1);
                pinobj.Entry4.BottomLineColor = pinobj.FilledLineColor;
            }
        }

        private static async void PINWrongStartAnimation(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bool)newValue == true)
            {
                var obj = (PINEntry)bindable;
                await obj.TranslateTo(3, 0, 25);
                await obj.TranslateTo(-6, 0, 25);
                await obj.TranslateTo(6, 0, 25);
                await obj.TranslateTo(-6, 0, 25);
                await obj.TranslateTo(6, 0, 25);
                await obj.TranslateTo(-6, 0, 25);
                await obj.TranslateTo(6, 0, 25);
                await obj.TranslateTo(-6, 0, 25);
                await obj.TranslateTo(3, 0, 25);
                obj.SetValue(PINCodeProperty, "");
            }
        }

        public PINEntry()
        {
            InitializeComponent();
        }
    }
}