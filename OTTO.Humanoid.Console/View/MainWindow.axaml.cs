﻿using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Mediapipe.Net.Framework.Protobuf;
using NLog;
using OTTO.Humanoid.Console.Mp;
using Image = Avalonia.Controls.Image;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

namespace OTTO.Humanoid.Console.View;

/// <inheritdoc />
public class MainWindow : Window
{
    //private readonly Otto _otto;
    private readonly Canvas _landmarkcanvas = new();
    private readonly Image _image = new();
    private readonly PoseDetector _detector;
    private readonly int[] _indexes = { 11, 12, 15, 16, 23, 24, 25, 26 };
    private readonly TextBox _text = new();
    /// <inheritdoc />
    public MainWindow()
    {
        _detector = new PoseDetector(LogManager.GetCurrentClassLogger(),CancellationToken.None);
        _detector.PropertyChanged += DetectorOnPropertyChanged;
        // = new Otto(new NullLogger<Otto>());
        //_otto.StartAsync(new CancellationToken());
        InitializeComponent();
        var maincanvas = this.FindControl<Canvas>("Mycanvas");
        maincanvas.Background = Brushes.Blue;
        _image.Width = 640;
        _image.Height = 480;
        _image.Source = _detector.Currentimage;
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
        var rectangle = new Rectangle(){Width = 10, Height = 10};
        Canvas.SetTop(rectangle,landmark.Y*480);
        Canvas.SetLeft(rectangle, landmark.X*680);
        rectangle.Fill = Brushes.OrangeRed;
        _landmarkcanvas.Children.Add(rectangle);
    }
private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}
