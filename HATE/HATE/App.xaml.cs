﻿using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace HATE
{
    public partial class App : Application
    {
        public static bool NeedMessageBox { get; set; }

        public App(bool IsMessageBox)
        {
            InitializeComponent();

            if(!IsMessageBox)
                MainPage = new MainPage();
            else
                MainPage = new MessageBox();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
