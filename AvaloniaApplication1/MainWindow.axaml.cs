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
    private double _jumpStrength = -5;  

    // --- Game State ---
    private double _pipeSpeed = 2.5; 
    private int _spawnCounter = 0;   
    private int _score = 0;
    private bool _isGameOver = false;
    private bool _isGameStarted = false;

    private List<Rectangle> _pipes = new List<Rectangle>();
    private List<Rectangle> _countedPipes = new List<Rectangle>(); 

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
        if (e.Key == Key.Space && !_isGameOver)
        {
            // The first jump starts the game
            if (!_isGameStarted)
            {
                _isGameStarted = true;
            }
        
            _velocity = _jumpStrength; 
        }
    }

    private void Update(object? sender, EventArgs e)
    {
        if (_isGameOver || !_isGameStarted) return;

        // 1. Bird Physics
        _velocity += _gravity;
        double birdTop = Canvas.GetTop(Bird);
        double birdLeft = Canvas.GetLeft(Bird);
        Canvas.SetTop(Bird, birdTop + _velocity);

        // 2. Pipe Spawning
        _spawnCounter++;
        if (_spawnCounter > 100)
        {
            CreatePipePair();
            _spawnCounter = 0;
        }

        // 3. Move Pipes & Logic
        foreach (var pipe in _pipes.ToList())
        {
            double pipeLeft = Canvas.GetLeft(pipe);
            double pipeTop = Canvas.GetTop(pipe);
            Canvas.SetLeft(pipe, pipeLeft - _pipeSpeed);

            // --- SCORE CALCULATION ---
            if (pipeLeft + pipe.Width < birdLeft && !_countedPipes.Contains(pipe))
            {
                _countedPipes.Add(pipe);
                if (_countedPipes.Count % 2 == 0)
                {
                    _score++;
                    ScoreText.Text = _score.ToString();
                }
            }

            //MANUAL COLLISION
            double birdRight = birdLeft + 50; 
            double birdBottom = birdTop + 30;
            double pipeRight = pipeLeft + pipe.Width;
            double pipeBottom = pipeTop + pipe.Height;

            bool intersectsX = birdRight > pipeLeft && birdLeft < pipeRight;
            bool intersectsY = birdBottom > pipeTop && birdTop < pipeBottom;

            if (intersectsX && intersectsY)
            {
                EndGame();
                return;
            }

            // Clean up off-screen pipes
            if (pipeLeft < -100)
            {
                GameCanvas.Children.Remove(pipe);
                _pipes.Remove(pipe);
                _countedPipes.Remove(pipe);
            }
        }

        // 4. Floor/Ceiling Check
        if (birdTop > this.Height || birdTop < 0)
        {
            EndGame();
        }
    }

    private void CreatePipePair()
    {
        double gapHeight = 180; 
        double pipeWidth = 70;
        double randomY = _random.Next(50, (int)(this.Height - gapHeight - 50));

        Rectangle topPipe = new Rectangle {
            Width = pipeWidth, Height = randomY,
            Fill = Brushes.Green, Stroke = Brushes.Black, StrokeThickness = 2,
            [Canvas.LeftProperty] = 450, [Canvas.TopProperty] = 0
        };

        Rectangle bottomPipe = new Rectangle {
            Width = pipeWidth, Height = this.Height - randomY - gapHeight,
            Fill = Brushes.Green, Stroke = Brushes.Black, StrokeThickness = 2,
            [Canvas.LeftProperty] = 450, [Canvas.TopProperty] = randomY + gapHeight
        };

        _pipes.Add(topPipe);
        _pipes.Add(bottomPipe);
        GameCanvas.Children.Add(topPipe);
        GameCanvas.Children.Add(bottomPipe);
    }

    private void EndGame()
    {
        _isGameOver = true;
        _timer.Stop();
        FinalScoreText.Text = $"Final Score: {_score}";
        GameOverPanel.IsVisible = true;
    }

    public void OnRestartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _isGameStarted = false;
        
        //1. Reset Variables
        _score = 0;
        _velocity = 0;
        _spawnCounter = 0;
        _isGameOver = false;
        
        // 2. Reset UI 
        ScoreText.Text = "0";
        GameOverPanel.IsVisible = false; 
        Canvas.SetTop(Bird, 200);

        // 3. Clear all pipes from the screen
        foreach (var pipe in _pipes.ToList())
       {
            GameCanvas.Children.Remove(pipe);
       }
        // 4. Empty the lists
        _pipes.Clear();
        _countedPipes.Clear();

        // 5. Restart the timer
        _timer.Start();
    }
}