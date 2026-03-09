using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBall
{
    internal class BallLogic
    {
        private readonly Ball _ball;

        private double _velocityX = 5;
        private double _velocityY = 5;

        public BallLogic(Ball ball)
        {
            _ball = ball;
        }

        public void UpdatePosition(double boardWidth, double boardHeight)
        {
            _ball.X += _velocityX;
            _ball.Y += _velocityY;

            double diameter = _ball.Radius * 2;

            if (_ball.X <= 0 || _ball.X + diameter >= boardWidth)
            {
                _velocityX = -_velocityX;
            }

            if (_ball.Y <= 0 || _ball.Y + diameter >= boardHeight)
            {
                _velocityY = -_velocityY;
            }
        }

        public Ball GetBall()
        {
            return _ball;
        }
    }
}