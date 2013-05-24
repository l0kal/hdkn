using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Hadouken.Common.Data;
using Hadouken.Common.Http;
using Hadouken.Hosting;

using Hadouken.Data;
using Hadouken.Http;
using Hadouken.Plugins;
using Hadouken.BitTorrent;
using NLog;
using Hadouken.Common;
using Hadouken.Configuration;
using Hadouken.Messaging;

namespace Hadouken.Impl.Hosting
{
    [Component]
    public class DefaultHadoukenHost : IHadoukenHost
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnvironment _environment;
        private readonly IBitTorrentEngine _torrentEngine;
        private readonly IKeyValueStore _keyValueStore;
        private readonly IMigrationRunner _migratorRunner;
        private readonly IPluginEngine _pluginEngine;
        private readonly IMessageBus _messageBus;

        private readonly IHttpWebApiServerFactory _serverFactory;
        private readonly IHttpFileSystemServer _httpServer;
        private readonly IHttpHubServer _httpHubServer;
        private IHttpWebApiServer _webApiServer;

        public DefaultHadoukenHost(IEnvironment environment,
                                   IKeyValueStore keyValueStore,
                                   IBitTorrentEngine torrentEngine,
                                   IMigrationRunner runner,
                                   IPluginEngine pluginEngine,
                                   IHttpFileSystemServer httpServer,
                                   IHttpWebApiServerFactory httpServerFactory,
                                   IHttpHubServer httpHubServer,
                                   IMessageBus messageBus)
        {
            _environment = environment;
            _keyValueStore = keyValueStore;
            _torrentEngine = torrentEngine;
            _serverFactory = httpServerFactory;
            _httpServer = httpServer;
            _migratorRunner = runner;
            _pluginEngine = pluginEngine;
            _httpHubServer = httpHubServer;
            _messageBus = messageBus;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.FatalException("Unhandled exception.", e.ExceptionObject as Exception);
        }

        public void Load()
        {
            Logger.Trace("Load()");

            Logger.Debug("Running migrations");
            _migratorRunner.Up(this.GetType().Assembly);

            Logger.Debug("Starting the hub server");
            _httpHubServer.Start("http://localhost:8081/superduperhub/");

            Logger.Debug("Loading the IBitTorrentEngine implementation");
            _torrentEngine.Load();

            //Logger.Debug("Loading the IPluginEngine implementation");
            _pluginEngine.Load();

            var httpUser = _keyValueStore.Get<string>("auth.username");
            var httpPass = _keyValueStore.Get<string>("auth.password");

            _webApiServer = _serverFactory.Create(_environment.HttpBinding + "api",
                                                     new NetworkCredential(httpUser, httpPass),
                                                     AppDomain.CurrentDomain.GetAssemblies());

            Logger.Debug("Starting the HTTP API server");
            _webApiServer.Start();
            
            Logger.Debug("Starting the HTTP UI server");
            _httpServer.Start();

            _messageBus.Publish(new Hadouken.BitTorrent.Messages.TorrentAddedMessage() {Name = "hejhejhej"});
        }

        public void Unload()
        {
            Logger.Trace("Unload()");

            _pluginEngine.UnloadAll();

            _torrentEngine.Unload();

            _httpServer.Stop();

            _webApiServer.Stop();
        }
    }
}
