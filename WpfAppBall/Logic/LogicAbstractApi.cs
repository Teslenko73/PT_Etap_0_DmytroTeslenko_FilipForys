using System;
using System.Collections.Generic;
using WpfAppBall.Data;

namespace WpfAppBall.Logic
{
    /// <summary>
    /// Abstrakcyjne API warstwy Logika (Biznesowa).
    /// Warstwa Prezentacja (ViewModel/Model) zależy TYLKO od tej abstrakcji.
    /// Zawiera operacje:
    ///   - interaktywne: CreateBalls, StartSimulation, StopSimulation
    ///   - reaktywne:    Subscribe (powiadamianie o zmianie pozycji kul)
    /// </summary>
    public abstract class LogicAbstractApi
    {
        /// <summary>
        /// Fabryka - tworzy implementację warstwy Logika.
        /// Opcjonalny parametr dataApi umożliwia Dependency Injection w testach.
        /// </summary>
        public static LogicAbstractApi CreateApi(DataAbstractApi dataApi = null)
        {
            return new LogicImplementation.LogicApi(dataApi ?? DataAbstractApi.CreateApi());
        }

        // ─── Operacje interaktywne ────────────────────────────────────────────

        /// <summary>Tworzy podaną liczbę kul wewnątrz planszy.</summary>
        public abstract void CreateBalls(int count, double boardWidth, double boardHeight);

        /// <summary>Uruchamia timer symulacji (reaktywny).</summary>
        public abstract void StartSimulation(double boardWidth, double boardHeight);

        /// <summary>Zatrzymuje symulację i usuwa kule.</summary>
        public abstract void StopSimulation();

        // ─── Operacje reaktywne ───────────────────────────────────────────────

        /// <summary>
        /// Subskrybuje aktualizacje pozycji kul.
        /// Callback jest wywoływany co ~16 ms (≈60 FPS) z aktualną listą kul.
        /// </summary>
        public abstract void Subscribe(Action<IEnumerable<IBallDto>> onUpdate);

        /// <summary>Zwraca bieżący stan wszystkich kul (snapshot).</summary>
        public abstract IEnumerable<IBallDto> GetBalls();
    }

    /// <summary>
    /// DTO (Data Transfer Object) kuli - przenosi dane z warstwy Logika do Prezentacji.
    /// Warstwa Prezentacja nie widzi wewnętrznych typów warstwy Dane.
    /// </summary>
    public interface IBallDto
    {
        int Id { get; }
        double X { get; }
        double Y { get; }
        double Radius { get; }
    }
}