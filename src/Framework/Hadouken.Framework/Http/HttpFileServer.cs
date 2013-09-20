﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Hadouken.Framework.Http
{
    public class HttpFileServer : IHttpFileServer
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly string _baseDirectory;

        private static readonly IDictionary<string, string> MimeTypes = new Dictionary<string, string>()
        {
            {".html", "text/html"},
            {".css", "text/css"},
            {".js", "text/javascript"}
        };

        public HttpFileServer(string listenUri, string baseDirectory)
        {
            _baseDirectory = baseDirectory;
            _httpListener.Prefixes.Add(listenUri);
        }

        public void Open()
        {
            _httpListener.Start();
            _httpListener.BeginGetContext(GetContext, null);
        }

        public void Close()
        {
            _httpListener.Close();
        }

        private void GetContext(IAsyncResult ar)
        {
            var context = _httpListener.EndGetContext(ar);
            Task.Run(() => ProcessContext(context));
            _httpListener.BeginGetContext(GetContext, null);
        }

        private void ProcessContext(HttpListenerContext context)
        {
            var path = _baseDirectory + context.Request.Url.AbsolutePath;
            var extension = Path.GetExtension(path);

            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(reader.ReadToEnd());
                }

                context.Response.ContentType = "text/plain";

                if (MimeTypes.ContainsKey(extension))
                    context.Response.ContentType = MimeTypes[extension];

                context.Response.StatusCode = 200;
            }
            else
            {
                context.Response.StatusCode = 404;
            }

            context.Response.OutputStream.Close();
            context.Response.Close();
        }
    }
}