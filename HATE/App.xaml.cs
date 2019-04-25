using Avalonia;
using Avalonia.Markup.Xaml;

namespace HATE
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}