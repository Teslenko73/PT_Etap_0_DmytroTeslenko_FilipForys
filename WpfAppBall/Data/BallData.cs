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

        // Konstruktor testowy
        internal BallData(int id, double x, double y, double vx, double vy,
                          double radius = 15.0, double mass = 0)
        {
            Id = id;
            _x = x; _y = y; _vx = vx; _vy = vy;
            Radius = radius;
            Mass = mass > 0 ? mass : Math.PI * radius * radius;
        }

        // Wymagane przez Twoje testy jednostkowe
        public static void ResetIdCounter()
        {
            _idCounter = 0;
        }

        // Standardowy ruch zgodny z interfejsem IBallData
        public void Move(double boardWidth, double boardHeight)
        {
            Move(boardWidth, boardHeight, 1.0);
        }

        // Ruch rzeczywisty z uwzględnieniem czasu (deltaTime)
        public void Move(double boardWidth, double boardHeight, double deltaTime)
        {
            lock (_syncLock)
            {
                _x += _vx * deltaTime;
                _y += _vy * deltaTime;
            }
            _logger?.Log(Id, X, Y, VelocityX, VelocityY);
        }

        public void SetVelocity(double vx, double vy)
        {
            lock (_syncLock) { _vx = vx; _vy = vy; }
        }

        // Puste zachowania dla zachowania kompatybilności wstecznej interfejsów
        public void StartThread(double boardWidth, double boardHeight) { }
        public void StopThread() { }
        public void Stop() { }
        public void Dispose() { }
    }
}