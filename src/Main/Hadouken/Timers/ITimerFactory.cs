namespace Hadouken.Timers
{
    public interface ITimerFactory : IComponent
    {
        ITimer CreateTimer();
    }
}
