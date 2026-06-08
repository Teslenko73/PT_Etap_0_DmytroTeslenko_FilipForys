using System;
using System.IO;
using System.Threading;
using WpfAppBall.Data;
using WpfAppBall.Data.DataImplementation;
using Xunit;

namespace WpfAppBall.Testy
{
    public class DataLayerTests
    {
        // ── BallData – real-time Move ─────────────────────────────────────────

        [Fact]
        public void Ball_Move_WithDeltaTime_ShouldScaleDisplacement()
        {
            var ball = new BallData(1, 100, 100, 10, 0, 15);

            // deltaTime = 2.0 → przesunięcie = 10 * 2.0 = 20
            ball.Move(500, 500, 2.0);

            Assert.Equal(120.0, ball.X, precision: 5);
        }

        [Fact]
        public void Ball_ShouldMoveLinearlyWithoutWallLogicInDataLayer()
        {
            // Arrange
            var ball = new BallData(1, 95, 50, 10, 0, 15);

            // Act
            ball.Move(100, 100, 1.0); // Nowe Move tylko przesuwa: 95 + 10 = 105

            // Assert
            Assert.Equal(105.0, ball.X, precision: 5);
            Assert.Equal(10.0, ball.VelocityX, precision: 5); // Prędkość się nie zmienia w warstwie danych
        }

        [Fact]
        public void Ball_ShouldChangePositionOnMove()
        {
            var ball = new BallData(1, 100, 100, 5, -3, 15);
            double initialX = ball.X;
            double initialY = ball.Y;

            ball.Move(500, 500, 1.0);

            Assert.NotEqual(initialX, ball.X);
            Assert.NotEqual(initialY, ball.Y);
        }

        [Fact]
        public void Ball_SetVelocity_ShouldUpdateVelocity()
        {
            var ball = new BallData(1, 100, 100, 0, 0, 15);
            ball.SetVelocity(3.5, -2.1);

            Assert.Equal(3.5, ball.VelocityX, precision: 5);
            Assert.Equal(-2.1, ball.VelocityY, precision: 5);
        }

        // ── DiagnosticLogger ──────────────────────────────────────────────────

        [Fact]
        public void Logger_ShouldWriteEntryToFile()
        {
            string path = Path.GetTempFileName();
            try
            {
                using (var logger = new DiagnosticLogger(path))
                {
                    logger.Log(1, 100.0, 200.0, 1.5, -2.5);
                    // Dajemy czas wątkowi piszącemu na zapis
                    Thread.Sleep(200);
                }

                string content = File.ReadAllText(path);
                Assert.Contains("Ball=1", content);
                Assert.Contains("X=100.000", content);
                Assert.Contains("Y=200.000", content);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Logger_WhenBufferFull_ShouldNotBlockCaller()
        {
            string path = Path.GetTempFileName();
            try
            {
                // boundedCapacity=1000 w loggerze – wysyłamy 2000 wpisów bardzo szybko
                using (var logger = new DiagnosticLogger(path))
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    for (int i = 0; i < 2000; i++)
                        logger.Log(i, i, i, 1, 1);

                    sw.Stop();

                    // Wywołania TryAdd nie powinny blokować – cały cykl < 1s
                    Assert.True(sw.ElapsedMilliseconds < 1000,
                        $"Logger zablokował wątek kuli! Czas: {sw.ElapsedMilliseconds} ms");
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Logger_EntryShouldBeAsciiEncoded()
        {
            string path = Path.GetTempFileName();
            try
            {
                using (var logger = new DiagnosticLogger(path))
                {
                    logger.Log(42, 1.1, 2.2, 3.3, 4.4);
                    Thread.Sleep(200);
                }

                // Odczytujemy jako bajty i sprawdzamy brak non-ASCII
                byte[] bytes = File.ReadAllBytes(path);
                foreach (byte b in bytes)
                    Assert.True(b < 128, $"Znaleziono bajt non-ASCII: {b}");
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}