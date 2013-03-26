using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Plugins;
using NLog;
using Hadouken.Configuration;
using Hadouken.Timers;

namespace HdknPlugins.AutoUpdate
{
    [Plugin("autoupdate", "1.0")]
    public class AutoUpdatePlugin : IPlugin
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IKeyValueStore _keyValueStore;
        private readonly ITimer _timer;

        public AutoUpdatePlugin(IKeyValueStore keyValueStore, ITimerFactory timerFactory)
        {
            _keyValueStore = keyValueStore;
            _timer = timerFactory.CreateTimer();
        }

        public void Load()
        {
            var pollInterval = _keyValueStore.Get("plugins.autoupdate.pollIntervall", 60);

            _timer.SetCallback(pollInterval, CheckForUpdates);
            _timer.Start();
        }

        private void CheckForUpdates()
        {
            
        }

        public void Unload()
        {
            _timer.Stop();
        }
    }
}
