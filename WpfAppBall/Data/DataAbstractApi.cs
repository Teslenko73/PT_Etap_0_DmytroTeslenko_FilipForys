using System;
using System.Collections.Generic;

namespace WpfAppBall.Data
{
    public abstract class DataAbstractApi : IDisposable
    {
        public static DataAbstractApi CreateApi()
        {
            return new DataImplementation.DataApi();
        }

        public abstract IBallData CreateBall(double boardWidth, double boardHeight);
        public abstract IReadOnlyList<IBallData> GetAllBalls();
        public abstract void ClearBalls();
        public abstract void Subscribe(Action<IBallData> onBallMoved);

        // IDisposable – implementacja domyślna (pusta) dla mocków w testach
        public virtual void Dispose() { }
    }

    public interface IBallData
    {
        int Id { get; }
        double X { get; }
        double Y { get; }
        double Radius { get; }
        double Mass { get; }
        double VelocityX { get; }
        double VelocityY { get; }
        void Move(double boardWidth, double boardHeight);
        void SetVelocity(double vx, double vy);
        //void NotifyMoved();
    }
}
//using System;
//using System.Collections.Generic;

//namespace WpfAppBall.Data
//{
//    public abstract class DataAbstractApi
//    {
//        public static DataAbstractApi CreateApi()
//        {
//            return new DataImplementation.DataApi();
//        }

//        public abstract IBallData CreateBall(double boardWidth, double boardHeight);
//        public abstract IReadOnlyList<IBallData> GetAllBalls();
//        public abstract void ClearBalls();
//        // Usunięte MoveAll – każda kula porusza się we własnym wątku
//        public abstract void Subscribe(Action<IBallData> onBallMoved);
//    }

//    public interface IBallData
//    {
//        int Id { get; }
//        double X { get; }
//        double Y { get; }
//        double Radius { get; }
//        double Mass { get; }          // NOWE
//        double VelocityX { get; }
//        double VelocityY { get; }

//        void Move(double boardWidth, double boardHeight);
//        void SetVelocity(double vx, double vy);   // potrzebne przy kolizjach
//        void NotifyMoved();                        // wywołuje callback
//    }
//}
