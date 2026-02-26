using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaApplication1;

public partial class MainWindow : Window
{
    // --- Physics Settings ---
    private double _gravity = 0.3;      
    private double _velocity = 0;       
    private double _jumpStrength = -7;  

    // --- Pipe Settings ---
    private double _pipeSpeed = 2.5; // How fast pipes move left
    private int _spawnCounter = 0;   // Counts frames to know when to spawn
    private List<Rectangle> _pipes = new List<Rectangle>();

    private DispatcherTimer _timer;
    private Random _random = new Random();

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Update;
        _timer.Start();

        this.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            _velocity = _jumpStrength; 
        }
    }

    private void Update(object? sender, EventArgs e)
    {
        // 1. Bird Physics
        _velocity += _gravity;
        double birdTop = Canvas.GetTop(Bird);
        Canvas.SetTop(Bird, birdTop + _velocity);

        // 2. Pipe Spawning (Every 100 frames approx)
        _spawnCounter++;
        if (_spawnCounter > 100)
        {
            CreatePipePair();
            _spawnCounter = 0;
        }

        // 3. Move Pipes
        foreach (var pipe in _pipes.ToList())
        {
            double currentLeft = Canvas.GetLeft(pipe);
            Canvas.SetLeft(pipe, currentLeft - _pipeSpeed);

            // Remove pipe if it goes off screen to save memory
            if (currentLeft < -60)
            {
                GameCanvas.Children.Remove(pipe);
                _pipes.Remove(pipe);
            }
        }

        // 4. Floor Check
        if (birdTop > this.Height)
        {
            ResetGame();
        }
    }

    private void CreatePipePair()
    {
        double gapHeight = 180; // The space the bird flies through
        double pipeWidth = 70;
        double randomY = _random.Next(50, (int)(this.Height - gapHeight - 50));

        // Top Pipe
        Rectangle topPipe = new Rectangle
        {
            Width = pipeWidth,
            Height = randomY,
            Fill = Brushes.Green,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            [Canvas.LeftProperty] = 400,
            [Canvas.TopProperty] = 0
        };

        // Bottom Pipe
        Rectangle bottomPipe = new Rectangle
        {
            Width = pipeWidth,
            Height = this.Height - randomY - gapHeight,
            Fill = Brushes.Green,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            [Canvas.LeftProperty] = 400,
            [Canvas.TopProperty] = randomY + gapHeight
        };

        _pipes.Add(topPipe);
        _pipes.Add(bottomPipe);
        GameCanvas.Children.Add(topPipe);
        GameCanvas.Children.Add(bottomPipe);
    }

    private void ResetGame()
    {
        _velocity = 0;
        Canvas.SetTop(Bird, 200);

        // Clear pipes when resetting
        foreach (var pipe in _pipes)
        {
            GameCanvas.Children.Remove(pipe);
        }
        _pipes.Clear();
    }
}