using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hadouken.Reflection;
using System.Net;
using System.IO;
using Hadouken.IO;
using Hadouken.Configuration;
using Ionic.Zip;
using NLog;
using Hadouken.Security;

namespace Hadouken.Http.HttpServer
{
    [Component]
    public class DefaultHttpServer : IHttpServer
    {
        private static readonly int DefaultPort = 8081;
        private static readonly string DefaultBinding = "http://localhost:{port}/";

        private static readonly IDictionary<string, string> MimeTypes = new Dictionary<string, string>()
            {
                { ".html", "text/html" },
                { ".css", "text/css" },
                { ".js", "text/javascript" },
                { ".png", "image/png" }
            }; 

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IFileSystem _fileSystem;
        private readonly IKeyValueStore _keyValueStore;
        private readonly IRegistryReader _registryReader;

        private HttpListener _listener;
        private string _webUIPath;

        private static readonly string TokenCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly int TokenLength = 40;
        
        public DefaultHttpServer(IKeyValueStore keyValueStore, IRegistryReader registryReader, IFileSystem fileSystem)
        {
            _keyValueStore = keyValueStore;
            _registryReader = registryReader;
            _fileSystem = fileSystem;
        }

        public void Start()
        {
            var binding = GetBinding();

            _listener = new HttpListener();
            _listener.Prefixes.Add(binding);
            _listener.AuthenticationSchemes = AuthenticationSchemes.Basic;

            UnzipWebUI();

            try
            {
                _listener.Start();
            }
            catch(HttpListenerException e)
            {
                Logger.FatalException("Could not start the HTTP server interface. HTTP server NOT up and running.", e);
                return;
            }

            _listener.BeginGetContext(BeginGetContext, null);

            Logger.Info("HTTP server up and running on address " + ListenUri);
        }

        public void Stop()
        {
            if (_listener == null || !_listener.IsListening) return;

            _listener.Stop();
            _listener.Close();
        }

        public Uri ListenUri
        {
            get
            {
                return _listener.IsListening ? new Uri(_listener.Prefixes.First()) : null;
            }
        }

        private string GetBinding()
        {
            var binding = _registryReader.ReadString("webui.binding", DefaultBinding);
            var port = _registryReader.ReadInt("webui.port", DefaultPort);

            // Allow overriding from application configuration file.
            if (HdknConfig.ConfigManager.AllKeys.Contains("WebUI.Binding"))
                binding = HdknConfig.ConfigManager["WebUI.Binding"];

            if (HdknConfig.ConfigManager.AllKeys.Contains("WebUI.Port"))
                port = Convert.ToInt32(HdknConfig.ConfigManager["WebUI.Port"]);

            return binding.Replace("{port}", port.ToString());;
        }

        private void BeginGetContext(IAsyncResult ar)
        {
            try
            {
                var context = _listener.EndGetContext(ar);

                if (IsAuthenticatedUser(context.User.Identity as HttpListenerBasicIdentity))
                    Task.Factory.StartNew(() => ProcessRequest(new HttpContext(context)));

                _listener.BeginGetContext(BeginGetContext, null);
            }
            catch (HttpListenerException)
            {
                // ignore this exception
            }
            catch (Exception e)
            {
                Logger.ErrorException("Error when getting context", e);
            }
        }

        private void ProcessRequest(IHttpContext context)
        {
            try
            {
                Logger.Trace("Incoming request to {0}", context.Request.Url);

                string url = context.Request.Url.AbsolutePath;

                var result = (((url == "/api" || url == "/api/") && context.Request.QueryString["action"] != null)
                                    ? FindAndExecuteAction(context)
                                    : CheckFileSystem(context));

                if (result != null)
                {
                    result.Execute(context);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "404 - File not found";
                }

                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
                context.Response.Close();
            }
            catch(Exception e)
            {
                Logger.ErrorException("Error when processing request.", e);
                context.Response.Error(e);
            }
        }

        private void UnzipWebUI()
        {
            _webUIPath = HdknConfig.GetPath("Paths.WebUI");

            string uiZip = Path.Combine(HdknConfig.ApplicationDirectory, "webui.zip");

            Logger.Debug("Checking if webui.zip exists at {0}", uiZip);

            if (_fileSystem.FileExists(uiZip))
            {
                string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _fileSystem.CreateDirectory(path);

                Logger.Info("Extracting webui.zip to {0}", path);

                using (var zip = ZipFile.Read(uiZip))
                {
                    zip.ExtractAll(path);
                }

                _webUIPath = path;
            }
        }

        private ActionResult CheckFileSystem(IHttpContext context)
        {
            var path = _webUIPath + (context.Request.Url.AbsolutePath == "/" ? "/index.html" : context.Request.Url.AbsolutePath);

            if (_fileSystem.FileExists(path))
            {
                var contentType = "text/html";

                if (MimeTypes.ContainsKey(Path.GetExtension(path)))
                    contentType = MimeTypes[Path.GetExtension(path)];

                return new ContentResult { Content = _fileSystem.ReadAllBytes(path), ContentType = contentType };
            }

            return null;
        }

        private bool IsAuthenticatedUser(HttpListenerBasicIdentity identity)
        {
            if (identity == null)
                return false;

            var usr = _keyValueStore.Get<string>("auth.username");
            var pwd = _keyValueStore.Get<string>("auth.password");
            var comp = StringComparison.InvariantCultureIgnoreCase;

            return (String.Equals(usr, identity.Name, comp) &&
                    String.Equals(pwd, Hash.Generate(identity.Password), comp));
        }

        private ActionResult FindAndExecuteAction(IHttpContext context)
        {
            string actionName = context.Request.QueryString["action"];

            if (actionName == "gettoken")
                return GenerateCSRFToken();

            var action = (from a in Kernel.Resolver.GetAll<IApiAction>()
                          where a.GetType().HasAttribute<ApiActionAttribute>()
                          let attr = a.GetType().GetAttribute<ApiActionAttribute>()
                          where attr != null && attr.Name == actionName
                          select a).SingleOrDefault();

            if (action != null)
            {
                try
                {
                    action.Context = context;
                    return action.Execute();
                }
                catch (Exception e)
                {
                    Logger.ErrorException(String.Format("Could not execute action {0}", action.GetType().FullName), e);
                }
            }

            return null;
        }

        private ActionResult GenerateCSRFToken()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var sb = new StringBuilder();

            for (int i = 0; i < TokenLength; i++)
            {
                sb.Append(TokenCharacters[rnd.Next(0, TokenCharacters.Length - 1)]);
            }

            return new JsonResult() { Data = sb.ToString() };
        }
    }
}
