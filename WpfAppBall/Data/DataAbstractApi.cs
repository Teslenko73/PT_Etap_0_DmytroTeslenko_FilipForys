using System;
using System.Collections.Generic;

namespace WpfAppBall.Data
{
    /// <summary>
    /// Abstrakcyjne API warstwy Dane.
    /// Warstwa Logika i testy zależą TYLKO od tej abstrakcji - nigdy od konkretnej implementacji.
    /// Wzorzec: Abstract Factory + Dependency Injection przez fabrykę.
    /// </summary>
    public abstract class DataAbstractApi
    {
        /// <summary>
        /// Fabryka tworząca domyślną implementację warstwy Dane.
        /// Umożliwia wstrzykiwanie zależności (Dependency Injection).
        /// </summary>
        public static DataAbstractApi CreateApi()
        {
            return new DataImplementation.DataApi();
        }

        // ─── Operacje na kulach ───────────────────────────────────────────────

        /// <summary>Tworzy nową kulę z losowym położeniem w granicach planszy.</summary>
        public abstract IBallData CreateBall(double boardWidth, double boardHeight);

        /// <summary>Zwraca listę wszystkich kul (snapshot).</summary>
        public abstract IReadOnlyList<IBallData> GetAllBalls();

        /// <summary>Usuwa wszystkie kule.</summary>
        public abstract void ClearBalls();

        /// <summary>Przesuwa wszystkie kule o jeden krok i odbija od ścian.</summary>
        public abstract void MoveAll(double boardWidth, double boardHeight);

        /// <summary>
        /// Subskrybuje zdarzenie zmiany pozycji dowolnej kuli.
        /// Reaktywne API warstwy Dane.
        /// </summary>
        public abstract void Subscribe(Action<IBallData> onBallMoved);
    }

    /// <summary>
    /// Dane jednej kuli - abstrakcja (interfejs).
    /// Warstwa wyżej nigdy nie widzi konkretnego typu BallData.
    /// </summary>
    public interface IBallData
    {
        int Id { get; }
        double X { get; }
        double Y { get; }
        double Radius { get; }
        double VelocityX { get; }
        double VelocityY { get; }
    }
}