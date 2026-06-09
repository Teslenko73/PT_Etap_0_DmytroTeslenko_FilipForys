using System;
using System.Collections.Generic;
using System.Linq;
using WpfAppBall.Data;

namespace WpfAppBall.Logic.LogicImplementation
{
    internal class LogicApi : LogicAbstractApi
    {
        private readonly DataAbstractApi _data;

        private Action<IEnumerable<IBallDto>> _subscriber;
        private double _boardWidth, _boardHeight;

        // Globalny lock do sekcji krytycznej kolizji
        private readonly object _collisionLock = new object();
        public LogicApi(DataAbstractApi data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _data.Subscribe(OnBallMoved);
        }

        public override void CreateBalls(int count, double boardWidth, double boardHeight)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (boardWidth <= 0 || boardHeight <= 0) throw new ArgumentException("Wymiary muszą być dodatnie.");

            _boardWidth = boardWidth;
            _boardHeight = boardHeight;

            _data.ClearBalls();
            for (int i = 0; i < count; i++)
                _data.CreateBall(boardWidth, boardHeight);
        }

        public override void StartSimulation(double boardWidth, double boardHeight)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
            // Wątki są uruchamiane per kulka przy CreateBall
        }

        public override void StopSimulation()
        {
            _data.ClearBalls();
        }

        public override void Subscribe(Action<IEnumerable<IBallDto>> onUpdate)
        {
            _subscriber = onUpdate;
        }

        public override IEnumerable<IBallDto> GetBalls()
        {
            return _data.GetAllBalls().Select(b => (IBallDto)new BallDto(b)).ToList();
        }

        // ─
        private void OnBallMoved(IBallData movedBall)
        {
            //lock (_collisionLock)
            //{
            ResolveWallCollisions(movedBall);
            ResolveCollisions(movedBall);
            //}
            

            var dtos = _data.GetAllBalls()
                            .Select(b => (IBallDto)new BallDto(b))
                            .ToList();

            _subscriber?.Invoke(dtos);
        }
        private void ResolveWallCollisions(IBallData ball)
        {
            // Pobieramy bezpieczne wartości prędkości i pozycji
            double vx = ball.VelocityX;
            double vy = ball.VelocityY;
            double x = ball.X;
            double y = ball.Y;
            double r = ball.Radius;

            bool changed = false;

            // Sprawdzenie lewej/prawej ściany
            if (x - r < 0) { vx = Math.Abs(vx); changed = true; }
            else if (x + r > _boardWidth) { vx = -Math.Abs(vx); changed = true; }

            // Sprawdzenie górnej/dolnej ściany
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
               // double dist = Math.Sqrt(dx * Math.Sqrt(dx * dx + dy * dy)); 
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

                            // inny wątek mógł już obsłużyć to zderzenie i zmienić pozycje kulek.
                            // Dlatego wewnątrz sekcji krytycznej przeliczamy odległość ponownie.
                            dx = b.X - a.X;
                            dy = b.Y - a.Y;
                            realDist = Math.Sqrt(dx * dx + dy * dy);

                            if (realDist < minDist && realDist > 1e-9)
                            {
                                // Wektor zderzenia
                                double nx = dx / realDist;
                                double ny = dy / realDist;

                                // Składowe prędkości
                                double aVn = a.VelocityX * nx + a.VelocityY * ny;
                                double bVn = b.VelocityX * nx + b.VelocityY * ny;

                                // Kule muszą się zbliżać do siebie
                                if (aVn - bVn < 0) continue;

                                double ma = a.Mass, mb = b.Mass;
                                double sum = ma + mb;

                                // Wzory na zderzenie sprężyste
                                double newAVn = (aVn * (ma - mb) + 2 * mb * bVn) / sum;
                                double newBVn = (bVn * (mb - ma) + 2 * ma * aVn) / sum;

                                double deltaAVn = newAVn - aVn;
                                double deltaBVn = newBVn - bVn;

                                a.SetVelocity(a.VelocityX + deltaAVn * nx, a.VelocityY + deltaAVn * ny);
                                b.SetVelocity(b.VelocityX + deltaBVn * nx, b.VelocityY + deltaBVn * ny);
                            }
                        }

                        // Separacja (zapobiega "przyklejaniu się")
                        //double overlap = (minDist - dist) / 2.0;
                        //a.SetVelocity(a.VelocityX - overlap * nx,
                        //              a.VelocityY - overlap * ny);
                        // separacja przez przesunięcie – tylko powiadomienia, bez bezpośredniej zmiany X/Y
                        // (X/Y zmienia się przy kolejnym Move – wystarczy dla płynności)
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
