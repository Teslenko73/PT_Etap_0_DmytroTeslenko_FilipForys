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
       // private CancellationTokenSource _cts;
        private Timer _timer;

        // Logger 
        private readonly DiagnosticLogger _logger;

        public int Id { get; }
        public double Radius { get; }
        public double Mass { get; }

        public double X { get { lock (_syncLock) return _x; } }
        public double Y { get { lock (_syncLock) return _y; } }
        public double VelocityX { get { lock (_syncLock) return _vx; } }
        public double VelocityY { get { lock (_syncLock) return _vy; } }

        // Konstruktor produkcyjny
        public BallData(double boardWidth, double boardHeight,
                        Action<IBallData> onMoved,
                        DiagnosticLogger logger = null)
        {
            Id = Interlocked.Increment(ref _idCounter);
            Radius = 15.0;
            Mass = Math.PI * Radius * Radius;
            _onMoved = onMoved;
            _logger = logger;

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

        // Konstruktor testowy (bez wątku, bez loggera)
        internal BallData(int id, double x, double y, double vx, double vy,
                          double radius = 15.0, double mass = 0)
        {
            Id = id;
            _x = x; _y = y; _vx = vx; _vy = vy;
            Radius = radius;
            Mass = mass > 0 ? mass : Math.PI * radius * radius;
        }

        // ── IBallData ──────────────────────────────────────────────────────────


        public void Move(double boardWidth, double boardHeight, double deltaTime = 1.0)
        {
            lock (_syncLock)
            {

                _x += _vx * deltaTime;
                _y += _vy * deltaTime;
            }
        }

        // Stara sygnatura zachowana dla IBallData (deltaTime domyślny = 1.0)
        void IBallData.Move(double boardWidth, double boardHeight)
            => Move(boardWidth, boardHeight, 1.0);

        public void SetVelocity(double vx, double vy)
        {
            lock (_syncLock) { _vx = vx; _vy = vy; }
        }

        public void NotifyMoved() => _onMoved?.Invoke(this);

        // ── Wątek ─────────────────────────────────────────────────────────────

        internal void StartThread(double boardWidth, double boardHeight)
        {

            _timer = new Timer(ExecuteStep, Tuple.Create(boardWidth, boardHeight), 0, 10);
        }
        private void ExecuteStep(object state)
        {
            var bounds = (Tuple<double, double>)state;
            double width = bounds.Item1;
            double height = bounds.Item2;

            // ponieważ to systemowy Timer gwarantuje stałość interwału czasowego.
            Move(width, height, 1.0);
            NotifyMoved();

            // Logowanie diagnostyczne
            _logger?.Log(Id, X, Y, VelocityX, VelocityY);
        }

        public void Stop()
        {
            // Zatrzymanie timera
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
        internal void StopThread()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}


//using System;
//using System.Threading;

//namespace WpfAppBall.Data.DataImplementation
//{
//    internal class BallData : IBallData
//    {
//        private static int _idCounter = 0;
//        private static readonly Random _rng = new Random();

//        private readonly object _syncLock = new object();
//        private Action<IBallData> _onMoved;

//        private double _x, _y, _vx, _vy;
//        private CancellationTokenSource _cts;

//        public int Id { get; }
//        public double Radius { get; }
//        public double Mass { get; }        

//        public double X { get { lock (_syncLock) return _x; } }
//        public double Y { get { lock (_syncLock) return _y; } }
//        public double VelocityX { get { lock (_syncLock) return _vx; } }
//        public double VelocityY { get { lock (_syncLock) return _vy; } }


//        public BallData(double boardWidth, double boardHeight, Action<IBallData> onMoved)
//        {
//            Id = Interlocked.Increment(ref _idCounter);
//            Radius = 15.0;
//            Mass = Math.PI * Radius * Radius;
//            _onMoved = onMoved;

//            lock (_rng)
//            {
//                _x = _rng.NextDouble() * (boardWidth - 2 * Radius) + Radius;
//                _y = _rng.NextDouble() * (boardHeight - 2 * Radius) + Radius;
//                double speed = _rng.NextDouble() * 3.0 + 1.5;
//                double angle = _rng.NextDouble() * 2 * Math.PI;
//                _vx = speed * Math.Cos(angle);
//                _vy = speed * Math.Sin(angle);
//            }
//        }

//        // Konstruktor test(bez wątku)
//        internal BallData(int id, double x, double y, double vx, double vy,
//                          double radius = 15.0, double mass = 0)
//        {
//            Id = id;
//            _x = x; _y = y; _vx = vx; _vy = vy;
//            Radius = radius;
//            Mass = mass > 0 ? mass : Math.PI * radius * radius;
//        }

//        // ── IBallData
//        public void Move(double boardWidth, double boardHeight)
//        {
//            lock (_syncLock)
//            {
//                double nx = _x + _vx;
//                double ny = _y + _vy;

//                if (nx - Radius < 0) { nx = Radius; _vx = Math.Abs(_vx); }
//                else if (nx + Radius > boardWidth) { nx = boardWidth - Radius; _vx = -Math.Abs(_vx); }

//                if (ny - Radius < 0) { ny = Radius; _vy = Math.Abs(_vy); }
//                else if (ny + Radius > boardHeight) { ny = boardHeight - Radius; _vy = -Math.Abs(_vy); }

//                _x = nx;
//                _y = ny;
//            }
//        }

//        public void SetVelocity(double vx, double vy)
//        {
//            lock (_syncLock) { _vx = vx; _vy = vy; }
//        }

//        public void NotifyMoved() => _onMoved?.Invoke(this);

//        // watek
//        internal void StartThread(double boardWidth, double boardHeight)
//        {
//            _cts = new CancellationTokenSource();
//            var token = _cts.Token;

//            var thread = new Thread(() =>
//            {
//                while (!token.IsCancellationRequested)
//                {
//                    Move(boardWidth, boardHeight);
//                    NotifyMoved();
//                    Thread.Sleep(16);   // ~60 fps
//                }
//            })
//            { IsBackground = true, Name = $"Ball-{Id}" };

//            thread.Start();
//        }

//        internal void StopThread() => _cts?.Cancel();
//    }
//}
