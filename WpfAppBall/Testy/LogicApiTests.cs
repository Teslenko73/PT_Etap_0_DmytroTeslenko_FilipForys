using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using WpfAppBall.Data;
using WpfAppBall.Logic;

namespace WpfAppBall.Testy
{
    public class LogicApiTests
    {
        // ── Mock warstwy Danych ───────────────────────────────────────────────
        private class MockBall : IBallData
        {
            public int Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Radius { get; set; } = 15;
            public double Mass { get; set; } = Math.PI * 15 * 15;
            public double VelocityX { get; set; }
            public double VelocityY { get; set; }

            public void Move(double w, double h) { X += VelocityX; Y += VelocityY; }
            public void SetVelocity(double vx, double vy) { VelocityX = vx; VelocityY = vy; }
            public void NotifyMoved() { }
        }

        private class MockDataApi : DataAbstractApi
        {
            private readonly List<MockBall> _balls = new List<MockBall>();
            private Action<IBallData> _callback;
            private int _nextId = 1;

            public MockBall AddBall(double x, double y, double vx, double vy)
            {
                var b = new MockBall { Id = _nextId++, X = x, Y = y, VelocityX = vx, VelocityY = vy };
                _balls.Add(b);
                return b;
            }

            public void FireMoved(IBallData ball) => _callback?.Invoke(ball);

            public override IBallData CreateBall(double bw, double bh)
            {
                var b = new MockBall { Id = _nextId++, X = bw / 2, Y = bh / 2, VelocityX = 1, VelocityY = 0 };
                _balls.Add(b);
                return b;
            }
            public override IReadOnlyList<IBallData> GetAllBalls() => _balls.AsReadOnly();
            public override void ClearBalls() => _balls.Clear();
            public override void Subscribe(Action<IBallData> cb) => _callback = cb;
            // Dispose – mock nie ma zasobów, pusta implementacja 
        }

        // ── Testy ─────────────────────────────────────────────────────────────

        [Fact]
        public void CreateBalls_ShouldCallCreateBallNTimes()
        {
            var mock = new MockDataApi();
            var logic = LogicAbstractApi.CreateApi(mock);
            logic.CreateBalls(5, 800, 600);
            Assert.Equal(5, mock.GetAllBalls().Count);
        }

        [Fact]
        public void CreateBalls_ZeroCount_ShouldThrow()
        {
            var mock = new MockDataApi();
            var logic = LogicAbstractApi.CreateApi(mock);
            Assert.Throws<ArgumentOutOfRangeException>(() => logic.CreateBalls(0, 800, 600));
        }

        [Fact]
        public void CreateBalls_NegativeDimensions_ShouldThrow()
        {
            var mock = new MockDataApi();
            var logic = LogicAbstractApi.CreateApi(mock);
            Assert.Throws<ArgumentException>(() => logic.CreateBalls(3, -100, 600));
        }

        [Fact]
        public void Subscribe_ShouldReceiveUpdateWhenBallMoves()
        {
            var mock = new MockDataApi();
            var ball = mock.AddBall(100, 100, 1, 0);
            var logic = LogicAbstractApi.CreateApi(mock);

            IEnumerable<IBallDto> received = null;
            logic.Subscribe(dtos => received = dtos);

            mock.FireMoved(ball);

            Assert.NotNull(received);
            Assert.Equal(1, received.Count());
        }

        [Fact]
        public void GetBalls_ShouldReturnDtoForEachDataBall()
        {
            var mock = new MockDataApi();
            mock.AddBall(10, 20, 1, 0);
            mock.AddBall(50, 60, -1, 1);
            var logic = LogicAbstractApi.CreateApi(mock);

            var balls = logic.GetBalls().ToList();
            Assert.Equal(2, balls.Count);
        }

        [Fact]
        public void StopSimulation_ShouldClearBalls()
        {
            var mock = new MockDataApi();
            mock.AddBall(100, 100, 1, 0);
            var logic = LogicAbstractApi.CreateApi(mock);

            logic.StopSimulation();

            Assert.Equal(0, mock.GetAllBalls().Count);
        }

        [Fact]
        public void ElasticCollision_HeadOn_ShouldExchangeVelocities()
        {
            var mock = new MockDataApi();
            var ballA = mock.AddBall(100, 300, 2, 0);
            var ballB = mock.AddBall(128, 300, -2, 0); // dist=28 < 30

            var logic = LogicAbstractApi.CreateApi(mock);


            logic.StartSimulation(800, 600);

            mock.FireMoved(ballA);

            Assert.True(ballA.VelocityX < 0, "Kula A powinna zmienić kierunek");
            Assert.True(ballB.VelocityX > 0, "Kula B powinna zmienić kierunek");
        }

        [Fact]
        public void Logic_ShouldBounceBallOffWall_WhenBallExceedsBounds()
        {
            
            var mock = new MockDataApi();
            var ball = mock.AddBall(95, 50, 10, 0); 

            var logic = LogicAbstractApi.CreateApi(mock);
            logic.StartSimulation(100, 100); // 100x100

            
            ball.Move(100, 100); 
            mock.FireMoved(ball); 

            Assert.True(ball.VelocityX < 0, "Logika powinna zmienić zwrot prędkości po uderzeniu w ścianę");
        }
    }
}




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;
//using WpfAppBall.Data;
//using WpfAppBall.Logic;

