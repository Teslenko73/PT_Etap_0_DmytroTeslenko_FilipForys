using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace WpfAppBall.Data
{
    public class DiagnosticLogger : IDisposable
    {
        private readonly BlockingCollection<string> _queue;
        private readonly Thread _writerThread;
        private readonly string _filePath;
        private bool _disposed = false;

        public DiagnosticLogger(string filePath = "diagnostic.log")
        {
            _filePath = filePath;
            _queue = new BlockingCollection<string>(boundedCapacity: 1000);

            _writerThread = new Thread(WriteLoop)
            {
                IsBackground = true,
                Name = "DiagnosticLogger-Writer"
            };
            _writerThread.Start();
        }

        public void Log(int ballId, double x, double y, double vx, double vy)
        {
            if (_disposed) return;

            string entry = string.Format(
                CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss.fff};Ball={1};X={2:F3};Y={3:F3};VX={4:F3};VY={5:F3}",
                DateTime.Now, ballId, x, y, vx, vy);

            _queue.TryAdd(entry);
        }

        private void WriteLoop()
        {
            try
            {
                using (var fs = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fs, Encoding.ASCII))
                {
                    writer.AutoFlush = false;

                    foreach (var entry in _queue.GetConsumingEnumerable())
                    {
                        writer.WriteLine(entry);

                        if (_queue.Count == 0)
                            writer.Flush();
                    }

                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DiagnosticLogger] Błąd zapisu: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _queue.CompleteAdding();
            _writerThread.Join(timeout: TimeSpan.FromSeconds(3));
            _queue.Dispose();
        }
    }
}