using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Launcher.ViewModels;
using Launcher.Views;
using System.Threading.Tasks;

namespace Launcher
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var splashScreenVM = new SplashScreenViewModel();
                var splashScreen = new SplashScreen
                {
                    DataContext = splashScreenVM
                };

                desktop.MainWindow = splashScreen;

                splashScreen.Show();


                // Create the main window, and swap it in for the real main window
                var mainWin = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                try
                {
                    await mainWin.GetVersion();
                }
                catch (TaskCanceledException)
                { }
                    // If the task was canceled, we don't need to do anything
                desktop.MainWindow = mainWin;
                mainWin.Show();

                // Get rid of the splash screen
                splashScreen.Close();

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}