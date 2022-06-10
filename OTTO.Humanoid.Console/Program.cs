using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Hosting;
using NLog.Fluent;
using OTTO.Humanoid.Console.Mp;
using OTTO.Humanoid.Console.Mqtt;
using OTTO.Humanoid.Console.View;
using SkiaSharp;

namespace OTTO.Humanoid.Console;

/// <summary>
/// 
/// </summary>
public static class Program
{
    private static Task? _backgroundHostTask;
    private static Task<int>? _uiTask;
    private static readonly HostBuilder Builder = new();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("cs");
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture; //CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
        if (OperatingSystem.IsWindows())
        {
            // ensure console input handles Czech characters correctly
            System.Console.InputEncoding = Encoding.Unicode;
            System.Console.OutputEncoding = Encoding.Unicode;
        }

        try
        {
            System.Console.WriteLine(SKTypeface.Default.FamilyName);
            System.Console.WriteLine(SKTypeface.FromFamilyName("Sans").FamilyName);
            _backgroundHostTask = CreateHostBuilder(args).Build().RunAsync();
            _uiTask = Task.Run(() =>
            {
                var app = BuildAvaloniaApp();
                return app.StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            });
            await _backgroundHostTask;
        }
        catch (Exception exception)
        {
            System.Console.WriteLine($"FATAL | Exception in Main: ${exception}");
        }
        finally
        {
            System.Console.WriteLine("Bye!");
            LogManager.Shutdown();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Builder.ConfigureServices((context, services) =>
            {
                var configurationRoot = context.Configuration;
                services.Configure<MqttServiceConfiguration>(
                    configurationRoot.GetSection(nameof(MqttServiceConfiguration)));
                services.Configure<OpenCvOptions>(
                    configurationRoot.GetSection(nameof(OpenCvOptions)));
                services.AddHostedService<MqttServer>();
                //services.AddHostedService<Otto>();
            }).ConfigureLogging(log => { log.ClearProviders(); })
            .UseNLog()
            .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.json"));

    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().LogToTrace();
}
