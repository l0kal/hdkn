using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http
{
    public class HttpFileServer : IHttpFileServer
    {
        private readonly IFileProvider _fileProvider;
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly string _binding;

        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>()
            {
                { ".css", "text/css" },
                { ".html", "text/html" },
                { ".js", "text/javascript" },
                { ".png", "image/png" }
            }; 

        public HttpFileServer(string binding, IFileProvider fileProvider)
        {
            _binding = binding;
            _fileProvider = fileProvider;
        }

        public void Start()
        {
            _httpListener.Prefixes.Add(_binding);
            _httpListener.Start();

            _httpListener.BeginGetContext(GetContext, null);
        }

        private void GetContext(IAsyncResult asyncResult)
        {
            try
            {
                var context = _httpListener.EndGetContext(asyncResult);
                Task.Run(() => ProcessRequest(context));
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _httpListener.BeginGetContext(GetContext, null);
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var data = _fileProvider.GetFile(context.Request.Url.AbsolutePath);

            if (data == null)
            {
                context.Response.StatusCode = 404;
            }
            else
            {
                var extension = Path.GetExtension(context.Request.Url.AbsolutePath);

                context.Response.ContentType = "text/html";

                if (!String.IsNullOrEmpty(extension) && MimeTypes.ContainsKey(extension))
                    context.Response.ContentType = MimeTypes[extension];

                context.Response.StatusCode = 200;
                context.Response.OutputStream.Write(data, 0, data.Length);
            }

            context.Response.OutputStream.Close();
            context.Response.Close();
        }

        public void Stop()
        {
            _httpListener.Stop();
        }
    }
}
