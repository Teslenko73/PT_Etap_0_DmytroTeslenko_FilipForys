using System;
using Xunit;
using WpfAppBall.Data.DataImplementation;

namespace WpfAppBall.Testy
{
    public class DataApiTests
    {
        [Fact]
        public void Ball_ShouldChangePositionOnMove()
        {
            // Arrange
            var ball = new BallData(1, 100, 100, 5, -3, 15);
            double initialX = ball.X;
            double initialY = ball.Y;

            // Act - Zmieniono na wywołanie z parametrem deltaTime zgodnie z nową implementacją czasu rzeczywistego
            ball.Move(500, 500, 1.0);

            // Assert
            Assert.NotEqual(initialX, ball.X);
            Assert.NotEqual(initialY, ball.Y);
        }
    }
}