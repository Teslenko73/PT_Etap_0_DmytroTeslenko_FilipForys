using System;
using System.Threading;

namespace PR_Ball
{
    class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            Logika logika = new Logika(data.GetBall());

            // Symulacja okna o rozmiarach 500x300
            for (int i = 0; i < 100; i++)
            {
                logika.UpdatePosition(500, 300);
                var b = logika.GetBall();
                Console.WriteLine($"Pozycja kulki: X={b.X:F1}, Y={b.Y:F1}");
                Thread.Sleep(50); // Krótka pauza, żeby nadążyć wzrokiem
            }
        }
    }
}