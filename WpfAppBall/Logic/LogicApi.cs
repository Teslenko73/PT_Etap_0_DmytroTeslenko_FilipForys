using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WpfAppBall.Data;

namespace WpfAppBall.Logic.LogicImplementation
{
    /// <summary>
    /// Konkretna implementacja warstwy Logika.
    /// Zarządza timerem symulacji i tłumaczy dane z warstwy Dane na DTO.
    /// </summary>
    internal class LogicApi : LogicAbstractApi
    {
        private readonly DataAbstractApi _data;
        private Action<IEnumerable<IBallDto>> _subscriber;
        private Timer _timer;

        private double _boardWidth;
        private double _boardHeight;

        // 16 ms ≈ 60 FPS - inżynierska miara płynności animacji
        private const int TickMs = 16;

        public LogicApi(DataAbstractApi data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));

            // Warstwa Dane reaktywnie powiadamia Logikę o każdym ruchu
            _data.Subscribe(OnBallMoved);
        }

        // ─── Operacje interaktywne ────────────────────────────────────────────

        public override void CreateBalls(int count, double boardWidth, double boardHeight)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Liczba kul musi być > 0.");
            if (boardWidth <= 0 || boardHeight <= 0)
                throw new ArgumentException("Wymiary planszy muszą być dodatnie.");

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

            DisposeTimer();
            _timer = new Timer(_ => Tick(), null, 0, TickMs);
        }

        public override void StopSimulation()
        {
            DisposeTimer();
            _data.ClearBalls();
        }

        // ─── Operacje reaktywne ───────────────────────────────────────────────

        public override void Subscribe(Action<IEnumerable<IBallDto>> onUpdate)
        {
            _subscriber = onUpdate;
        }

        public override IEnumerable<IBallDto> GetBalls()
        {
            return _data.GetAllBalls().Select(b => (IBallDto)new BallDto(b)).ToList();
        }

        // ─── Prywatne ─────────────────────────────────────────────────────────

        private void Tick()
        {
            _data.MoveAll(_boardWidth, _boardHeight);
        }

        private void OnBallMoved(IBallData _)
        {
            // Po każdym ruchu kuli powiadamiamy subskrybenta aktualną listą
            var dtos = _data.GetAllBalls()
                            .Select(b => (IBallDto)new BallDto(b))
                            .ToList();
            _subscriber?.Invoke(dtos);
        }

        private void DisposeTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    /// <summary>DTO - przenosi dane kuli z Logiki do Prezentacji.</summary>
    internal class BallDto : IBallDto
    {
        public int Id { get; }
        public double X { get; }
        public double Y { get; }
        public double Radius { get; }

        public BallDto(IBallData d)
        {
            Id = d.Id; X = d.X; Y = d.Y; Radius = d.Radius;
        }
    }
}