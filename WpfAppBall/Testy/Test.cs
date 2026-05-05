using Xunit;
using WpfAppBall.Data;

namespace WpfAppBall.Testy
{
    public class DataApiTests
    {
        [Fact]
        public void Ball_ShouldStayWithinBoundsAfterMove()
        {
            // Arrange
            var dataApi = DataAbstractApi.CreateApi();
            double width = 100;
            double height = 100;

            // Tworzymy kulę przez API
            var ball = dataApi.CreateBall(width, height);

            // Act
            // Wykonujemy serię ruchów
            for (int i = 0; i < 100; i++)
            {
                dataApi.MoveAll(width, height);
            }

            // Assert
            // Sprawdzamy czy kula nadal jest na planszy (uwzględniając promień)
            Assert.True(ball.X >= ball.Radius && ball.X <= width - ball.Radius);
            Assert.True(ball.Y >= ball.Radius && ball.Y <= height - ball.Radius);
        }

        [Fact]
        public void Ball_ShouldChangePositionOnMove()
        {
            // Arrange
            var dataApi = DataAbstractApi.CreateApi();
            var ball = dataApi.CreateBall(500, 500);
            double initialX = ball.X;
            double initialY = ball.Y;

            // Act
            dataApi.MoveAll(500, 500);

            // Assert
            // Kula powinna zmienić położenie
            Assert.NotEqual(initialX, ball.X);
            Assert.NotEqual(initialY, ball.Y);
        }
    }
}