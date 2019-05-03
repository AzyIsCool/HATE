using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;

namespace HATE
{
    internal partial class MessageBox : UserControl
    {
        public enum MessageButton
        {
            AbortRetryIgnore = 2,
            OK = 0,
            OKCancel = 1,
            RetryCancel = 5,
            YesNo = 4,
            YesNoCancel = 3
        }

        public enum MessageIcon
        {
            Asterisk = 64,
            Error = 16,
            Exclamation = 48,
            Hand = 16,
            Information = 64,
            None = 0,
            Question = 32,
            Stop = 16,
            Warning = 48
        }

        public enum MessageResult
        {
            Abort = 3,
            Cancel = 2,
            Ignore = 5,
            No = 7,
            None = 0,
            OK = 1,
            Retry = 4,
            Yes = 6
        }

        private MessageBox(MessageBoxOptions messageBoxOptions)
        {
            InitializeComponent(messageBoxOptions);
        }

        private MessageResult Result { get; set; }

        private void InitializeComponent(MessageBoxOptions messageBoxOptions)
        {
            AvaloniaXamlLoader.Load(this);

            labMessage = this.FindControl<TextBlock>("labMessage");
            butNo = this.FindControl<Button>("butNo");
            butOK = this.FindControl<Button>("butOK");
            butYes = this.FindControl<Button>("butYes");
            butAbort = this.FindControl<Button>("butAbort");
            butRetry = this.FindControl<Button>("butRetry");
            butCancel = this.FindControl<Button>("butCancel");
            butIgnore = this.FindControl<Button>("butIgnore");
            gridImageAndMessage = this.FindControl<Grid>("gridImageAndMessage");
            imgIcon = this.FindControl<Image>("imgIcon");

            Setup(messageBoxOptions);
        }

        private async void Setup(MessageBoxOptions messageBoxOptions)
        {
            labMessage.Text = messageBoxOptions.Message;

            switch (messageBoxOptions.Buttons)
            {
                case MessageButton.AbortRetryIgnore:
                    butAbort.IsVisible = true;
                    butRetry.IsVisible = true;
                    butIgnore.IsVisible = true;
                    break;
                case MessageButton.OK:
                    butOK.IsVisible = true;
                    break;
                case MessageButton.OKCancel:
                    butOK.IsVisible = true;
                    butCancel.IsVisible = true;
                    break;
                case MessageButton.RetryCancel:
                    butRetry.IsVisible = true;
                    butCancel.IsVisible = true;
                    break;
                case MessageButton.YesNo:
                    butYes.IsVisible = true;
                    butNo.IsVisible = true;
                    break;
                case MessageButton.YesNoCancel:
                    butYes.IsVisible = true;
                    butNo.IsVisible = true;
                    butCancel.IsVisible = true;
                    break;
            }

            if ((int) messageBoxOptions.Icon == 0)
            {
                gridImageAndMessage.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                imgIcon.IsVisible = false;
            }
            else
            {
                switch ((int) messageBoxOptions.Icon)
                {
                    case 64:
                        imgIcon.Source =
                            new Bitmap(await GetEmbeddedFile.GetFileStream("information-outline", "png", "Images"));
                        break;
                    case 48:
                        imgIcon.Source = new Bitmap(await GetEmbeddedFile.GetFileStream("alert", "png", "Images"));
                        break;
                    case 16:
                        imgIcon.Source =
                            new Bitmap(await GetEmbeddedFile.GetFileStream("alert-circle-outline", "png", "Images"));
                        break;
                    case 32:
                        imgIcon.Source =
                            new Bitmap(await GetEmbeddedFile.GetFileStream("information-outline", "png", "Images"));
                        break;
                }
            }
        }

        private static async Task<MessageResult> _Show(string message, MessageButton messageButton,
            MessageIcon messageIcon, string title, Window window)
        {
            var messageBox = new MessageBox(new MessageBoxOptions
            {
                Title = title,
                Message = message,
                Icon = messageIcon,
                Buttons = messageButton
            });
            messageBox.OwnerWindow = new MainWindow(messageBox)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await messageBox.OwnerWindow.ShowDialog(window);
            return messageBox.Result;
        }

        public static Task Show(string message, MessageIcon messageIcon, Window window,
            MessageButton messageButton = MessageButton.OKCancel, string title = "HATE")
        {
            _ = _Show(message, messageButton, messageIcon, title, window);
            return Task.CompletedTask;
        }

        public static async Task<MessageResult> Show(string message, MessageButton messageButton,
            MessageIcon messageIcon, Window window, string title = "HATE")
        {
            return await _Show(message, messageButton, messageIcon, title, window);
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            switch (((Button) sender).Content.ToString())
            {
                case "Abort":
                    Result = MessageResult.Abort;
                    break;
                case "Cancel":
                    Result = MessageResult.Cancel;
                    break;
                case "Ignore":
                    Result = MessageResult.Ignore;
                    break;
                case "No":
                    Result = MessageResult.No;
                    break;
                case "OK":
                    Result = MessageResult.OK;
                    break;
                case "Retry":
                    Result = MessageResult.Retry;
                    break;
                case "Yes":
                    Result = MessageResult.Yes;
                    break;
            }

            OwnerWindow.Close();
        }

        private class MessageBoxOptions
        {
            public MessageButton Buttons;
            public MessageIcon Icon;
            public string Message;
            public string Title;
        }
    }
}