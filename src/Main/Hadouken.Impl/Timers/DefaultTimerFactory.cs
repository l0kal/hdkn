using Hadouken.Timers;

namespace Hadouken.Impl.Timers
{
    public class DefaultTimerFactory : ITimerFactory
    {
        public ITimer CreateTimer()
        {
            return new ThreadedTimer();
        }
    }
}
