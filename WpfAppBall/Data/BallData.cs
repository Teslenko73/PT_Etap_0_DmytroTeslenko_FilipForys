using System;
using System.Threading;

namespace WpfAppBall.Data.DataImplementation
{
    internal class BallData : IBallData
    {
        private static int _idCounter = 0;
        private static readonly Random _rng = new Random();

        private readonly object _syncLock = new object();
        private Action<IBallData> _onMoved;

        private double _x, _y, _vx, _vy;
        private CancellationTokenSource _cts;

        public int Id { get; }
        public double Radius { get; }
        public double Mass { get; }        

        public double X { get { lock (_syncLock) return _x; } }
        public double Y { get { lock (_syncLock) return _y; } }
        public double VelocityX { get { lock (_syncLock) return _vx; } }
        public double VelocityY { get { lock (_syncLock) return _vy; } }

        // Konstruktor produkcyjny
        public BallData(double boardWidth, double boardHeight, Action<IBallData> onMoved)
        {
            Id = Interlocked.Increment(ref _idCounter);
            Radius = 15.0;
            Mass = Math.PI * Radius * Radius;
            _onMoved = onMoved;

            lock (_rng)
            {
                _x = _rng.NextDouble() * (boardWidth - 2 * Radius) + Radius;
                _y = _rng.NextDouble() * (boardHeight - 2 * Radius) + Radius;
                double speed = _rng.NextDouble() * 3.0 + 1.5;
                double angle = _rng.NextDouble() * 2 * Math.PI;
                _vx = speed * Math.Cos(angle);
                _vy = speed * Math.Sin(angle);
            }
        }

        // Konstruktor testowalny (bez wątku)
        internal BallData(int id, double x, double y, double vx, double vy,
                          double radius = 15.0, double mass = 0)
        {
            Id = id;
            _x = x; _y = y; _vx = vx; _vy = vy;
            Radius = radius;
            Mass = mass > 0 ? mass : Math.PI * radius * radius;
        }

        // ── IBallData
        public void Move(double boardWidth, double boardHeight)
        {
            lock (_syncLock)
            {
                double nx = _x + _vx;
                double ny = _y + _vy;

                if (nx - Radius < 0) { nx = Radius; _vx = Math.Abs(_vx); }
                else if (nx + Radius > boardWidth) { nx = boardWidth - Radius; _vx = -Math.Abs(_vx); }

                if (ny - Radius < 0) { ny = Radius; _vy = Math.Abs(_vy); }
                else if (ny + Radius > boardHeight) { ny = boardHeight - Radius; _vy = -Math.Abs(_vy); }

                _x = nx;
                _y = ny;
            }
        }

        public void SetVelocity(double vx, double vy)
        {
            lock (_syncLock) { _vx = vx; _vy = vy; }
        }

        public void NotifyMoved() => _onMoved?.Invoke(this);

        // watek
        internal void StartThread(double boardWidth, double boardHeight)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var thread = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Move(boardWidth, boardHeight);
                    NotifyMoved();
                    Thread.Sleep(16);   // ~60 fps
                }
            })
            { IsBackground = true, Name = $"Ball-{Id}" };

            thread.Start();
        }

        internal void StopThread() => _cts?.Cancel();
    }
}

//using System;

//namespace WpfAppBall.Data.DataImplementation
//{

//    internal class BallData : IBallData
//    {
//        // Licznik do unikalnych ID
//        private static int _idCounter = 0;
//        private static readonly Random _rng = new Random();

//        private readonly object _syncLock = new object();

//        public int Id { get; }
//        public double X { get; private set; }
//        public double Y { get; private set; }
//        public double Radius { get; } = 15.0;
//        public double VelocityX { get; private set; }
//        public double VelocityY { get; private set; }


//        public BallData(double boardWidth, double boardHeight)
//        {
//            Id = System.Threading.Interlocked.Increment(ref _idCounter);


//            X = _rng.NextDouble() * (boardWidth - 2 * Radius) + Radius;
//            Y = _rng.NextDouble() * (boardHeight - 2 * Radius) + Radius;


//            double speed = _rng.NextDouble() * 4.0 + 2.0;
//            double angle = _rng.NextDouble() * 2 * Math.PI;
//            VelocityX = speed * Math.Cos(angle);
//            VelocityY = speed * Math.Sin(angle);
//        }

//        internal BallData(int id, double x, double y, double vx, double vy, double radius = 15.0)
//        {
//            Id = id;
//            X = x; Y = y;
//            VelocityX = vx; VelocityY = vy;
//            Radius = radius;
//        }


//        public void Move(double boardWidth, double boardHeight)
//        {
//            lock (_syncLock)
//            {
//                double nx = X + VelocityX;
//                double ny = Y + VelocityY;

//                // Odbicie od lewej / prawej
//                if (nx - Radius < 0)
//                {
//                    nx = Radius;
//                    VelocityX = Math.Abs(VelocityX);
//                }
//                else if (nx + Radius > boardWidth)
//                {
//                    nx = boardWidth - Radius;
//                    VelocityX = -Math.Abs(VelocityX);
//                }

//                // Odbicie od górnej / dolnej
//                if (ny - Radius < 0)
//                {
//                    ny = Radius;
//                    VelocityY = Math.Abs(VelocityY);
//                }
//                else if (ny + Radius > boardHeight)
//                {
//                    ny = boardHeight - Radius;
//                    VelocityY = -Math.Abs(VelocityY);
//                }

//                X = nx;
//                Y = ny;
//            }
//        }
//    }
//}