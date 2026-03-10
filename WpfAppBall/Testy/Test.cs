using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using WpfAppBall;


namespace WpfAppBall.Testy
{
    public class Test
    {
        [Fact] 
        public void Ball_ShouldMoveCorrect()
        {
      
            var ball = new Ball { X = 100, Y = 100 };
            var logic = new BallLogic(ball);

            logic.UpdatePosition(500, 500);

            Assert.Equal(105, ball.X);
            Assert.Equal(105, ball.Y);
        }
    }
}
