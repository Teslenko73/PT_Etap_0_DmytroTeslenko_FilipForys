using System;

namespace WpfAppBall.Data.DataImplementation
{

    internal class BallData : IBallData
    {
        // Licznik do unikalnych ID
        private static int _idCounter = 0;
        private static readonly Random _rng = new Random();

        private readonly object _syncLock = new object();

        public int Id { get; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Radius { get; } = 15.0;
        public double VelocityX { get; private set; }
        public double VelocityY { get; private set; }


        public BallData(double boardWidth, double boardHeight)
        {
            Id = System.Threading.Interlocked.Increment(ref _idCounter);


            X = _rng.NextDouble() * (boardWidth - 2 * Radius) + Radius;
            Y = _rng.NextDouble() * (boardHeight - 2 * Radius) + Radius;


            double speed = _rng.NextDouble() * 4.0 + 2.0;
            double angle = _rng.NextDouble() * 2 * Math.PI;
            VelocityX = speed * Math.Cos(angle);
            VelocityY = speed * Math.Sin(angle);
        }

        internal BallData(int id, double x, double y, double vx, double vy, double radius = 15.0)
        {
            Id = id;
            X = x; Y = y;
            VelocityX = vx; VelocityY = vy;
            Radius = radius;
        }


        public void Move(double boardWidth, double boardHeight)
        {
            lock (_syncLock)
            {
                double nx = X + VelocityX;
                double ny = Y + VelocityY;

                // Odbicie od lewej / prawej
                if (nx - Radius < 0)
                {
                    nx = Radius;
                    VelocityX = Math.Abs(VelocityX);
                }
                else if (nx + Radius > boardWidth)
                {
                    nx = boardWidth - Radius;
                    VelocityX = -Math.Abs(VelocityX);
                }

                // Odbicie od górnej / dolnej
                if (ny - Radius < 0)
                {
                    ny = Radius;
                    VelocityY = Math.Abs(VelocityY);
                }
                else if (ny + Radius > boardHeight)
                {
                    ny = boardHeight - Radius;
                    VelocityY = -Math.Abs(VelocityY);
                }

                X = nx;
                Y = ny;
            }
        }
    }
}