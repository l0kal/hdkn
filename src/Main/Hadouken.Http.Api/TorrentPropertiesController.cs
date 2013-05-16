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

            return new
                {
                    props = (from tm in _torrentEngine.Managers.Values
                             where id.Contains(tm.InfoHash)
                             select new
                                 {
                                     hash = tm.InfoHash,
                                     trackers = "",
                                     ulrate = tm.Settings.MaxUploadSpeed,
                                     dlrate = tm.Settings.MaxDownloadSpeed,
                                     superseed = tm.Settings.InitialSeedingEnabled,
                                     dht = tm.Settings.UseDht,
                                     pex = tm.Settings.EnablePeerExchange,
                                     seed_override = false,
                                     seed_ratio = 0,
                                     seed_time = 0,
                                     ulslots = tm.Settings.UploadSlots,
                                     seed_num = 0
                                 })
                };
        }

        public HttpResponseMessage Put([FromBody] PutTorrentPropertiesDto dto, [FromUri] string[] id = null)
        {
            if (id == null || id.Length == 0)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            foreach (var infoHash in id)
            {
                if (!_torrentEngine.Managers.ContainsKey(infoHash))
                    continue;

                var tm = _torrentEngine.Managers[infoHash];
                tm.Label = dto.Label;
            }

            using (var response = Request.CreateResponse(HttpStatusCode.NoContent))
            {
                return response;
            }
        }
    }
}
