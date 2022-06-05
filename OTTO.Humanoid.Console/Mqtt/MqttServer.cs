using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace OTTO.Humanoid.Console.Mqtt;

/// <summary>
/// 
/// </summary>
public class MqttServer : BackgroundService, IMqttServerSubscriptionInterceptor, IMqttServerApplicationMessageInterceptor, IMqttServerConnectionValidator
{
    private readonly ILogger<MqttServer> _logger;
    private readonly string _serviceName;
    private static double BytesDivider => 1048576.0;
    /// <summary>
    /// 
    /// </summary>
    public MqttServiceConfiguration MqttServiceConfiguration { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    public MqttServer(ILogger<MqttServer> logger, IOptions<MqttServiceConfiguration> options)
    {
        MqttServiceConfiguration = options.Value;
        _logger = logger; 
        _serviceName = "mqtt server";
    }
    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!MqttServiceConfiguration.IsValid())
        {
            throw new Exception("The configuration is invalid");
        }

        _logger.LogInformation("Starting service");
        StartMqttServer();
        _logger.LogInformation("Service started");
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                LogMemoryInformation();
                await Task.Delay(MqttServiceConfiguration.DelayInMilliSeconds, cancellationToken);
            }
            catch (Exception ex)
            {
               _logger.LogError("An error occurred: {Exception}", ex);
            }
        }
    }


    /// <inheritdoc />
    public Task InterceptSubscriptionAsync(MqttSubscriptionInterceptorContext context)
    {
        try
        {
            context.AcceptSubscription = true;
            LogMessage(context, true);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
           _logger.LogError("An error occurred: {Exception}", ex);
            return Task.FromException(ex);
        }
    }


    /// <inheritdoc />
    public Task InterceptApplicationMessagePublishAsync(MqttApplicationMessageInterceptorContext context)
    {
        try
        {
            context.AcceptPublish = true;
            LogMessage(context);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {Exception}", ex);
            return Task.FromException(ex);
        }
    }


    /// <inheritdoc />
    public Task ValidateConnectionAsync(MqttConnectionValidatorContext context)
    {
        try
        {
            var currentUser = MqttServiceConfiguration.Users.FirstOrDefault(u => u.UserName == context.Username);

            if (currentUser == null)
            {
                context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(context, true);
                return Task.CompletedTask;
            }

            if (context.Username != currentUser.UserName)
            {
                context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(context, true);
                return Task.CompletedTask;
            }

            if (context.Password != currentUser.Password)
            {
                context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(context, true);
                return Task.CompletedTask;
            }

            context.ReasonCode = MqttConnectReasonCode.Success;
            LogMessage(context, false);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {Exception}", ex);
            return Task.FromException(ex);
        }
    }
    /// <summary>
    /// Starts the MQTT server.
    /// </summary>
    private void StartMqttServer()
    {
        var optionsBuilder = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(MqttServiceConfiguration.Port)
            .WithEncryptedEndpointPort(MqttServiceConfiguration.TlsPort)
            .WithConnectionValidator(this)
            .WithSubscriptionInterceptor(this)
            .WithApplicationMessageInterceptor(this);

        var mqttServer = new MqttFactory().CreateMqttServer();
        mqttServer.StartAsync(optionsBuilder.Build());
    }
    /// <summary> 
    ///     Logs the message from the MQTT subscription interceptor context. 
    /// </summary> 
    /// <param name="context">The MQTT subscription interceptor context.</param> 
    /// <param name="successful">A <see cref="bool"/> value indicating whether the subscription was successful or not.</param> 
    private void LogMessage(MqttSubscriptionInterceptorContext context, bool successful)
    {
        _logger.LogInformation(
            successful
                ? $"New subscription: ClientId = {context.ClientId}, TopicFilter = {context.TopicFilter}"
                : $"Subscription failed for clientId = {context.ClientId}, TopicFilter = {context.TopicFilter}");
    }
    /// <summary>
    ///     Logs the message from the MQTT message interceptor context.
    /// </summary>
    /// <param name="context">The MQTT message interceptor context.</param>
    private void LogMessage(MqttApplicationMessageInterceptorContext context)
    {
        var payload = context.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(context.ApplicationMessage.Payload);

        _logger.LogInformation(
            "Message: ClientId = {ClientId}, Topic = {Topic}, Payload = {Payload}, QoS = {Qos}, Retain-Flag = {RetainFlag}",
            context.ClientId,
            context.ApplicationMessage?.Topic,
            payload,
            context.ApplicationMessage?.QualityOfServiceLevel,
            context.ApplicationMessage?.Retain);
    }
    /// <summary> 
    ///     Logs the message from the MQTT connection validation context. 
    /// </summary> 
    /// <param name="context">The MQTT connection validation context.</param> 
    /// <param name="showPassword">A <see cref="bool"/> value indicating whether the password is written to the log or not.</param> 
    private void LogMessage(MqttConnectionValidatorContext context, bool showPassword)
    {
        if (showPassword)
        {
            _logger.LogInformation(
                "New connection: ClientId = {ClientId}, Endpoint = {Endpoint}, Username = {UserName}, Password = {Password}, CleanSession = {CleanSession}",
                context.ClientId,
                context.Endpoint,
                context.Username,
                context.Password,
                context.CleanSession);
        }
        else
        {
            _logger.LogInformation(
                "New connection: ClientId = {ClientId}, Endpoint = {Endpoint}, Username = {UserName}, CleanSession = {CleanSession}",
                context.ClientId,
                context.Endpoint,
                context.Username,
                context.CleanSession);
        }
    }
    /// <summary>
    /// Logs the heartbeat message with some memory information.
    /// </summary>
    private void LogMemoryInformation()
    {
        var totalMemory = GC.GetTotalMemory(false);
        var memoryInfo = GC.GetGCMemoryInfo();
        var divider = BytesDivider;
        _logger.LogInformation(
            "Heartbeat for service {ServiceName}: Total {Total}, heap size: {HeapSize}, memory load: {MemoryLoad}",
            _serviceName, $"{(totalMemory / divider):N3}", $"{(memoryInfo.HeapSizeBytes / divider):N3}", $"{(memoryInfo.MemoryLoadBytes / divider):N3}");
    }
}