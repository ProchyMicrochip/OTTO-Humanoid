using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog;
using OTTO.Humanoid.Console.Mp;
using OTTO.Humanoid.Console.ViewModel;
using SixLabors.ImageSharp;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Image = Avalonia.Controls.Image;
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

namespace OTTO.Humanoid.Console.View;

/// <inheritdoc />
public class MainWindow : Window
{
    private MainWindowViewModel _viewModel;
    //private readonly Otto _otto;
    private Canvas _canvas;
    private Image _image = new Image();
    private List<Rectangle> _list = new();
    private PoseDetector _detector;
    /// <inheritdoc />
    public MainWindow()
    {
        _detector = new PoseDetector(LogManager.GetCurrentClassLogger());
        _detector.PropertyChanged += DetectorOnPropertyChanged;
        //_otto = new Otto(new NullLogger<Otto>());
        //_otto.StartAsync(new CancellationToken());
        InitializeComponent();
        _canvas = this.FindControl<Canvas>("Mycanvas");
        _canvas.Background = Brushes.Blue;
        _viewModel = (MainWindowViewModel)DataContext!;
        _image.Width = 640;
        _image.Height = 480;
        Canvas.SetTop(_image, 0);
        Canvas.SetLeft(_image,0);
        _canvas.Children.Add(_image);
        var source = new CancellationTokenSource();
        Compute(source.Token);
        
    }

    private async void Compute(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _detector.GetPose();
            _image.Source = _detector.CreateLandMarkedImage();
            await Task.Delay(2, cancellationToken);
        }
    }
    private void DetectorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //_detector.GetPose();
        _image.Source = _detector.CreateLandMarkedImage();
        _canvas.Children.Clear();
        _canvas.Children.Add(_image);
        if(_detector.Landmarks == null) return;
        DrawLandmark(_detector.Landmarks.Landmark[11]);
        DrawLandmark(_detector.Landmarks.Landmark[12]);
        DrawLandmark(_detector.Landmarks.Landmark[15]);
        DrawLandmark(_detector.Landmarks.Landmark[16]);
        DrawLandmark(_detector.Landmarks.Landmark[23]);
        DrawLandmark(_detector.Landmarks.Landmark[24]);
        DrawLandmark(_detector.Landmarks.Landmark[25]);
        DrawLandmark(_detector.Landmarks.Landmark[26]);
    }

    

    private void DrawLandmark(NormalizedLandmark? landmark)
    {
        if(landmark == null) return;
        var rectangle = new Rectangle(){Width = 10, Height = 10};
        Canvas.SetTop(rectangle,landmark.Y*480);
        Canvas.SetLeft(rectangle, landmark.X*680);
        rectangle.Fill = Brushes.OrangeRed;
        _canvas.Children.Add(rectangle);
    }
private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
