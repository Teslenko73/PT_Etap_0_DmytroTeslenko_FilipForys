using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfAppBall
{
    public partial class MainWindow : Window
    {
        private BallLogic _logic;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            BallRepository data = new BallRepository();
            _logic = new BallLogic(data.CreateBall());

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(20);
            _timer.Tick += Update;
            _timer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            _logic.UpdatePosition(GameCanvas.ActualWidth, GameCanvas.ActualHeight);

            Ball b = _logic.GetBall();

            Canvas.SetLeft(BallShape, b.X);
            Canvas.SetTop(BallShape, b.Y);
        }
    }
}   
