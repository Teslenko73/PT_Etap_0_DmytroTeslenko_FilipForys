using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Timers;

namespace WpfAppBall.Data.DataImplementation
{
    public class DiagnosticLogger : IDisposable
    {
        private readonly string _logFilePath;
        private readonly object _fileLock = new object();
        private readonly Timer _writeTimer;
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private const int WriteIntervalMs = 1000; // Zapis co 1 sekundę (1000 ms)

        public DiagnosticLogger(string fileName)
        {
            // Zapisujemy w dokumentach użytkownika w formacie ASCII
            _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            // INICJALIZACJA WYMAGANEGO TIMERA
            _writeTimer = new Timer(WriteIntervalMs);
            _writeTimer.Elapsed += FlushQueueToFile;
            _writeTimer.AutoReset = true;
            _writeTimer.Enabled = true;
        }

        // Metoda wywoływana błyskawicznie przez wątki kul
        public void Log(int ballId, double x, double y, double vx, double vy)
        {
            // Serializacja do prostego formatu tekstowego (ASCII / CSV)
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Ball ID: {ballId} | Pos: ({x:F2}, {y:F2}) | Vel: ({vx:F2}, {vy:F2}){Environment.NewLine}";

            // Bezpieczne wrzucenie do bufora (rozwiązuje problem braku przepustowości dysku)
            _logQueue.Enqueue(logEntry);
        }

        // METODA WYWOŁYWANA PRZEZ TIMER CO 1 SEKUNDĘ
        private void FlushQueueToFile(object sender, ElapsedEventArgs e)
        {
            if (_logQueue.IsEmpty) return;

            lock (_fileLock)
            {
    
                StringBuilder sb = new StringBuilder();

                while (_logQueue.TryDequeue(out string entry))
                {
                    sb.Append(entry);
                }

                if (sb.Length > 0)
                {
                    // Zapis do pliku w kodowaniu ASCII
                    File.AppendAllText(_logFilePath, sb.ToString(), Encoding.ASCII);
                }
            }
        }

        public void Dispose()
        {
            _writeTimer?.Stop();
            _writeTimer?.Dispose();
            // Zapisz to, co zostało w kolejce przed wyłączeniem
            FlushQueueToFile(null, null);
        }
    }
}