using System;
using System.Collections.Generic;

namespace WpfAppBall.Data.DataImplementation
{
    internal class DataApi : DataAbstractApi
    {
        private readonly List<BallData> _balls = new List<BallData>();
        private readonly object _lock = new object();
        private Action<IBallData> _onBallMoved;

        private double _boardWidth, _boardHeight;

        public override void Subscribe(Action<IBallData> onBallMoved)
        {
            _onBallMoved = onBallMoved;
        }

        public override IBallData CreateBall(double boardWidth, double boardHeight)
        {
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;

            var ball = new BallData(boardWidth, boardHeight, b => _onBallMoved?.Invoke(b));
            lock (_lock) { _balls.Add(ball); }

            ball.StartThread(boardWidth, boardHeight);
            return ball;
        }

        public override IReadOnlyList<IBallData> GetAllBalls()
        {
            lock (_lock) { return _balls.AsReadOnly(); }
        }

        public override void ClearBalls()
        {
            List<BallData> snapshot;
            lock (_lock)
            {
                snapshot = new List<BallData>(_balls);
                _balls.Clear();
            }
            foreach (var b in snapshot) b.StopThread();
        }
    }
}

//using System;
//using System.Collections.Generic;

//namespace WpfAppBall.Data.DataImplementation
//{

//    internal class DataApi : DataAbstractApi
//    {
//        private readonly List<BallData> _balls = new List<BallData>();
//        private readonly object _lock = new object();
//        private Action<IBallData> _onBallMoved;

//        public override IBallData CreateBall(double boardWidth, double boardHeight)
//        {
//            var ball = new BallData(boardWidth, boardHeight);
//            lock (_lock) { _balls.Add(ball); }
//            return ball;
//        }

//        public override IReadOnlyList<IBallData> GetAllBalls()
//        {
//            lock (_lock) { return _balls.AsReadOnly(); }
//        }

//        public override void ClearBalls()
//        {
//            lock (_lock) { _balls.Clear(); }
//        }

//        public override void MoveAll(double boardWidth, double boardHeight)
//        {
//            List<BallData> snapshot;
//            lock (_lock) { snapshot = new List<BallData>(_balls); }

//            foreach (var ball in snapshot)
//            {
//                ball.Move(boardWidth, boardHeight);
//                _onBallMoved?.Invoke(ball);
//            }
//        }

//        public override void Subscribe(Action<IBallData> onBallMoved)
//        {
//            _onBallMoved = onBallMoved;
//        }
//    }
//}
