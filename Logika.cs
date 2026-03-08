using System;

namespace PR_Ball
{
    internal class Logika
    {
        private readonly Ball _ball;
        private double _velX = 5;
        private double _velY = 5;

        public Logika(Ball ball)
        {
            _ball = ball;
        }

        public void UpdatePosition(double boardWidth, double boardHeight)
        {
            // Ruch kulki
            _ball.X += _velX;
            _ball.Y += _velY;

            // Średnica kulki (Radius * 2)
            double diameter = _ball.Radius * 2;

            // Odbicie od lewej i prawej ściany
            if (_ball.X <= 0 || _ball.X + diameter >= boardWidth)
            {
                _velX = -_velX;
            }

            // Odbicie od góry i dołu (Poprawione boardHeight!)
            if (_ball.Y <= 0 || _ball.Y + diameter >= boardHeight)
            {
                _velY = -_velY;
            }
        }

        public Ball GetBall() => _ball;
    }
}