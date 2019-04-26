using System;
using Avalonia;
using System.IO;
using HATE.Core;
using Avalonia.Media;
using HATE.Core.Logging;
using Avalonia.Controls;
using System.Diagnostics;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace HATE
{
    partial class MainWindow : Window
    {
        private static class UIStyle
        {
            public static IBrush GetOptionColor(bool IsTicked)
            {
                return IsTicked ? Brushes.Yellow : Brushes.White;
            }

            public static IBrush GetCorruptColor(bool IsCorrupting)
            {
                return IsCorrupting ? Brushes.Coral : Brushes.LimeGreen;
            }

            public static string GetCorruptLabel(bool IsCorrupting)
            {
                return IsCorrupting ? "-CORRUPT-" : "-RESTORE-";
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Icon = new WindowIcon(await GetEmbeddedFile.GetFileStream("hateicon", "png"));

            //Set controls vars so we don't need to use this.FindControl every time we need to access a UI element
            btnCorrupt = this.FindControl<Button>("btnCorrupt");
            btnLaunch = this.FindControl<Button>("btnLaunch");
            chbShuffleText = this.FindControl<CheckBox>("chbShuffleText");
            chbShuffleGFX = this.FindControl<CheckBox>("chbShuffleGFX");
            chbHitboxFix = this.FindControl<CheckBox>("chbHitboxFix");
            chbShuffleFonts = this.FindControl<CheckBox>("chbShuffleFonts");
            chbShuffleSprites = this.FindControl<CheckBox>("chbShuffleSprites");
            chbShuffleAudio = this.FindControl<CheckBox>("chbShuffleAudio");
            chbShowSeed = this.FindControl<CheckBox>("chbShowSeed");
            chbGarbleText = this.FindControl<CheckBox>("chbGarbleText");
            labGameName = this.FindControl<TextBlock>("labGameName");
            labShuffleAudio = this.FindControl<TextBlock>("labShuffleAudio");
            labShuffleGFX = this.FindControl<TextBlock>("labShuffleGFX");
            labShuffleFonts = this.FindControl<TextBlock>("labShuffleFonts");
            labHitboxFix = this.FindControl<TextBlock>("labHitboxFix");
            labShuffleSprites = this.FindControl<TextBlock>("labShuffleSprites");
            labShuffleText = this.FindControl<TextBlock>("labShuffleText");
            labGarbleText = this.FindControl<TextBlock>("labGarbleText");
            labShowSeed = this.FindControl<TextBlock>("labShowSeed");
            labPower = this.FindControl<TextBlock>("labPower");
            labSeed = this.FindControl<TextBlock>("labSeed");
            txtPower = this.FindControl<TextBox>("txtPower");
            txtSeed = this.FindControl<TextBox>("txtSeed");

            FontFamily font = null;
            switch (OS.WhatOperatingSystemUserIsOn)
            {
                case OS.OperatingSystem.Linux:
                    font = FontFamily.Parse("TSCu_Comic");
                    break;
                case OS.OperatingSystem.Windows:
                case OS.OperatingSystem.macOS:
                    font = FontFamily.Parse("Comic Sans MS");
                    break;
            }

            FontFamily font2 = null;
            switch (OS.WhatOperatingSystemUserIsOn)
            {
                case OS.OperatingSystem.Linux:
                    font2 = FontFamily.Parse("TSCu_Comic");
                    break;
                case OS.OperatingSystem.Windows:
                case OS.OperatingSystem.macOS:
                    font2 = FontFamily.Parse("Papyrus");
                    break;
            }

            labShuffleAudio.FontFamily = font;
            labShuffleGFX.FontFamily = font;
            labShuffleFonts.FontFamily = font;
            labHitboxFix.FontFamily = font;
            labShuffleSprites.FontFamily = font;
            labShuffleText.FontFamily = font;
            labGarbleText.FontFamily = font;
            labShowSeed.FontFamily = font;
            txtSeed.FontFamily = font;
            txtPower.FontFamily = font;

            labSeed.FontFamily = font2;
            labPower.FontFamily = font2;
            btnLaunch.FontFamily = font2;
            btnCorrupt.FontFamily = font2;

            Logger.MessageHandle += Logger_SecondChange;

            //Set dataWin
            if (File.Exists(Main.GetFileLocation("game.ios")))
                Main.DataFile = "game.ios";
            else if (File.Exists(Main.GetFileLocation("game.unx")))
                Main.DataFile = "game.unx";
            else if (File.Exists("data.win"))
                Main.DataFile = "data.win";

            //Set dataWin based on OS if we didn't see a file
            if (string.IsNullOrWhiteSpace(Main.DataFile))
            {
                switch (OS.WhatOperatingSystemUserIsOn)
                {
                    case OS.OperatingSystem.macOS:
                        Main.DataFile = "game.ios";
                        break;
                    case OS.OperatingSystem.Linux:
                        Main.DataFile = "game.unx";
                        break;
                    case OS.OperatingSystem.Windows:
                    case OS.OperatingSystem.Unknown:
                        Main.DataFile = "data.win";
                        break;
                }
            }

            if (File.Exists("DELTARUNE.exe") || Directory.Exists("SURVEY_PROGRAM.app") ||
                Safe.IsValidFile(Main.GetFileLocation("options.ini")) &&
                File.ReadAllLines(Main.GetFileLocation("options.ini"))[1] == "DisplayName=\"SURVEY_PROGRAM\"")
            {
                labGameName.Text = "Deltarune";
            }
            else if (File.Exists("UNDERTALE.exe") || Directory.Exists("UNDERTALE.app") ||
                     Safe.IsValidFile(Main.GetFileLocation("options.ini")) &&
                     File.ReadAllLines(Main.GetFileLocation("options.ini"))[1] == "DisplayName=\"UNDERTALE\"")
            {
                labGameName.Text = "Undertale";
            }
            else
            {
                string game = Main.GetGame().Replace(".exe", "");
                if (!string.IsNullOrWhiteSpace(game))
                {
                    labGameName.Text = game;
                }

                if (!string.IsNullOrWhiteSpace(labGameName.Text))
                    Logger.Log(MessageType.Warning,
                        $"We couldn't find Deltarune or Undertale in this folder, if you're using this for another game then as long there is a {Main.DataFile} file and the game was made with GameMaker then this program should work but there are no guarantees that it will.",
                        true);
                else
                    Logger.Log(MessageType.Warning,
                        "We couldn't find any game in this folder, check that this is in the right folder.");
            }

            _random = new Random();

            UpdateCorrupt();

            //This is so it doesn't keep starting the program over and over in case something messes up with becoming a elevated program in Windows (Linux and macOS don't need elevated permissions)
            if (Process.GetProcessesByName("HATE").Length == 1)
            {
                Task.Run(() => BecomeElevated()).ConfigureAwait(false);
            }
        }

        private StreamWriter _logWriter;

        private bool _shuffleGFX = false;
        private bool _shuffleText = false;
        private bool _hitboxFix = false;
        private bool _shuffleFont = false;
        private bool _shuffleBG = false;
        private bool _shuffleAudio = false;
        private bool _garbleText = false;
        private bool _corrupt = false;
        private bool _showSeed = false;
        private float _truePower = 0;

        private static readonly DateTime _unixTimeZero = new DateTime(1970, 1, 1);
        private Random _random;

        public bool HasWriteAccess(string directory)
        {
            try
            {
                File.WriteAllText(Path.Combine(directory, "Wew"),
                    "Wew this is just HATE checking if we can write and delete to the drive (if you see this delete the file if you want to, it will no ill effect on HATE :p)");
                File.Delete(Path.Combine(directory, "Wew"));
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async Task BecomeElevated()
        {
            if (File.Exists(Main.GetFileLocation(Main.DataFile)) &&
                !HasWriteAccess(Main.GetFileLocation("Wew").Replace("/Wew", "")))
            {
                if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Windows)
                {
                    var dialogResult = MessageBox
                        .Show(
                            $"The game is in a protected folder and we need elevated permissions in order to mess with {Main.DataFile}, Do you allow us to get elevated permissions (if you press no this will just close the program as we can't do anything)",
                            MessageBox.MessageButton.YesNo, MessageBox.MessageIcon.Exclamation, this)
                        .ConfigureAwait(true).GetAwaiter().GetResult();
                    if (dialogResult == MessageBox.MessageResult.Yes)
                    {
                        //Restart program and run as admin
                        ProcessStartInfo startInfo = new ProcessStartInfo("HATE.exe")
                        {
                            Arguments = "true",
                            Verb = "runas" //What makes the program load with admin
                        };

                        Process.Start(startInfo);
                        Application.Current.Exit();
                    }
                    else
                    {
                        Application.Current.Exit();
                    }
                }
                else
                {
                    MessageBox.Show(
                            $"The game is in a protected folder and we need elevated permissions in order to mess with {Main.DataFile}. You need to open this with sudo (if you used a .sh file to open this make sure it says sudo ./HATE or linux and sudo ./HATE.app for macOS or if you opened this though terminal)",
                            MessageBox.MessageButton.OK, MessageBox.MessageIcon.Exclamation, this).ConfigureAwait(true)
                        .GetAwaiter().GetResult();
                    Application.Current.Exit();
                }
            }
        }

        private async void Logger_SecondChange(MessageEventArgs messageType)
        {
            switch (messageType.MessageType)
            {
                case MessageType.Debug when !messageType.WaitForAnything:
                    MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK,
                        MessageBox.MessageIcon.Information, this);
                    break;
                case MessageType.Debug:
                    await MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK,
                        MessageBox.MessageIcon.Information, this);
                    break;
                case MessageType.Warning when !messageType.WaitForAnything:
                    MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK,
                        MessageBox.MessageIcon.Exclamation, this);
                    break;
                case MessageType.Warning:
                    await MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK,
                        MessageBox.MessageIcon.Exclamation, this);
                    break;
                case MessageType.Error when !messageType.WaitForAnything:
                    MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Error,
                        this);
                    break;
                case MessageType.Error:
                    await MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK,
                        MessageBox.MessageIcon.Error, this);
                    break;
            }
        }

        private string UseWine()
        {
            return OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Linux ? "wine" : "";
        }

        private void btnLaunch_Clicked(object sender, EventArgs e)
        {
            EnableControls(false);

            ProcessStartInfo processStartInfo = null;
            if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Windows)
            {
                processStartInfo = new ProcessStartInfo(Main.GetGame())
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
            }
            else if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Linux)
            {
                if (Main.DataFile == "data.win")
                {
                    processStartInfo = new ProcessStartInfo(UseWine(), arguments: Main.GetGame())
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                }
                else if (Main.DataFile == "game.unx")
                {
                    processStartInfo = new ProcessStartInfo(Main.GetGame())
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                }
            }
            else if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.macOS)
            {
                processStartInfo = new ProcessStartInfo("open", $"-a '/Applications/{Main.GetGame()}'")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
            }
            else if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Unknown
            ) //Let try to load it like how we do with Windows, worst case is we'll get a Exception and fail to load it
            {
                processStartInfo = new ProcessStartInfo(Main.GetGame())
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
            }

            try
            {
                Process.Start(processStartInfo);
            }
            catch (Exception)
            {
                Logger.Log(MessageType.Error, $"Unable to launch {labGameName.Text}");
            }

            EnableControls(true);
        }

        private void button_Corrupt_Clicked(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                _logWriter = new StreamWriter("HATE.log", true);
            }
            catch (Exception)
            {
                Logger.Log(MessageType.Error, "Could not set up the log file.");
            }

            if (!Setup())
            {
                goto End;
            }

            ;
            //DebugListChunks(_dataWin, _logWriter);
            //Shuffle.LoadDataAndFind("STRG", _random, 0, _logWriter, _dataWin, Shuffle.ComplexShuffle(Shuffle.StringDumpAccumulator, Shuffle.SimpleShuffler, Shuffle.SimpleWriter));
            if (_hitboxFix && !Main.HitboxFix_Func(_random, _truePower, _logWriter))
            {
                goto End;
            }

            if (_shuffleGFX && !Main.ShuffleGFX_Func(_random, _truePower, _logWriter))
            {
                goto End;
            }

            if (_shuffleText && !Main.ShuffleText_Func(_random, _truePower, _logWriter, labGameName.Text))
            {
                goto End;
            }

            if (_shuffleFont && !Main.ShuffleFont_Func(_random, _truePower, _logWriter))
            {
                goto End;
            }

            if (_shuffleBG && !Main.ShuffleBG_Func(_random, _truePower, _logWriter))
            {
                goto End;
            }

            if (_shuffleAudio && !Main.ShuffleAudio_Func(_random, _truePower, _logWriter))
            {
                goto End;
            }

            End:
            _logWriter.Close();
            EnableControls(true);
        }

        private void EnableControls(bool state)
        {
            btnCorrupt.IsEnabled = state;
            btnLaunch.IsEnabled = state;
            chbShuffleText.IsEnabled = state;
            chbShuffleGFX.IsEnabled = state;
            chbHitboxFix.IsEnabled = state;
            chbShuffleFonts.IsEnabled = state;
            chbShuffleSprites.IsEnabled = state;
            chbShuffleAudio.IsEnabled = state;
            chbShowSeed.IsEnabled = state;
            txtPower.IsEnabled = state;
            txtSeed.IsEnabled = state;
        }

        private bool Setup()
        {
            _logWriter.WriteLine("-------------- Session at: " + DateTime.Now.ToLongDateString() + " " +
                                 DateTime.Now.ToLongTimeString() + "\n");

            /** SEED PARSING AND RNG SETUP **/
            Main.FriskMode = false;
            _random = new Random();
            int timeSeed = (int) DateTime.Now.Subtract(_unixTimeZero).TotalSeconds;

            if (!byte.TryParse(txtPower.Text, out byte power) && string.IsNullOrEmpty(txtPower.Text) && _corrupt)
            {
                Logger.Log(MessageType.Warning, "Please set Power to a number between 0 and 255 and try again.", true);
                return false;
            }

            _truePower = (float) power / 255;

            if (txtSeed.Text.ToUpper() == "FRISK" && (!File.Exists("DELTARUNE.exe") ||
                                                      !Directory.Exists("SURVEY_PROGRAM.app")))
                Main.FriskMode = true;

            txtSeed.Text = _showSeed ? $"#{timeSeed}" : "";

            _logWriter.WriteLine($"Time seed - {timeSeed}");
            _logWriter.WriteLine($"Power - {power}");
            _logWriter.WriteLine($"TruePower - {_truePower}");

            /** ENVIRONMENTAL CHECKS **/
            if (File.Exists("UNDERTALE.exe") || Directory.Exists("UNDERTALE.app") &&
                !File.Exists(Main.GetFileLocation("mus_barrier.ogg")))
            {
                Logger.Log(MessageType.Error,
                    "ERROR:\nIt seems you've either placed HATE.exe in the wrong location or are using an old version of Undertale. Solutions to both problems are given in the README.txt file included in the download.");
                return false;
            }

            if (!File.Exists(Main.GetFileLocation(Main.DataFile)))
            {
                Logger.Log(MessageType.Error,
                    $"You seem to be missing your resource file, {Main.DataFile}. Make sure you've placed HATE.exe in the proper location.");
                return false;
            }
            else if (!Directory.Exists("Data"))
            {
                if (!Safe.CreateDirectory("Data"))
                {
                    return false;
                }

                if (!Safe.CopyFile(Main.GetFileLocation(Main.DataFile),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data", Main.DataFile)))
                {
                    return false;
                }

                if (labGameName.Text == "Deltarune")
                {
                    if (!Safe.CreateDirectory(Path.Combine("Data", "lang")))
                    {
                        return false;
                    }

                    if (!Safe.CopyFile(Main.GetFileLocation(Path.Combine("lang", "lang_en.json")),
                        Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_en.json")))
                    {
                        return false;
                    }

                    ;
                    if (!Safe.CopyFile(Main.GetFileLocation(Path.Combine("lang", "lang_ja.json")),
                        Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_ja.json")))
                    {
                        return false;
                    }

                    ;
                }

                _logWriter.WriteLine($"Finished setting up the Data folder.");
            }

            if (!Safe.DeleteFile(Main.GetFileLocation(Main.DataFile)))
            {
                return false;
            }

            _logWriter.WriteLine($"Deleted {Main.DataFile}.");
            if (labGameName.Text == "Deltarune")
            {
                if (!Safe.DeleteFile(Main.GetFileLocation(Path.Combine("lang", "lang_en.json"))))
                {
                    return false;
                }

                _logWriter.WriteLine($"Deleted ./lang/lang_en.json.");
                if (!Safe.DeleteFile(Main.GetFileLocation(Path.Combine("lang", "lang_ja.json"))))
                {
                    return false;
                }

                _logWriter.WriteLine($"Deleted ./lang/lang_ja.json.");
            }

            if (!Safe.CopyFile(Path.Combine(Directory.GetCurrentDirectory(), "Data", Main.DataFile),
                Main.GetFileLocation(Main.DataFile)))
            {
                return false;
            }

            _logWriter.WriteLine($"Copied {Main.DataFile}.");
            if (labGameName.Text == "Deltarune")
            {
                if (!Safe.CopyFile(Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_en.json"),
                    Main.GetFileLocation(Path.Combine("lang", "lang_en.json"))))
                {
                    return false;
                }

                ;
                _logWriter.WriteLine($"Copied ./lang/lang_en.json.");
                if (!Safe.CopyFile(Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_ja.json"),
                    Main.GetFileLocation(Path.Combine("lang", "lang_ja.json"))))
                {
                    return false;
                }

                ;
                _logWriter.WriteLine($"Copied ./lang/lang_ja.json.");
            }

            return true;
        }

        public void UpdateCorrupt()
        {
            _corrupt = _shuffleGFX || _shuffleText || _hitboxFix || _shuffleFont || _shuffleAudio || _shuffleBG ||
                       _garbleText;
            btnCorrupt.Content = UIStyle.GetCorruptLabel(_corrupt);
            btnCorrupt.Foreground = UIStyle.GetCorruptColor(_corrupt);
            btnCorrupt.BorderBrush = btnCorrupt.Foreground;
        }

        private void chbShuffleAudio_Toggled(object sender, RoutedEventArgs e)
        {
            _shuffleAudio = chbShuffleAudio.IsChecked.Value;
            labShuffleAudio.Foreground = UIStyle.GetOptionColor(_shuffleAudio);
            chbShuffleAudio.BorderBrush = labShuffleAudio.Foreground;
            UpdateCorrupt();
        }

        private void chbShuffleGFX_Toggled(object sender, RoutedEventArgs e)
        {
            _shuffleGFX = chbShuffleGFX.IsChecked.Value;
            labShuffleGFX.Foreground = UIStyle.GetOptionColor(_shuffleGFX);
            chbShuffleGFX.BorderBrush = labShuffleGFX.Foreground;
            UpdateCorrupt();
        }

        private void chbShuffleFonts_Toggled(object sender, RoutedEventArgs e)
        {
            _shuffleFont = chbShuffleFonts.IsChecked.Value;
            labShuffleFonts.Foreground = UIStyle.GetOptionColor(_shuffleFont);
            chbShuffleFonts.BorderBrush = labShuffleFonts.Foreground;
            UpdateCorrupt();
        }

        private void chbHitboxFix_Toggled(object sender, RoutedEventArgs e)
        {
            _hitboxFix = chbHitboxFix.IsChecked.Value;
            labHitboxFix.Foreground = UIStyle.GetOptionColor(_hitboxFix);
            chbHitboxFix.BorderBrush = labHitboxFix.Foreground;
            UpdateCorrupt();
        }

        private void chbShuffleSprites_Toggled(object sender, RoutedEventArgs e)
        {
            _shuffleBG = chbShuffleSprites.IsChecked.Value;
            labShuffleSprites.Foreground = UIStyle.GetOptionColor(_shuffleBG);
            chbShuffleSprites.BorderBrush = labShuffleSprites.Foreground;
            UpdateCorrupt();
        }

        private void chbShuffleText_Toggled(object sender, RoutedEventArgs e)
        {
            _shuffleText = chbShuffleText.IsChecked.Value;
            labShuffleText.Foreground = UIStyle.GetOptionColor(_shuffleText);
            chbShuffleText.BorderBrush = labShuffleText.Foreground;
            UpdateCorrupt();
        }

        private void chbGarbleText_Toggled(object sender, RoutedEventArgs e)
        {
            _garbleText = chbGarbleText.IsChecked.Value;
            labGarbleText.Foreground = UIStyle.GetOptionColor(_garbleText);
            chbGarbleText.BorderBrush = labGarbleText.Foreground;
            UpdateCorrupt();
        }

        private void chbShowSeed_Toggled(object sender, RoutedEventArgs e)
        {
            TextBlock thing = new TextBlock();
            thing.PointerPressed += labPointerPressed;
            _showSeed = chbShowSeed.IsChecked.Value;
            labShowSeed.Foreground = UIStyle.GetOptionColor(_showSeed);
            chbShowSeed.BorderBrush = labShowSeed.Foreground;
        }

        private void labPointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            switch (((TextBlock)sender).Name)
            {
                case "labGarbleText":
                    chbGarbleText.IsChecked = !chbGarbleText.IsChecked;
                    chbGarbleText.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labHitboxFix":
                    chbHitboxFix.IsChecked = !chbHitboxFix.IsChecked;
                    chbHitboxFix.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labShowSeed":
                    chbShowSeed.IsChecked = !chbShowSeed.IsChecked;
                    chbShowSeed.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labShuffleAudio":
                    chbShuffleAudio.IsChecked = !chbShuffleAudio.IsChecked;
                    chbShuffleAudio.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labShuffleFonts":
                    chbShuffleFonts.IsChecked = !chbShuffleFonts.IsChecked;
                    chbShuffleFonts.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labShuffleGFX":
                    chbShuffleGFX.IsChecked = !chbShuffleGFX.IsChecked;
                    chbShuffleGFX.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labShuffleSprites":
                    chbShuffleSprites.IsChecked = !chbShuffleSprites.IsChecked;
                    chbShuffleSprites.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
                case "labShuffleText":
                    chbShuffleText.IsChecked = !chbShuffleText.IsChecked;
                    chbShuffleText.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
                    break;
            }
        }
    }
}