/*using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics.Logger;
using MQTTnet.Implementations;
using NLog;
using OTTO.Humanoid.Console.Mp;

namespace OTTO.Humanoid.Console;

/// <summary>
/// 
/// </summary>
public class Otto : BackgroundService
{
    private IMqttClient _client;
    private ILogger<Otto> _logger;
    private CancellationToken _stoppingToken;
    private Thread? _scanThread;
    /// <summary>
    /// 
    /// </summary>
    public IImage? LandMarkedImage { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public NormalizedLandmarkList? LandmarkList { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public Otto(ILogger<Otto> logger)
    {
        var mqttFactory = new MqttFactory();
        _client = mqttFactory.CreateMqttClient();
        _logger = logger;
    }

   
    private async void ScanThreadLoop()
    {
        var mqttOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost").WithCredentials("Jakub","admin").Build();
        await _client.ConnectAsync(mqttOptions, CancellationToken.None);
        PoseDetector poseDetector = new PoseDetector(LogManager.GetCurrentClassLogger());
        Stopwatch stopwatch = Stopwatch.StartNew();
        int count = 0;
        try
        {
            while (!_stoppingToken.IsCancellationRequested)
            {
                try
                {
                    poseDetector.GetPose();
                    LandMarkedImage = poseDetector.MyImage;
                    LandmarkList = poseDetector.Landmarks;
                    if (LandmarkList == null) continue;
                    var message = new MqttApplicationMessageBuilder().WithTopic("OTTO/test")
                        .WithPayload(LandmarkList.Landmark.GetAngles().MqttPayload()).Build();
                    count++;
                    System.Console.WriteLine(count*1000/stopwatch.ElapsedMilliseconds);
                    await _client.PublishAsync(message, CancellationToken.None);

                }
                catch (Exception exception)
                {
                    _logger.LogError("{Message}",exception.Message); // log and restart workflow
                }
                
            }
        }
        catch (ThreadInterruptedException)
        {
            _logger.LogDebug("Aplikace přerušena");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Aplikace selhala: {Message}", exception.Message);
        }
        finally
        {
            _logger.LogDebug($"Smyčka ukončena");
        }
    }

    


    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
            
        _scanThread = new Thread(ScanThreadLoop)
        {
            IsBackground = true,
        };
        _scanThread.Start();
        return Task.CompletedTask;
    }
}*/