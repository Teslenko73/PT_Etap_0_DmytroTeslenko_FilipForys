using System;
using System.Collections.Generic;

namespace WpfAppBall.Data
{

    public abstract class DataAbstractApi
    {

        public static DataAbstractApi CreateApi()
        {
            return new DataImplementation.DataApi();
        }

        public abstract IBallData CreateBall(double boardWidth, double boardHeight);

        public abstract IReadOnlyList<IBallData> GetAllBalls();


        public abstract void ClearBalls();


        public abstract void MoveAll(double boardWidth, double boardHeight);


        public abstract void Subscribe(Action<IBallData> onBallMoved);
    }


    public interface IBallData
    {
        int Id { get; }
        double X { get; }
        double Y { get; }
        double Radius { get; }
        double VelocityX { get; }
        double VelocityY { get; }
    }
}