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

        // ── Reaktywne – wywoływane przez wątek kuli ──────────────────────────
        private void OnBallMoved(IBallData movedBall)
        {
            // Sekcja krytyczna: tylko jeden wątek naraz sprawdza i rozwiązuje kolizje
            lock (_collisionLock)
            {
                ResolveCollisions(movedBall);
            }

            // Powiadomienie ViewModel (poza lockiem – nie blokujemy długo)
            var dtos = _data.GetAllBalls()
                            .Select(b => (IBallDto)new BallDto(b))
                            .ToList();
            _subscriber?.Invoke(dtos);
        }

        // ── Elastic collision 2D ─────────────────────────────────────────────
        private void ResolveCollisions(IBallData a)
        {
            var balls = _data.GetAllBalls();
            foreach (var b in balls)
            {
                if (b.Id == a.Id) continue;

                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double minDist = a.Radius + b.Radius;

                if (dist < minDist && dist > 1e-9)
                {
                    // Wektor normalny zderzenia
                    double nx = dx / dist;
                    double ny = dy / dist;

                    // Składowe prędkości wzdłuż normalnej
                    double aVn = a.VelocityX * nx + a.VelocityY * ny;
                    double bVn = b.VelocityX * nx + b.VelocityY * ny;

                    // Kolizja możliwa tylko jeśli kule zbliżają się do siebie
                    if (aVn - bVn < 0) continue;

                    double ma = a.Mass, mb = b.Mass;
                    double sum = ma + mb;

                    // Wzory na zderzenie sprężyste
                    double newAVn = (aVn * (ma - mb) + 2 * mb * bVn) / sum;
                    double newBVn = (bVn * (mb - ma) + 2 * ma * aVn) / sum;

                    double deltaAVn = newAVn - aVn;
                    double deltaBVn = newBVn - bVn;

                    a.SetVelocity(a.VelocityX + deltaAVn * nx,
                                  a.VelocityY + deltaAVn * ny);
                    b.SetVelocity(b.VelocityX + deltaBVn * nx,
                                  b.VelocityY + deltaBVn * ny);

                    // Separacja (zapobiega "przyklejaniu się")
                    double overlap = (minDist - dist) / 2.0;
                    a.SetVelocity(a.VelocityX - overlap * nx,
                                  a.VelocityY - overlap * ny);
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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using WpfAppBall.Data;

//namespace WpfAppBall.Logic.LogicImplementation
//{

//    internal class LogicApi : LogicAbstractApi
//    {
//        private readonly DataAbstractApi _data;
//        private Action<IEnumerable<IBallDto>> _subscriber;
//        private Timer _timer;

//        private double _boardWidth;
//        private double _boardHeight;

//        // czestotliwosc odswiezana
//        private const int TickMs = 45;

//        public LogicApi(DataAbstractApi data)
//        {
//            _data = data ?? throw new ArgumentNullException(nameof(data));

//            // Warstwa Dane reaktywnie powiadamia Logikę o każdym ruchu
//            _data.Subscribe(OnBallMoved);
//        }


//        public override void CreateBalls(int count, double boardWidth, double boardHeight)
//        {
//            if (count <= 0)
//                throw new ArgumentOutOfRangeException(nameof(count), "Liczba kul musi być > 0.");
//            if (boardWidth <= 0 || boardHeight <= 0)
//                throw new ArgumentException("Wymiary planszy muszą być dodatnie.");

//            _boardWidth = boardWidth;
//            _boardHeight = boardHeight;

//            _data.ClearBalls();
//            for (int i = 0; i < count; i++)
//                _data.CreateBall(boardWidth, boardHeight);
//        }

//        public override void StartSimulation(double boardWidth, double boardHeight)
//        {
//            _boardWidth = boardWidth;
//            _boardHeight = boardHeight;

//            DisposeTimer();
//            _timer = new Timer(_ => Tick(), null, 0, TickMs);
//        }

//        public override void StopSimulation()
//        {
//            DisposeTimer();
//            _data.ClearBalls();
//        }

//        //reaktywne

//        public override void Subscribe(Action<IEnumerable<IBallDto>> onUpdate)
//        {
//            _subscriber = onUpdate;
//        }

//        public override IEnumerable<IBallDto> GetBalls()
//        {
//            return _data.GetAllBalls().Select(b => (IBallDto)new BallDto(b)).ToList();
//        }


//        private void Tick()
//        {
//            _data.MoveAll(_boardWidth, _boardHeight);
//        }

//        private void OnBallMoved(IBallData _)
//        {
//            // Po każdym ruchu kuli powiadamiamy subskrybenta aktualną listą
//            var dtos = _data.GetAllBalls()
//                            .Select(b => (IBallDto)new BallDto(b))
//                            .ToList();
//            _subscriber?.Invoke(dtos);
//        }

//        private void DisposeTimer()
//        {
//            _timer?.Dispose();
//            _timer = null;
//        }
//    }

//    internal class BallDto : IBallDto
//    {
//        public int Id { get; }
//        public double X { get; }
//        public double Y { get; }
//        public double Radius { get; }

//        public BallDto(IBallData d)
//        {
//            Id = d.Id; X = d.X; Y = d.Y; Radius = d.Radius;
//        }
//    }
//}