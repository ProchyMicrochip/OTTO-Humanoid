﻿namespace OTTO.Humanoid.Console.Mqtt;

/// <summary>
/// 
/// </summary>
public class MqttServiceConfiguration
{
    /// <summary>
    ///     Gets or sets the port.
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    ///     Gets or sets the list of valid users.
    /// </summary>
    public List<User> Users { get; set; } = new();

    /// <summary>
    /// Gets or sets the heartbeat delay in milliseconds.
    /// </summary>
    public int DelayInMilliSeconds { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the TLS port.
    /// </summary>
    public int TlsPort { get; set; } = 8883;

    /// <summary>
    /// Checks whether the configuration is valid or not.
    /// </summary>
    /// <returns>A value indicating whether the configuration is valid or not.</returns>
    public bool IsValid()
    {
        if (Port is <= 0 or > 65535)
        {
            throw new Exception("The port is invalid");
        }

        if (!Users.Any())
        {
            throw new Exception("The users are invalid");
        }

        if (DelayInMilliSeconds <= 0)
        {
            throw new Exception("The heartbeat delay is invalid");
        }

        if (TlsPort is <= 0 or > 65535)
        {
            throw new Exception("The TLS port is invalid");
        }

        return true;
    }
}