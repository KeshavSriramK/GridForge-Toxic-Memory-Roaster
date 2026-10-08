namespace GridForge.Core.Interfaces
{
    public class EngineTime
    {
        public double DeltaTime { get; private set; }
        public double TotalTime { get; private set; }

        public void UpdateTime(double elapsedSeconds)
        {
            DeltaTime = elapsedSeconds;
            TotalTime += elapsedSeconds;
        }
    }
}