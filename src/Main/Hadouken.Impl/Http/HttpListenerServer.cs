using Hadouken.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hadouken.Common.Security;
using Hadouken.Http;
using Hadouken.Common;
using Hadouken.Configuration;
using Ionic.Zip;
using NLog;

namespace Hadouken.Impl.Http
{
    [Component(ComponentType.Singleton)]
    public class HttpListenerServer : IHttpFileSystemServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HttpListener _httpListener;

        private readonly IFileSystem _fileSystem;
        private readonly IKeyValueStore _keyValueStore;

        private static readonly IDictionary<string, string> MimeTypes = new Dictionary<string, string>()
            {
                {".css", "text/css"},
                {".js",  "text/javascript"},
                {".html", "text/html"}
            };

        public HttpListenerServer(IFileSystem fileSystem, IKeyValueStore keyValueStore, IEnvironment environment)
        {
            _fileSystem = fileSystem;
            _keyValueStore = keyValueStore;

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(environment.HttpBinding);
            _httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;

            RootDirectory = HdknConfig.GetPath("Paths.WebUI");

            try
            {
                Unzip();
            }
            catch (Exception e)
            {
                // Enter failure mode
                Logger.ErrorException("Could not unzip.", e);
            }
        }

        public string RootDirectory { get; private set; }

        public void Start()
        {
            if (_httpListener == null) return;

            Logger.Info("RootDirectory=" + Path.GetFullPath(RootDirectory));

            _httpListener.Start();
            _httpListener.BeginGetContext(BeginGetContext, null);
        }

        private void Unzip()
        {
            var zip = Path.Combine(HdknConfig.WorkingDirectory, "webui.zip");

            if (_fileSystem.FileExists(zip))
            {
                Logger.Debug("Unzipping web ui");

                // Get a temporary path
                var temporaryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                if (!_fileSystem.DirectoryExists(temporaryPath))
                    _fileSystem.CreateDirectory(temporaryPath);

                // Unzip the webui.zip
                using (var zipFile = ZipFile.Read(zip))
                {
                    zipFile.ExtractAll(temporaryPath);
                }

                RootDirectory = temporaryPath;
            }
        }

        private void BeginGetContext(IAsyncResult ar)
        {
            try
            {
                var context = _httpListener.EndGetContext(ar);

                if (IsAuthenticated(context.User.Identity as HttpListenerBasicIdentity))
                    Task.Factory.StartNew(() => OnHttpRequest(context));

                _httpListener.BeginGetContext(BeginGetContext, null);
            }
            catch (HttpListenerException)
            {
                //TODO: better catch clause
            }
        }

        private void OnHttpRequest(HttpListenerContext context)
        {
            try
            {
                var pathSegments = context.Request.Url.Segments.Skip(1).Select(s => s.Replace("/", "")).ToList();
                pathSegments.Insert(0, RootDirectory);

                // Check file system for file
                var file = Path.Combine(pathSegments.ToArray());

                if (_fileSystem.FileExists(file))
                {
                    var contentType = "";

                    if (MimeTypes.ContainsKey(Path.GetExtension(file)))
                        contentType = MimeTypes[Path.GetExtension(file)];

                    var data = _fileSystem.ReadAllBytes(file);

                    context.Response.StatusCode = 200;
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.ContentType = contentType;
                    context.Response.OutputStream.Write(data, 0, data.Length);
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
            catch (Exception e)
            {
                try
                {
                    var data = Encoding.UTF8.GetBytes(e.ToString());

                    context.Response.StatusCode = 500;
                    context.Response.OutputStream.Write(data, 0, data.Length);
                }
                catch (Exception e2)
                {
                    Logger.ErrorException("Could not write exception to response", e2);
                }
            }

            context.Response.OutputStream.Close();
            context.Response.Close();
        }

        private bool IsAuthenticated(HttpListenerBasicIdentity identity)
        {
            if (identity == null)
                return false;

            var usr = _keyValueStore.Get<string>("auth.username");
            var pwd = _keyValueStore.Get<string>("auth.password");
            var comp = StringComparison.InvariantCultureIgnoreCase;

            return (String.Equals(usr, identity.Name, comp) && String.Equals(pwd, Hash.Generate(identity.Password)));
        }

        public void Stop()
        {
            _httpListener.Stop();
        }
    }
}
