﻿using System.Net;
using Hadouken.Common.Messaging;
using Hadouken.Http.Api.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Hadouken.Common.BitTorrent;
using Hadouken.BitTorrent;

namespace Hadouken.Http.Api
{
    public class TorrentsController : ApiController
    {
        private readonly IMessageBus _messageBus;
        private readonly IBitTorrentEngine _torrentEngine;

        public TorrentsController(IMessageBusFactory messageBusFactory, IBitTorrentEngine torrentEngine)
        {
            _messageBus = messageBusFactory.Create("hdkn");
            _torrentEngine = torrentEngine;
        }

        public object Get()
        {
            return new
                {
                    torrents = (from m in _torrentEngine.Managers.Values
                                select new
                                    {
                                        m.Torrent.Name,
                                        m.Torrent.Size,
                                        m.InfoHash,
                                        m.Label
                                    })
                };
        }

        public HttpResponseMessage Post([FromBody] PostTorrentDto post)
        {
            var data = Convert.FromBase64String(post.Data);
            var torrent = _torrentEngine.AddTorrent(data);

            if (!String.IsNullOrEmpty(post.Label))
                torrent.Label = post.Label;

            if (post.AutoStart)
                torrent.Start();

            using (var response = Request.CreateResponse(HttpStatusCode.Created))
            {
                //response.Headers.Location = ""; //TODO: fix
                return response;
            }
        }
    }
}
