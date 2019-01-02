﻿using System;
using Xamarin.Forms;
using HATE.Core;
using HATE.Core.Logging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HATE.UI
{
    public partial class MainPage : ContentPage
    {
        private static class UIStyle
        {
            public static Color GetOptionColor(bool IsTicked) { return IsTicked ? Color.Yellow : Color.White; }
            public static Color GetCorruptColor(bool IsCorrupting) { return IsCorrupting ? Color.Coral : Color.LimeGreen; }
            public static string GetCorruptLabel(bool IsCorrupting) { return IsCorrupting ? "-CORRUPT-" : "-RESTORE-"; }
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

        //TODO: Find a way to get labels to have a Click event
        public MainPage()
        {
            InitializeComponent();

            Logger.MessageHandle += Logger_SecondChange;

            if (File.Exists(Main.GetFileLocation("game.ios")))
                Main._dataWin = "game.ios";
            else if (File.Exists(Main.GetFileLocation("game.unx")))
                Main._dataWin = "game.unx";
            else if (File.Exists("data.win"))
                Main._dataWin = "data.win";

            if (string.IsNullOrWhiteSpace(Main._dataWin))
            {
                if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.macOS)
                    Main._dataWin = "game.ios";
                else if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Linux)
                    Main._dataWin = "game.unx";
                else if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Windows || OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Unknown)
                    Main._dataWin = "data.win";
            }

            if (File.Exists("DELTARUNE.exe") || Directory.Exists("SURVEY_PROGRAM.app") || Safe.IsValidFile(Main.GetFileLocation("options.ini")) && File.ReadAllLines(Main.GetFileLocation("options.ini"))[1] == "DisplayName=\"SURVEY_PROGRAM\"") { labGameName.Text = "Deltarune"; }
            else if (File.Exists("UNDERTALE.exe") || Directory.Exists("UNDERTALE.app") || Safe.IsValidFile(Main.GetFileLocation("options.ini")) && File.ReadAllLines(Main.GetFileLocation("options.ini"))[1] == "DisplayName=\"UNDERTALE\"") { labGameName.Text = "Undertale"; }
            else
            {
                string game = Main.GetGame().Replace(".exe", "");
                if (!string.IsNullOrWhiteSpace(game)) { labGameName.Text = game; }
                if (!string.IsNullOrWhiteSpace(labGameName.Text))
                    Logger.Log(MessageType.Warning, $"We couldn't find Deltarune or Undertale in this folder, if you're using this for another game then as long there is a {Main._dataWin} file and the game was made with GameMaker then this program should work but there are no guarantees that it will.", true);
                else
                    Logger.Log(MessageType.Warning, "We couldn't find any game in this folder, check that this is in the right folder.");
            }

            _random = new Random();

            UpdateCorrupt();

            //This is so it doesn't keep starting the program over and over in case something messes up with becoming a elevated program in Windows (Linux and macOS don't need elevated permissions)
            if (Process.GetProcessesByName("HATE").Length == 1)
            {
                Task.Run(() => BecomeElevated()).ConfigureAwait(false);
            }
        }

        public bool HasWriteAccess(string directory)
        {
            try
            {
                File.WriteAllText(Path.Combine(directory, "Wew"), "Wew this is just HATE checking if we can write and delete to the drive");
                File.Delete(Path.Combine(directory, "Wew"));
            }
            catch { return false; }
            return true;
        }

        public async Task BecomeElevated()
        {
            if (File.Exists(Main.GetFileLocation(Main._dataWin)) && !HasWriteAccess(Main.GetFileLocation("Wew").Replace("/Wew", "")))
            {
                if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Windows)
                {
                    var dialogResult = MessageBox.Show($"The game is in a protected folder and we need elevated permissions in order to mess with {Main._dataWin}, Do you allow us to get elevated permissions (if you press no this will just close the program as we can't do anything)", MessageBox.MessageButton.YesNo, MessageBox.MessageIcon.Exclamation).ConfigureAwait(true).GetAwaiter().GetResult();
                    if (dialogResult == MessageBox.MessageResult.Yes)
                    {
                        //Restart program and run as admin
                        var exeName = "HATE.exe";
                        ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                        {
                            Arguments = "true",
                            Verb = "runas" //What makes the program load with admin
                        };

                        Process.Start(startInfo);
                        Application.Current.Quit();
                    }
                    else
                    {
                        Application.Current.Quit();
                    }
                }
                else 
                {
                    MessageBox.Show($"The game is in a protected folder and we need elevated permissions in order to mess with {Main._dataWin}. You need to open this with sudo (if you used a .sh file to open this make sure it says sudo mono HATE.exe and if you opened this though bash)", MessageBox.MessageButton.OK, MessageBox.MessageIcon.Exclamation).ConfigureAwait(true).GetAwaiter().GetResult();
                    Application.Current.Quit();
                }
            }
        }

        private async void Logger_SecondChange(MessageEventArgs messageType)
        {
            if (messageType.MessageType == MessageType.Debug)
            {
                if (!messageType.WaitForAnything)
                    MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Information);
                else
                    await MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Information);
            }
            else if (messageType.MessageType == MessageType.Warning)
            {
                if (!messageType.WaitForAnything)
                    MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Exclamation);
                else
                    await MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Exclamation);
            }
            else if (messageType.MessageType == MessageType.Error)
            {
                if (!messageType.WaitForAnything)
                    MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Error);
                else
                    await MessageBox.Show(messageType.Message, MessageBox.MessageButton.OK, MessageBox.MessageIcon.Error);
            }
        }

        public string LinuxWine()
        {
            if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Linux)
                return "wine";
            else
                return "";
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
                if (Main._dataWin == "data.win") 
                {
                    processStartInfo = new ProcessStartInfo(LinuxWine(), arguments: Main.GetGame())
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                }
                else if (Main._dataWin == "game.unx")
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
            else if (OS.WhatOperatingSystemUserIsOn == OS.OperatingSystem.Unknown) //Let try to load it like how we do with Windows, worst case is we'll get a Exception and fail to load it
            {
                processStartInfo = new ProcessStartInfo(Main.GetGame())
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
            }

            try { Process.Start(processStartInfo); }
            catch (Exception) { Logger.Log(MessageType.Error, $"Unable to launch {labGameName.Text}"); }

            EnableControls(true);
        }

        private void button_Corrupt_Clicked(object sender, EventArgs e)
        {
            EnableControls(false);

            try { _logWriter = new StreamWriter("HATE.log", true); }
            catch (Exception) { Logger.Log(MessageType.Error, "Could not set up the log file."); }

            if (!Setup()) { goto End; };
            //DebugListChunks(_dataWin, _logWriter);
            //Shuffle.LoadDataAndFind("STRG", _random, 0, _logWriter, _dataWin, Shuffle.ComplexShuffle(Shuffle.StringDumpAccumulator, Shuffle.SimpleShuffler, Shuffle.SimpleWriter));
            if (_hitboxFix && !Main.HitboxFix_Func(_random, _truePower, _logWriter)) { goto End; }
            if (_shuffleGFX && !Main.ShuffleGFX_Func(_random, _truePower, _logWriter)) { goto End; }
            if (_shuffleText && !Main.ShuffleText_Func(_random, _truePower, _logWriter, labGameName.Text)) { goto End; }
            if (_shuffleFont && !Main.ShuffleFont_Func(_random, _truePower, _logWriter)) { goto End; }
            if (_shuffleBG && !Main.ShuffleBG_Func(_random, _truePower, _logWriter)) { goto End; }
            if (_shuffleAudio && !Main.ShuffleAudio_Func(_random, _truePower, _logWriter)) { goto End; }

            End:
            _logWriter.Close();
            EnableControls(true);
        }

        public void EnableControls(bool state)
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

        public bool Setup()
        {
            _logWriter.WriteLine("-------------- Session at: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "\n");

            /** SEED PARSING AND RNG SETUP **/
            Main._friskMode = false;
            _random = new Random();
            int timeSeed = (int)DateTime.Now.Subtract(_unixTimeZero).TotalSeconds;

            if (!byte.TryParse(txtPower.Text, out byte power) && string.IsNullOrEmpty(txtPower.Text) && _corrupt)
            {
                Logger.Log(MessageType.Warning, "Please set Power to a number between 0 and 255 and try again.", true);
                return false;
            }
            _truePower = (float)power / 255;

            if (txtSeed.Text.ToUpper() == "FRISK" && !File.Exists("DELTARUNE.exe") || Directory.Exists("SURVEY_PROGRAM.app"))
                Main._friskMode = true;

            if (_showSeed)
                txtSeed.Text = $"#{timeSeed}";
            else
                txtSeed.Text = "";

            _logWriter.WriteLine($"Time seed - {timeSeed}");
            _logWriter.WriteLine($"Power - {power}");
            _logWriter.WriteLine($"TruePower - {_truePower}");

            /** ENVIRONMENTAL CHECKS **/
            if (File.Exists("UNDERTALE.exe") || Directory.Exists("UNDERTALE.app") && !File.Exists(Main.GetFileLocation("mus_barrier.ogg")))
            {
                Logger.Log(MessageType.Error, "ERROR:\nIt seems you've either placed HATE.exe in the wrong location or are using an old version of Undertale. Solutions to both problems are given in the README.txt file included in the download.");
                return false;
            }

            if (!File.Exists(Main.GetFileLocation(Main._dataWin)))
            {
                Logger.Log(MessageType.Error, $"You seem to be missing your resource file, {Main._dataWin}. Make sure you've placed HATE.exe in the proper location.");
                return false;
            }
            else if (!Directory.Exists("Data"))
            {
                if (!Safe.CreateDirectory("Data")) { return false; }
                if (!Safe.CopyFile(Main.GetFileLocation(Main._dataWin), Path.Combine(Directory.GetCurrentDirectory(), "Data", Main._dataWin))) { return false; }
                if (labGameName.Text == "Deltarune")
                {
                    if (!Safe.CreateDirectory(Path.Combine("Data", "lang"))) { return false; }
                    if (!Safe.CopyFile(Main.GetFileLocation(Path.Combine("lang", "lang_en.json")), Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_en.json"))) { return false; };
                    if (!Safe.CopyFile(Main.GetFileLocation(Path.Combine("lang","lang_ja.json")), Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_ja.json"))) { return false; };
                }
                _logWriter.WriteLine($"Finished setting up the Data folder.");
            }

            if (!Safe.DeleteFile(Main.GetFileLocation(Main._dataWin))) { return false; }
            _logWriter.WriteLine($"Deleted {Main._dataWin}.");
            if (labGameName.Text == "Deltarune")
            {
                if (!Safe.DeleteFile(Main.GetFileLocation(Path.Combine("lang", "lang_en.json")))) { return false; }
                _logWriter.WriteLine($"Deleted ./lang/lang_en.json.");
                if (!Safe.DeleteFile(Main.GetFileLocation(Path.Combine("lang", "lang_ja.json")))) { return false; }
                _logWriter.WriteLine($"Deleted ./lang/lang_ja.json.");
            }

            if (!Safe.CopyFile(Path.Combine(Directory.GetCurrentDirectory(), "Data", Main._dataWin), Main.GetFileLocation(Main._dataWin))) { return false; }
            _logWriter.WriteLine($"Copied {Main._dataWin}.");
            if (labGameName.Text == "Deltarune")
            {
                if (!Safe.CopyFile(Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_en.json"), Main.GetFileLocation(Path.Combine("lang", "lang_en.json")))) { return false; };
                _logWriter.WriteLine($"Copied ./lang/lang_en.json.");
                if (!Safe.CopyFile(Path.Combine(Directory.GetCurrentDirectory(), "Data", "lang", "lang_ja.json"), Main.GetFileLocation(Path.Combine("lang", "lang_ja.json")))) { return false; };
                _logWriter.WriteLine($"Copied ./lang/lang_ja.json.");
            }

            return true;
        }

        public void UpdateCorrupt()
        {
            _corrupt = _shuffleGFX || _shuffleText || _hitboxFix || _shuffleFont || _shuffleAudio || _shuffleBG || _garbleText;
            btnCorrupt.Text = UIStyle.GetCorruptLabel(_corrupt);
            btnCorrupt.TextColor = UIStyle.GetCorruptColor(_corrupt);
            btnCorrupt.BorderColor = btnCorrupt.TextColor;
        }

        private void chbShuffleAudio_Toggled(object sender, ToggledEventArgs e)
        {
            _shuffleAudio = chbShuffleAudio.IsToggled;
            labShuffleAudio.TextColor = UIStyle.GetOptionColor(_shuffleAudio);
            UpdateCorrupt();
        }

        private void chbShuffleGFX_Toggled(object sender, ToggledEventArgs e)
        {
            _shuffleGFX = chbShuffleGFX.IsToggled;
            labShuffleGFX.TextColor = UIStyle.GetOptionColor(_shuffleGFX);
            UpdateCorrupt();
        }

        private void chbShuffleFonts_Toggled(object sender, ToggledEventArgs e)
        {
            _shuffleFont = chbShuffleFonts.IsToggled;
            labShuffleFonts.TextColor = UIStyle.GetOptionColor(_shuffleFont);
            UpdateCorrupt();
        }

        private void chbHitboxFix_Toggled(object sender, ToggledEventArgs e)
        {
            _hitboxFix = chbHitboxFix.IsToggled;
            labHitboxFix.TextColor = UIStyle.GetOptionColor(_hitboxFix);
            UpdateCorrupt();
        }

        private void chbShuffleSprites_Toggled(object sender, ToggledEventArgs e)
        {
            _shuffleBG = chbShuffleSprites.IsToggled;
            labShuffleSprites.TextColor = UIStyle.GetOptionColor(_shuffleBG);
            UpdateCorrupt();
        }

        private void chbShuffleText_Toggled(object sender, ToggledEventArgs e)
        {
            _shuffleText = chbShuffleText.IsToggled;
            labShuffleText.TextColor = UIStyle.GetOptionColor(_shuffleText);
            UpdateCorrupt();
        }

        private void chbGarbleText_Toggled(object sender, ToggledEventArgs e)
        {
            _garbleText = chbGarbleText.IsToggled;
            labGarbleText.TextColor = UIStyle.GetOptionColor(_garbleText);
            UpdateCorrupt();
        }

        private void chbShowSeed_Toggled(object sender, ToggledEventArgs e)
        {
            _showSeed = chbShowSeed.IsToggled;
        }
    }
}
