namespace OTTO.Humanoid.Console;

/// <summary>
///     The <see cref="User" /> read from the configuration file.
/// </summary>
public class User
{
    /// <summary>
    ///     Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}