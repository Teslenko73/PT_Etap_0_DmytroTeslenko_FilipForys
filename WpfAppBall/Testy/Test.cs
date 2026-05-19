using System;
using Xunit;
using WpfAppBall.Data.DataImplementation; // Dopasuj do swojego namespace

namespace WpfAppBall.Testy
{
    public class DataApiTests
    {
        [Fact]
        public void Ball_ShouldStayWithinBoundsAfterMove()
        {
            double width = 100;
            double height = 100;
            double radius = 15;

            // Tworzymy kulę na samej krawędzi, lecącą w stronę ściany
            var ball = new BallData(1, 95, 50, 10, 0, radius);

            // Act
            ball.Move(width, height);

            // Assert - Sprawdzamy, czy odbiła się i została zepchnięta do wnętrza planszy
            Assert.True(ball.X <= width - ball.Radius, "Kulka wyszła poza prawą granicę!");
            Assert.True(ball.VelocityX < 0, "Prędkość po odbiciu od prawej ściany powinna być ujemna!");
        }

        [Fact]
        public void Ball_ShouldChangePositionOnMove()
        {
            // Arrange
            var ball = new BallData(1, 100, 100, 5, -3, 15);
            double initialX = ball.X;
            double initialY = ball.Y;

            // Act
            ball.Move(500, 500);

            // Assert
            Assert.NotEqual(initialX, ball.X);
            Assert.NotEqual(initialY, ball.Y);
        }
    }
}