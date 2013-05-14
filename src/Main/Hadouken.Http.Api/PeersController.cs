using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Hadouken.BitTorrent;

namespace Hadouken.Http.Api
{
    public class PeersController : ApiController
    {
        private readonly IBitTorrentEngine _torrentEngine;

        public PeersController(IBitTorrentEngine torrentEngine)
        {
            _torrentEngine = torrentEngine;
        }

        public object Get(string id)
        {
            if (_torrentEngine.Managers.ContainsKey(id))
            {
                return new
                    {
                        peers = new object[]
                            {
                                id,
                                (from peer in _torrentEngine.Managers[id].Peers
                                 select new object[]
                                     {
                                         "00", // country
                                         peer.EndPoint.Address.ToString(),
                                         peer.ReverseDns,
                                         0, // utp
                                         peer.EndPoint.Port,
                                         peer.ClientSoftware,
                                         "", // flags
                                         peer.Progress,
                                         peer.DownloadSpeed,
                                         peer.UploadSpeed,
                                         -1, // requests in
                                         -1, // requests out
                                         -1, // waited
                                         peer.UploadedBytes,
                                         peer.DownloadedBytes,
                                         peer.HashFails,
                                         -1, // peer dl
                                         -1, // max up
                                         -1, // max down
                                         -1, // queued
                                         -1, // inactive
                                         -1 // relevance
                                     })
                            }
                    };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
