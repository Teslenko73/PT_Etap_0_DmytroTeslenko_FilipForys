using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WpfAppBall.Data;
using WpfAppBall.Data.DataImplementation;

namespace WpfAppBall.Logic.LogicImplementation
{
    internal class LogicApi : LogicAbstractApi
    {
        private readonly DataAbstractApi _data;
        private double _boardWidth, _boardHeight;

        private readonly List<Thread> _ballThreads = new List<Thread>();
        private volatile bool _isRunning = false;

        // ─── IMPLEMENTACJA REAKTYWNEGO ZDARZENIA ───
        public override event EventHandler<IEnumerable<IBallDto>> BallsUpdated;

        public LogicApi(DataAbstractApi data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override void CreateBalls(int count, double boardWidth, double boardHeight)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (boardWidth <= 0 || boardHeight <= 0) throw new ArgumentException("Wymiary muszą być dodatnie.");

            _boardWidth = boardWidth;
            _boardHeight = boardHeight;

            StopSimulation();

            _isRunning = true;

            for (int i = 0; i < count; i++)
            {
                var ballData = _data.CreateBall(boardWidth, boardHeight);

                Thread t = new Thread(() => BallLifecycle(ballData))
                {
                    IsBackground = true,
                    Name = $"LogicBall-Thread-{ballData.Id}"
                };

                _ballThreads.Add(t);
                t.Start();
            }
        }

        public override void StartSimulation(double boardWidth, double boardHeight)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
        }

        public override void StopSimulation()
        {
            _isRunning = false;
            _ballThreads.Clear();
            _data.ClearBalls();
        }

        public override IEnumerable<IBallDto> GetBalls()
        {
            return _data.GetAllBalls().Select(b => (IBallDto)new BallDto(b)).ToList();
        }

        // PĘTLA CZASU RZECZYWISTEGO DLA KAŻDEJ KULI
        private void BallLifecycle(IBallData ball)
        {
            DateTime lastTime = DateTime.UtcNow;

            while (_isRunning)
            {
                DateTime now = DateTime.UtcNow;
                double deltaTime = (now - lastTime).TotalMilliseconds / 16.0;
                lastTime = now;

                if (ball is BallData concreteBall)
                {
                    concreteBall.Move(_boardWidth, _boardHeight, deltaTime);
                }
                else
                {
                    ball.Move(_boardWidth, _boardHeight);
                }

                ResolveWallCollisions(ball);
                ResolveCollisions(ball);

                // Przygotowanie danych dla prezentacji
                var dtos = _data.GetAllBalls()
                                .Select(b => (IBallDto)new BallDto(b))
                                .ToList();

                // ─── REAKTYWNE WYWOŁANIE ZDARZENIA ───
                // Powiadamiamy subskrybentów o nowym stanie danych wejściowych
                BallsUpdated?.Invoke(this, dtos);

                Thread.Sleep(16);
            }
        }

        private void ResolveWallCollisions(IBallData ball)
        {
            double vx = ball.VelocityX;
            double vy = ball.VelocityY;
            double x = ball.X;
            double y = ball.Y;
            double r = ball.Radius;

            bool changed = false;

            if (x - r < 0) { vx = Math.Abs(vx); changed = true; }
            else if (x + r > _boardWidth) { vx = -Math.Abs(vx); changed = true; }

            if (y - r < 0) { vy = Math.Abs(vy); changed = true; }
            else if (y + r > _boardHeight) { vy = -Math.Abs(vy); changed = true; }

            if (changed)
            {
                ball.SetVelocity(vx, vy);
            }
        }

        private void ResolveCollisions(IBallData a)
        {
            var balls = _data.GetAllBalls();
            foreach (var b in balls)
            {
                if (b.Id == a.Id) continue;

                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double realDist = Math.Sqrt(dx * dx + dy * dy);
                double minDist = a.Radius + b.Radius;

                if (realDist < minDist && realDist > 1e-9)
                {
                    var firstLock = a.Id < b.Id ? a : b;
                    var secondLock = a.Id < b.Id ? b : a;

                    lock (firstLock)
                    {
                        lock (secondLock)
                        {
                            dx = b.X - a.X;
                            dy = b.Y - a.Y;
                            realDist = Math.Sqrt(dx * dx + dy * dy);

                            if (realDist < minDist && realDist > 1e-9)
                            {
                                double nx = dx / realDist;
                                double ny = dy / realDist;

                                double aVn = a.VelocityX * nx + a.VelocityY * ny;
                                double bVn = b.VelocityX * nx + b.VelocityY * ny;

                                if (aVn - bVn < 0) continue;

                                double ma = a.Mass, mb = b.Mass;
                                double sum = ma + mb;

                                double newAVn = (aVn * (ma - mb) + 2 * mb * bVn) / sum;
                                double newBVn = (bVn * (mb - ma) + 2 * ma * aVn) / sum;

                                double deltaAVn = newAVn - aVn;
                                double deltaBVn = newBVn - bVn;

                                a.SetVelocity(a.VelocityX + deltaAVn * nx, a.VelocityY + deltaAVn * ny);
                                b.SetVelocity(b.VelocityX + deltaBVn * nx, b.VelocityY + deltaBVn * ny);
                            }
                        }
                    }
                }
            }
        }

        internal class BallDto : IBallDto
        {
            public int Id { get; }
            public double X { get; }
            public double Y { get; }
            public double Radius { get; }

            public BallDto(IBallData d) { Id = d.Id; X = d.X; Y = d.Y; Radius = d.Radius; }
        }
    }
}