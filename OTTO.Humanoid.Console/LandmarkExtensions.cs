using Google.Protobuf.Collections;
using Mediapipe.Net.Framework.Protobuf;

namespace OTTO.Humanoid.Console;

/// <summary>
/// 
/// </summary>
public static class LandmarkExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="landmarks"></param>
    /// <returns></returns>
    public static Angles GetAngles(this RepeatedField<NormalizedLandmark> landmarks)
    {
        var angles = new Angles
        {
            LeftArm = GetAngle(landmarks[15], landmarks[11], landmarks[23]),
            RightArm = GetAngle(landmarks[16], landmarks[12], landmarks[24]),
            LeftLeg = (ushort)(180-GetAngle(landmarks[25], landmarks[23], landmarks[11])),
            RightLeg = (ushort)(180-GetAngle(landmarks[26], landmarks[24], landmarks[12]))
        };
        return angles;
    }

    private static (double a, double b , double c) GetTriangle(NormalizedLandmark l1, NormalizedLandmark l2, NormalizedLandmark l3) {
        var a = Math.Pow(l1.X - l2.X, 2) + Math.Pow(l1.Y - l2.Y, 2);
        var b = Math.Pow(l2.X - l3.X, 2) + Math.Pow(l2.Y - l3.Y, 2);
        var c = Math.Pow(l1.X - l3.X, 2) + Math.Pow(l1.Y - l3.Y, 2);
        return (a, b, c);
    }

    private static ushort GetAngle(NormalizedLandmark l1, NormalizedLandmark l2, NormalizedLandmark l3)
    {
        var triangle = GetTriangle(l1, l2, l3);
        return (ushort)(Math.Acos((triangle.a + triangle.b - triangle.c) / Math.Sqrt(4 * triangle.a * triangle.b)) * 180 / Math.PI);
    }
    
}
