using System;
using System.Collections.Generic;
using WpfAppBall.Data;

namespace WpfAppBall.Logic
{
    public abstract class LogicAbstractApi
    {
        public static LogicAbstractApi CreateApi(DataAbstractApi dataApi = null)
        {
            return new LogicImplementation.LogicApi(dataApi ?? DataAbstractApi.CreateApi());
        }

        public abstract void CreateBalls(int count, double boardWidth, double boardHeight);
        public abstract void StartSimulation(double boardWidth, double boardHeight);
        public abstract void StopSimulation();
        public abstract IEnumerable<IBallDto> GetBalls();

        // ─── REAKTYWNE ZDARZENIE ZAMIAST METODY SUBSCRIBE ───
        // Pozwala warstwie Prezentacji (ViewModel) subskrybować się na strumień zmian położenia kul
        public abstract event EventHandler<IEnumerable<IBallDto>> BallsUpdated;
    }

    public interface IBallDto
    {
        int Id { get; }
        double X { get; }
        double Y { get; }
        double Radius { get; }
    }
}