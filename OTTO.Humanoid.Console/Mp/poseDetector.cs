using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Mediapipe.Net.Calculators;
using Mediapipe.Net.Framework.Format;
using Mediapipe.Net.Framework.Packet;
using Mediapipe.Net.Framework.Protobuf;
using NLog;
using SeeShark;
using SeeShark.Device;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using IImage = Avalonia.Media.IImage;
using ImageFrame = Mediapipe.Net.Framework.Format.ImageFrame;

namespace OTTO.Humanoid.Console.Mp;

/// <summary>
/// 
/// </summary>
public sealed class PoseDetector : INotifyPropertyChanged
{

    private static Camera? _camera;
    private static FrameConverter? _converter;
    private static Calculator<NormalizedLandmarkListPacket, NormalizedLandmarkList>? _calculator;
    private readonly Logger _logger;
    private readonly CancellationToken _stoppingToken;
    /// <summary>
    /// 
    /// </summary>
    public IImage? Currentimage
    { 
        get => _currentimage;
        set
        {
            _currentimage = value;
            OnPropertyChanged(nameof(Currentimage));
        } 
    }

    private IImage? _currentimage;
    /// <summary>
    /// 
    /// </summary>
    public NormalizedLandmarkList? Landmarks 
    { 
        get => _landmarks;
        set
        {
            _landmarks = value;
            OnPropertyChanged(nameof(Landmarks));
        } 
    }

    private NormalizedLandmarkList? _landmarks;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="stoppingToken"></param>
    /// <param name="cameraInfo"></param>
    public PoseDetector(Logger logger,CameraInfo cameraInfo, CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        _logger = logger;
        using var manager = new CameraManager();
        _camera = manager.GetDevice(cameraInfo);
        _logger.Info("Using camera {Info}",_camera.Info);
        try
        {
            _calculator = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new BlazePoseGpuCalculator()
                : new BlazePoseCpuCalculator();
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            throw;
        }
        //_calculator.OnResult += HandleLandmarks;
        _calculator.Run();
        var poseThread = new Thread(PoseThreadLoop)
        {
            IsBackground = true,
        };
        poseThread.Start();
    }

    private void PoseThreadLoop()
    {
        while (!_stoppingToken.IsCancellationRequested)
        {
            //try
            //{
                GetPose();
                //await Task.Delay(20, _stoppingToken);
                /*}
                catch (Exception exception)
                {
                    _logger.Error("{Message}",exception.Message); // log and restart workflow
                }*/

        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetPose()
    {
        if (_camera == null) return;
        var frame = _camera.GetFrame();
        _converter ??= new FrameConverter(frame,1280,720, PixelFormat.Rgb24);

        var cFrame = _converter.Convert(frame);
        
        using var imgframe = new ImageFrame(ImageFormat.Srgb,
            cFrame.Width, cFrame.Height, cFrame.WidthStep, cFrame.RawData);
        
        if (_calculator != null)
        {
            unsafe
            {
                var img = _calculator.Send(imgframe);
                //var pixeldata = img.MutablePixelData;
                using var ms = new MemoryStream();
                if (img != null)
                {
                    var safearray = new byte[img.PixelDataSize];
                    Marshal.Copy((IntPtr)img.MutablePixelData,safearray,0,img.PixelDataSize );
                    var imageframe = Image.LoadPixelData<Rgb24>(safearray, cFrame.Width, cFrame.Height);
                    imageframe.SaveAsBmp(ms);
                }

                ms.Position = 0;
                var bitmap = new Bitmap(ms);
                Currentimage = bitmap;
                img?.Dispose();
            }
        }
        
    }
/*
    private void HandleLandmarks(object? sender, NormalizedLandmarkList? landmarks)
    {
        Landmarks = landmarks;
        if (landmarks != null)
            _logger.Info("Got a list of {Count} landmarks at frame {CurrentFrame}", landmarks.Landmark.Count,
                _calculator?.CurrentFrame);
    }
*/

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
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}