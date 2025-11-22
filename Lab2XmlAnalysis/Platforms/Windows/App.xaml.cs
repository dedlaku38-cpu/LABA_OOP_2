using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Lab2XmlAnalysis.Platforms.Windows
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var mauiWindow = global::Microsoft.Maui.Controls.Application.Current.Windows.First();
            var window = mauiWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;

            if (window != null)
            {
                var appWindow = window.AppWindow;
                if (appWindow != null)
                {
                    // ИСПРАВЛЕНИЕ: Увеличили ширину до 1600, чтобы контент не перекрывался
                    int desiredWidth = 1800;
                    int desiredHeight = 1000;

                    appWindow.Resize(new SizeInt32(desiredWidth, desiredHeight));
                }
            }
        }
    }
}