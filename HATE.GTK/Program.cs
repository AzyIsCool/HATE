using System;
using HATE.UI;
using System.IO;
using Xamarin.Forms;
using System.Threading.Tasks;
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

            try
            {
                Gtk.Application.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                File.AppendAllText("HATE.log", $"Program crash (Likely that GTK# had a moment):\r\n{e}");
            }
        }

        public static async Task<FormsWindow> LoadWindow(App app, int Width = 220, int Height = 485)
        {
            var window = new FormsWindow();
            window.LoadApplication(app);
            window.SetApplicationTitle("HATE");
            window.SetApplicationIcon("hateicon.png");

            if(Core.OS.WhatOperatingSystemUserIsOn == Core.OS.OperatingSystem.Linux)
                window.WidthRequest = Width;
            else if (Core.OS.WhatOperatingSystemUserIsOn == Core.OS.OperatingSystem.Windows)
                window.WidthRequest = Width;
            else if (Core.OS.WhatOperatingSystemUserIsOn == Core.OS.OperatingSystem.macOS)
                window.WidthRequest = Width;

            window.DefaultWidth = window.WidthRequest;

            if (Core.OS.WhatOperatingSystemUserIsOn == Core.OS.OperatingSystem.Linux)
                window.HeightRequest = Height - 2;
            else if (Core.OS.WhatOperatingSystemUserIsOn == Core.OS.OperatingSystem.Windows)
                window.HeightRequest = Height + 15;
            else if (Core.OS.WhatOperatingSystemUserIsOn == Core.OS.OperatingSystem.macOS)
                window.HeightRequest = Height - 40;
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
                    await Task.Delay(510);
                    formsWindow.SetSizeRequest(585, (int)MessageBox._MessageHeight);
                    formsWindow.ShowAll();
                    if (!string.IsNullOrWhiteSpace(MessageBox._Title))
                        formsWindow.SetApplicationTitle(MessageBox._Title);

                    while (App.NeedMessageBox)
                    {
                        await Task.Delay(10);
                    }
                    formsWindow.Hide();
                    MessageBox._MessageHeight = 135;
                }
                catch
                {
                    formsWindow.Destroy();
                    formsWindow = null;
                }
            }
        }
    }
}
