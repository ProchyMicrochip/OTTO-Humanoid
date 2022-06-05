using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using FFmpeg.AutoGen;
using JetBrains.Annotations;
using Mediapipe.Net.Calculators;
using Mediapipe.Net.External;
using Mediapipe.Net.Framework.Format;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Extensions.Logging;
using NLog;
using SeeShark;
using SeeShark.Device;
using SeeShark.FFmpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using IImage = Avalonia.Media.IImage;
using Image = SixLabors.ImageSharp.Image;
using ImageFrame = Mediapipe.Net.Framework.Format.ImageFrame;

namespace OTTO.Humanoid.Console.Mp;

/// <summary>
/// 
/// </summary>
public class PoseDetector : INotifyPropertyChanged
{

    private static Camera? _camera;
    private static FrameConverter? _converter;
    private static BlazePoseCpuCalculator? _calculator;
    private readonly Logger _logger;
    private Image<Rgb24> _image;
    private NormalizedLandmarkList? _landmarks;

    /// <summary>
    /// 
    /// </summary>
    public NormalizedLandmarkList? Landmarks
    {
        get => _landmarks;
        private set
        {
            _landmarks = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public PoseDetector(Logger logger)
    {
        (int, int)? videoSize = null;
        FFmpegManager.SetupFFmpeg(@"C:\ffmpeg\v5.0_x64\");
        Glog.Initialize("stuff");
        _logger = logger;
        MpOptions parsed = new MpOptions();
        using var manager = new CameraManager();
        _camera = manager.GetDevice(1);
        _logger.Info("Using camera {Info}",_camera.Info);
        _calculator = new();
        _calculator.OnResult += HandleLandmarks;
        _calculator.Run();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetPose()
    {
        if (_camera == null) return;
        var frame = _camera.GetFrame();
        _converter ??= new FrameConverter(frame, PixelFormat.Rgb24);

        Frame cFrame = _converter.Convert(frame);

        using ImageFrame imgframe = new ImageFrame(ImageFormat.Srgb,
            cFrame.Width, cFrame.Height, cFrame.WidthStep, cFrame.RawData);
        
        if (_calculator != null)
        {
            using var img = _calculator.Send(imgframe);
        }
        _image = Image.LoadPixelData<Rgb24>(cFrame.RawData, cFrame.Width, cFrame.Height);
    }
    private void HandleLandmarks(object? sender, NormalizedLandmarkList? landmarks)
    {
        Landmarks = landmarks;
        _logger.Info("Got a list of {Count} landmarks at frame {CurrentFrame}", landmarks.Landmark.Count, _calculator?.CurrentFrame);
    }

    /// <summary>
    /// 
    /// </summary>
    ~PoseDetector()
    {
        _camera?.Dispose();
        _converter?.Dispose();
        _calculator?.Dispose();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IImage? CreateLandMarkedImage()
    {
        using var ms = new MemoryStream();
       _image.SaveAsBmp(ms);
       ms.Position = 0;
       var bitmap = new Bitmap(ms);
       return bitmap;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}