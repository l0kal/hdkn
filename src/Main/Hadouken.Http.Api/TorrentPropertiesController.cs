using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using Hadouken.BitTorrent;
using System.Net.Http;
using Hadouken.Http.Api.Dto;

namespace Hadouken.Http.Api
{
    public class TorrentPropertiesController : ApiController
    {
        private readonly IBitTorrentEngine _torrentEngine;

        public TorrentPropertiesController(IBitTorrentEngine torrentEngine)
        {
            _torrentEngine = torrentEngine;
        }

        public object Get(string[] id = null)
        {
            if (id == null || id.Length == 0)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            return null;
        }

        public HttpResponseMessage Put([FromBody] PutTorrentPropertiesDto dto, [FromUri] string[] id = null)
        {
            return null;
        }
    }
}
