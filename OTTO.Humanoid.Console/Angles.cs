using System.Text;

namespace OTTO.Humanoid.Console;

/// <summary>
/// 
/// </summary>
public class Angles
{
    /// <summary>
    /// 
    /// </summary>
    public ushort LeftArm { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public ushort RightArm { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public ushort LeftLeg { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public ushort RightLeg { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string MqttPayload() => $"LeftArm: {LeftArm},\n RightArm: {RightArm},\n LeftLeg: {LeftLeg},\n RightLeg: {RightLeg}\n";

}
