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

        [Fact]
        public void kolejnytest()
        {
            var ball = new Ball { X = 468, Y = 100 };
            var logic = new BallLogic(ball);

            // Pierwszy krok powoduje kolizję i odwrócenie prędkości na -5
            logic.UpdatePosition(500, 500);
            // Drugi krok 473 + (-5) = 468
            logic.UpdatePosition(500, 500);

            Assert.Equal(468, ball.X); // Kulka wróciła na 468 zamiast lecieć na 478
        }
    }

}
