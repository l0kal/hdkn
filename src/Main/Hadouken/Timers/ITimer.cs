using System;

namespace Hadouken.Timers
{
    public interface ITimer
    {
        void SetCallback(int interval, Action callback);

        void Start();
        void Stop();
    }
}
