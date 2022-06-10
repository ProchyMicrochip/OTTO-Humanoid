using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging.Abstractions;
using NLog;

namespace OTTO.Humanoid.Console.View;

/// <inheritdoc />
public class App : Application
    {
        /// <inheritdoc />
        public override void Initialize()
        {
            System.Console.WriteLine("Inicialization");
            AvaloniaXamlLoader.Load(this);
            System.Console.WriteLine("Inicialization Completed");
        }

        /// <inheritdoc />
        public override void OnFrameworkInitializationCompleted()
        {
            System.Console.WriteLine("Mainwindow starting");
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.MainWindow = new MainWindow();
            }
            System.Console.WriteLine("Mainwindow started");
        }
    }
