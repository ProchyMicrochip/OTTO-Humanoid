using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Mediapipe.Net.External;
using Mediapipe.Net.Framework.Protobuf;
using NLog;
using OTTO.Humanoid.Console.Mp;
using OTTO.Humanoid.Console.ViewModel;
using SeeShark.Device;
using SeeShark.FFmpeg;
using Image = Avalonia.Controls.Image;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

namespace OTTO.Humanoid.Console.View;

/// <inheritdoc />
public class MainWindow : Window
{
    //private readonly Otto _otto;
    private readonly MainWindowViewModel _viewModel;
    private readonly Canvas _landmarkcanvas = new();
    private readonly Image _image = new();
    private PoseDetector? _detector;
    private readonly int[] _indexes = { 11, 12, 15, 16, 23, 24, 25, 26 };
    private readonly TextBox _text = new();
    
    /// <inheritdoc />
    public MainWindow()
    {
        var path = Directory.GetCurrentDirectory() + @"\ffmpeg\v5.0_x64\";
        //FFmpegManager.SetupFFmpeg(path,@"/usr/lib");
        System.Console.WriteLine("FFMpeg");
        FFmpegManager.SetupFFmpeg(@"/usr/lib");
        System.Console.WriteLine("Glog");
        Glog.Initialize("stuff");
        // = new Otto(new NullLogger<Otto>());
        //_otto.StartAsync(new CancellationToken());
        System.Console.WriteLine("init");
        InitializeComponent();
        System.Console.WriteLine("datacontext");
        _viewModel = (MainWindowViewModel)DataContext ?? throw new InvalidOperationException();
        System.Console.WriteLine("CamManager");
        using var manager = new CameraManager();
        _viewModel.CameraInfos = manager.Devices;
        var maincanvas = this.FindControl<Canvas>("Mycanvas");
        maincanvas.Background = Brushes.Blue;
        _image.Width = 640;
        _image.Height = 480;
        //_image.Source = _detector.Currentimage;
        _text.Text = "No Landmarks";
        Canvas.SetTop(_image, 0);
        Canvas.SetLeft(_image,0);
        Canvas.SetTop(_landmarkcanvas,0);
        Canvas.SetLeft(_landmarkcanvas,0);
        Canvas.SetTop(_text,0);
        Canvas.SetLeft(_text,0);
        maincanvas.Children.Add(_image);
        maincanvas.Children.Add(_landmarkcanvas);
    }

    private void DetectorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(_detector == null) return;
        if (e.PropertyName == nameof(_detector.Currentimage))
            Dispatcher.UIThread.InvokeAsync(() => _image.Source = _detector.Currentimage,
                DispatcherPriority.Background);
        if (e.PropertyName != nameof(_detector.Landmarks)) return;
        if (_detector.Landmarks?.Landmark == null || _detector.Landmarks.Landmark.Any(x =>
                _indexes.Contains(_detector.Landmarks.Landmark.IndexOf(x)) && x.Presence < 0.3))
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _landmarkcanvas.Children.Clear();
                _landmarkcanvas.Children.Add(_text);
            });
            return;
        }
        Dispatcher.UIThread.InvokeAsync(()=>{
            _landmarkcanvas.Children.Clear();
            _image.Source = _detector.Currentimage;
            DrawLandmark(_detector.Landmarks.Landmark[11]);
            DrawLandmark(_detector.Landmarks.Landmark[12]);
            DrawLandmark(_detector.Landmarks.Landmark[15]);
            DrawLandmark(_detector.Landmarks.Landmark[16]);
            DrawLandmark(_detector.Landmarks.Landmark[23]);
            DrawLandmark(_detector.Landmarks.Landmark[24]);
            DrawLandmark(_detector.Landmarks.Landmark[25]);
            DrawLandmark(_detector.Landmarks.Landmark[26]);}, DispatcherPriority.Background);
        
    }

 
   

    

    private void DrawLandmark(NormalizedLandmark? landmark)
    {
        if(landmark == null) return;
        var rectangle = new Rectangle {Width = 10, Height = 10};
        Canvas.SetTop(rectangle,landmark.Y*480);
        Canvas.SetLeft(rectangle, landmark.X*680);
        rectangle.Fill = Brushes.OrangeRed;
        _landmarkcanvas.Children.Add(rectangle);
    }
private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
{
    if (_detector != null) return;
    if (e.AddedItems[0] == null ) return;
    _detector = new PoseDetector(LogManager.GetCurrentClassLogger(),(CameraInfo)e.AddedItems[0],CancellationToken.None);
    //_detector.PropertyChanged += DetectorOnPropertyChanged;
}
}
