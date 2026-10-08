namespace GridForge.Core.Interfaces
{
    public interface IGameLoop
    {
        void Initialize();
        void Update(double deltaTime);
        void Render();
        void Stop();
    }
}