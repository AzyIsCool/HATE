using System;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HATE
{
    partial class MainWindow : Window
    {
        public MainWindow(UserControl content, Action openAction = null)
        {
            InitializeComponent(content, openAction);
        }

        private async void InitializeComponent(UserControl content, Action openAction)
        {
            AvaloniaXamlLoader.Load(this);
            this.Opened += new EventHandler((sender, e) => MainWindow_Opened(sender, e, openAction));
            this.Icon = new WindowIcon(await GetEmbeddedFile.GetFileStream("hateicon", "png"));
            gridContent = this.FindControl<Grid>("gridContent");
            btnClose = this.FindControl<Button>("btnClose");
            if (content != null)
            {
                Grid.SetRow(content, 1);
                gridContent.Children.Add(content);
            }
        }

        private void MainWindow_Opened(object sender, EventArgs e, Action action)
        {
            action?.Invoke();
        }

        public void SetContent(UserControl content)
        {
            Grid.SetRow(content, 1);
            gridContent.Children.Add(content);
        }

        public void btnClose_Clicked(object sender, EventArgs e)
        {
            this.Close();
        }

        public void recDrag_Pressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Button button = new Button();
            this.BeginMoveDrag();
        }

        private void btnClose_PointerEnter(object sender, Avalonia.Input.PointerEventArgs e)
        {
            btnClose.Background = Brush.Parse("#FFE64747");
        }

        private void btnClose_PointerLeave(object sender, Avalonia.Input.PointerEventArgs e)
        {
            btnClose.Background = Brushes.Transparent;
        }
    }
}