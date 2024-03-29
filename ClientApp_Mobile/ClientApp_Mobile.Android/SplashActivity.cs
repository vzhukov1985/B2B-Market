﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace ClientApp_Mobile.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
#pragma warning disable IDE0052 // Remove unread private members
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;
#pragma warning restore IDE0052 // Remove unread private members

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { StartMainActivity(); });
            startupWork.Start();
        }

        // Simulates background work that happens behind the splash screen
        void StartMainActivity()
        {
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}