//namespace WpfAppBall.Testy
//{
//    public class LogicApiTests
//    {
//        // ── Mock warstwy Danych ───────────────────────────────────────────────
//        private class MockBall : IBallData
//        {
//            public int Id { get; set; }
//            public double X { get; set; }
//            public double Y { get; set; }
//            public double Radius { get; set; } = 15;
//            public double Mass { get; set; } = Math.PI * 15 * 15;
//            public double VelocityX { get; set; }
//            public double VelocityY { get; set; }

//            public void Move(double w, double h) { X += VelocityX; Y += VelocityY; }
//            public void SetVelocity(double vx, double vy) { VelocityX = vx; VelocityY = vy; }
//            public void NotifyMoved() { }
//        }

//        private class MockDataApi : DataAbstractApi
//        {
//            private readonly List<MockBall> _balls = new List<MockBall>();
//            private Action<IBallData> _callback;
//            private int _nextId = 1;

//            public MockBall AddBall(double x, double y, double vx, double vy)
//            {
//                var b = new MockBall { Id = _nextId++, X = x, Y = y, VelocityX = vx, VelocityY = vy };
//                _balls.Add(b);
//                return b;
//            }

//            public void FireMoved(IBallData ball) => _callback?.Invoke(ball);

//            public override IBallData CreateBall(double bw, double bh)
//            {
//                var b = new MockBall { Id = _nextId++, X = bw / 2, Y = bh / 2, VelocityX = 1, VelocityY = 0 };
//                _balls.Add(b);
//                return b;
//            }
//            public override IReadOnlyList<IBallData> GetAllBalls() => _balls.AsReadOnly();
//            public override void ClearBalls() => _balls.Clear();
//            public override void Subscribe(Action<IBallData> cb) => _callback = cb;
//        }

//        // ── Testy ─

//        [Fact]
//        public void CreateBalls_ShouldCallCreateBallNTimes()
//        {
//            var mock = new MockDataApi();
//            var logic = LogicAbstractApi.CreateApi(mock);

//            logic.CreateBalls(5, 800, 600);

//            Assert.Equal(5, mock.GetAllBalls().Count);
//        }

//        [Fact]
//        public void CreateBalls_ZeroCount_ShouldThrow()
//        {
//            var mock = new MockDataApi();
//            var logic = LogicAbstractApi.CreateApi(mock);

//            Assert.Throws<ArgumentOutOfRangeException>(() => logic.CreateBalls(0, 800, 600));
//        }

//        [Fact]
//        public void CreateBalls_NegativeDimensions_ShouldThrow()
//        {
//            var mock = new MockDataApi();
//            var logic = LogicAbstractApi.CreateApi(mock);

//            Assert.Throws<ArgumentException>(() => logic.CreateBalls(3, -100, 600));
//        }

//        [Fact]
//        public void Subscribe_ShouldReceiveUpdateWhenBallMoves()
//        {
//            var mock = new MockDataApi();
//            var ball = mock.AddBall(100, 100, 1, 0);
//            var logic = LogicAbstractApi.CreateApi(mock);

//            IEnumerable<IBallDto> received = null;
//            logic.Subscribe(dtos => received = dtos);

//            mock.FireMoved(ball);

//            Assert.NotNull(received);
//            Assert.Equal(1, received.Count());
//        }

//        [Fact]
//        public void GetBalls_ShouldReturnDtoForEachDataBall()
//        {
//            var mock = new MockDataApi();
//            mock.AddBall(10, 20, 1, 0);
//            mock.AddBall(50, 60, -1, 1);
//            var logic = LogicAbstractApi.CreateApi(mock);

//            var balls = logic.GetBalls().ToList();

//            Assert.Equal(2, balls.Count);
//        }

//        [Fact]
//        public void StopSimulation_ShouldClearBalls()
//        {
//            var mock = new MockDataApi();
//            mock.AddBall(100, 100, 1, 0);
//            var logic = LogicAbstractApi.CreateApi(mock);

//            logic.StopSimulation();

//            Assert.Equal(0, mock.GetAllBalls().Count);
//        }

//        [Fact]
//        public void ElasticCollision_HeadOn_ShouldExchangeVelocities()
//        {
//            var mock = new MockDataApi();
//            var ballA = mock.AddBall(100, 300, 2, 0);
//            var ballB = mock.AddBall(128, 300, -2, 0);  // dist=28 < 2*15=30

//            var logic = LogicAbstractApi.CreateApi(mock);

//            mock.FireMoved(ballA);

//            Assert.True(ballA.VelocityX < 0, "Kula A powinna zmienić kierunek");
//            Assert.True(ballB.VelocityX > 0, "Kula B powinna zmienić kierunek");
//        }
//    }
//}
