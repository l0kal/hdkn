using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Hosting;

using Hadouken.Data;
using Hadouken.Plugins;
using Hadouken.BitTorrent;
using NLog;
using Hadouken.Http;
using Hadouken.Http.Api;
using Hadouken.Configuration;

namespace Hadouken.Impl.Hosting
{
    [Component]
    public class DefaultHost : IHost
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private IDataRepository _data;
        private IBitTorrentEngine _torrentEngine;
        private IMigrationRunner _migratorRunner;
        private IPluginEngine _pluginEngine;

        private readonly IHttpFileServer _fileServer;
        private readonly IHttpApiServer _apiServer;

        public DefaultHost(IDataRepository data, IBitTorrentEngine torrentEngine, IMigrationRunner runner,
                           IPluginEngine pluginEngine,
                           IHttpFileServerFactory fileServerFactory,
                           IHttpApiServerFactory apiServerFactory,
            IBindingBuilder bindingBuilder)
        {
            _data = data;
            _torrentEngine = torrentEngine;
            _migratorRunner = runner;
            _pluginEngine = pluginEngine;

            _fileServer = fileServerFactory.Create(bindingBuilder.Build(),
                                                   new FileSystemProvider(HdknConfig.GetPath("Paths.WebUI")));

            _apiServer = apiServerFactory.Create(bindingBuilder.Build("api"), typeof (Kernel).Assembly);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.FatalException("Unhandled exception.", e.ExceptionObject as Exception);
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var result = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                          where asm.GetName().FullName == args.Name
                          select asm).FirstOrDefault();

            return result;
        }

        public void Load()
        {
            _migratorRunner.Up(this.GetType().Assembly);

            _torrentEngine.Load();
            _pluginEngine.Load();

            _apiServer.Start();
            _fileServer.Start();
        }

        public void Unload()
        {
            _fileServer.Stop();
            _apiServer.Stop();

            _pluginEngine.UnloadAll();

            _torrentEngine.Unload();
        }
    }
}
