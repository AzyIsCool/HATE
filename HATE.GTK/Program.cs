using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

namespace HATE.GTK
{
    public class Program
    {
        static App _app = null;
        static App _messageBox = null;

        [STAThread]
        public static void Main(string[] args)
        {
            Gtk.Application.Init();
            Forms.Init();

            _app = new App(false);
            MessageBoxTask();
            LoadWindow(_app);

            Gtk.Application.Run();
        }

        public static async Task<FormsWindow> LoadWindow(App app, int Width = 220, int Height = 485)
        {
            var window = new FormsWindow();
            window.LoadApplication(app);
            window.SetApplicationTitle("HATE");
            window.SetApplicationIcon("hateicon.png");

            if(HATE.Core.OS.WhatOperatingSystemUserIsOn == HATE.Core.OS.OperatingSystem.Linux)
                window.WidthRequest = Width;
            else if (HATE.Core.OS.WhatOperatingSystemUserIsOn == HATE.Core.OS.OperatingSystem.Windows)
                window.WidthRequest = Width - 15;
            else if (HATE.Core.OS.WhatOperatingSystemUserIsOn == HATE.Core.OS.OperatingSystem.macOS)
                window.WidthRequest = Width;

            window.DefaultWidth = window.WidthRequest;

            if(HATE.Core.OS.WhatOperatingSystemUserIsOn == HATE.Core.OS.OperatingSystem.Linux)
                window.HeightRequest = Height;
            else if (HATE.Core.OS.WhatOperatingSystemUserIsOn == HATE.Core.OS.OperatingSystem.Windows)
                window.HeightRequest = Height + 15;
            else if (HATE.Core.OS.WhatOperatingSystemUserIsOn == HATE.Core.OS.OperatingSystem.macOS)
                window.HeightRequest = Height - 30;
            window.DefaultHeight = window.HeightRequest;

            window.AllowGrow = false;
            window.AllowShrink = false;
            window.Show();
            return window;
        }

        public static async Task MessageBoxTask()
        {
            _messageBox = new App(true);
            FormsWindow formsWindow = await LoadWindow(_messageBox, 585, 135);
            formsWindow.Hide();
            while (true)
            {
                try
                {
                    while (!App.NeedMessageBox)
                    {
                        await Task.Delay(10);
                    }
                    //Gtk.Dialog dialog = new Gtk.Dialog("thing", null, Gtk.DialogFlags.Modal, "Ok");
                    //dialog.VBox.Add(new Gtk.Label(MessageBox._Message));
                    //dialog.ShowAll();
                    //dialog.DeleteEvent += Dialog_DeleteEvent;

                    await Task.Delay(500);
                    formsWindow.SetSizeRequest(585, (int)MessageBox._MessageHeight);
                    formsWindow.ShowAll();
                    if (!string.IsNullOrWhiteSpace(MessageBox._Title))
                        formsWindow.SetApplicationTitle(MessageBox._Title);

                    while (App.NeedMessageBox)
                    {
                        await Task.Delay(10);
                    }
                    formsWindow.Hide();
                }
                catch
                {
                    formsWindow.Destroy();
                    formsWindow = null;
                }
            }
        }

        //private static void Dialog_DeleteEvent(object o, Gtk.DeleteEventArgs args)
        //{
        //    App.NeedMessageBox = false;
        //    ((Gtk.Dialog)o).Destroy();
        //    ((Gtk.Dialog)o).DeleteEvent -= Dialog_DeleteEvent;
        //    o = null;
        //}
    }
}
