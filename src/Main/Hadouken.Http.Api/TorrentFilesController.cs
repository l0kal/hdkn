using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using Hadouken.BitTorrent;
using System.Net.Http;

namespace Hadouken.Http.Api
{
    public class TorrentFilesController : ApiController
    {
        private readonly IBitTorrentEngine _torrentEngine;

        public TorrentFilesController(IBitTorrentEngine torrentEngine)
        {
            _torrentEngine = torrentEngine;
        }

        public object Get(string id)
        {
            if (_torrentEngine.Managers.ContainsKey(id))
            {
                return new
                    {
                        files = new object[]
                            {
                                id,
                                (from f in _torrentEngine.Managers[id].Torrent.Files
                                 select new object[]
                                     {
                                         f.Path,
                                         f.Length,
                                         f.BytesDownloaded,
                                         f.Priority
                                     })
                            }
                    };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
