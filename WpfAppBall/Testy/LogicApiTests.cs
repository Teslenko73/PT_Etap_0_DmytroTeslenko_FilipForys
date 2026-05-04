using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using WpfAppBall.Data;
using WpfAppBall.Logic;

namespace WpfAppBall.Testy
{

    public class LogicApiTests
    {

        private class FakeDataApi : DataAbstractApi
        {
            private readonly List<FakeBall> _balls = new List<FakeBall>();
            private Action<IBallData> _callback;

            public int MoveAllCallCount { get; private set; } = 0;

            public override IBallData CreateBall(double boardWidth, double boardHeight)
            {
                var b = new FakeBall(_balls.Count + 1, boardWidth / 2, boardHeight / 2, 3.0, 2.0);
                _balls.Add(b);
                return b;
            }

            public override IReadOnlyList<IBallData> GetAllBalls() => _balls.AsReadOnly();
            public override void ClearBalls() => _balls.Clear();
            public override void Subscribe(Action<IBallData> cb) => _callback = cb;

            public override void MoveAll(double w, double h)
            {
                MoveAllCallCount++;
                foreach (var b in _balls) { b.Move(); _callback?.Invoke(b); }
            }
        }

        private class FakeBall : IBallData
        {
            public int Id { get; }
            public double X { get; private set; }
            public double Y { get; private set; }
            public double Radius { get; } = 15;
            public double VelocityX { get; }
            public double VelocityY { get; }

            public FakeBall(int id, double x, double y, double vx, double vy)
            { Id = id; X = x; Y = y; VelocityX = vx; VelocityY = vy; }

            public void Move() { X += VelocityX; Y += VelocityY; }
        }


        [Fact]
        public void CreateBalls_ShouldCreateCorrectCount()
        {
            var fake = new FakeDataApi();
            var logic = LogicAbstractApi.CreateApi(fake);

            logic.CreateBalls(5, 600, 400);

            Assert.Equal(5, fake.GetAllBalls().Count);
        }

        [Fact]
        public void CreateBalls_ThrowsOnZeroCount()
        {
            var logic = LogicAbstractApi.CreateApi(new FakeDataApi());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                logic.CreateBalls(0, 600, 400));
        }

        [Fact]
        public void CreateBalls_ThrowsOnNegativeCount()
        {
            var logic = LogicAbstractApi.CreateApi(new FakeDataApi());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                logic.CreateBalls(-1, 600, 400));
        }

        [Fact]
        public void CreateBalls_ThrowsOnInvalidBoardSize()
        {
            var logic = LogicAbstractApi.CreateApi(new FakeDataApi());

            Assert.Throws<ArgumentException>(() =>
                logic.CreateBalls(3, 0, 400));
        }

        [Fact]
        public void StopSimulation_ShouldClearBalls()
        {
            var fake = new FakeDataApi();
            var logic = LogicAbstractApi.CreateApi(fake);

            logic.CreateBalls(3, 600, 400);
            logic.StopSimulation();

            Assert.Empty(fake.GetAllBalls());
        }

        [Fact]
        public void GetBalls_ShouldReturnCreatedBalls()
        {
            var fake = new FakeDataApi();
            var logic = LogicAbstractApi.CreateApi(fake);

            logic.CreateBalls(4, 600, 400);

            Assert.Equal(4, logic.GetBalls().Count());
        }

        [Fact]
        public void GetBalls_ShouldReturnDtosWithPositiveRadius()
        {
            var fake = new FakeDataApi();
            var logic = LogicAbstractApi.CreateApi(fake);

            logic.CreateBalls(1, 600, 400);

            var ball = logic.GetBalls().First();
            Assert.True(ball.Radius > 0);
        }

        [Fact]
        public void Subscribe_ShouldReceiveUpdatesAfterStart()
        {
            var fake = new FakeDataApi();
            var logic = LogicAbstractApi.CreateApi(fake);

            int received = 0;
            logic.Subscribe(_ => Interlocked.Increment(ref received));

            logic.CreateBalls(1, 600, 400);
            logic.StartSimulation(600, 400);

            Thread.Sleep(200);
            logic.StopSimulation();

            Assert.True(received > 0, "Subskrybent nie otrzymał żadnej aktualizacji");
        }

        [Fact]
        public void StartSimulation_ShouldCallMoveAll()
        {
            var fake = new FakeDataApi();
            var logic = LogicAbstractApi.CreateApi(fake);

            logic.CreateBalls(2, 600, 400);
            logic.StartSimulation(600, 400);

            Thread.Sleep(150);
            logic.StopSimulation();

            Assert.True(fake.MoveAllCallCount > 0, "MoveAll nie zostało wywołane");
        }
    }
}