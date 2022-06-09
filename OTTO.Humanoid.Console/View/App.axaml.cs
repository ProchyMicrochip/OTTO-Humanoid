﻿using Avalonia;
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
            System.Console.WriteLine("inicialization started");
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc />
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                System.Console.WriteLine("inicialization completed");
                desktopLifetime.MainWindow = new MainWindow();
            }
        }
    }
