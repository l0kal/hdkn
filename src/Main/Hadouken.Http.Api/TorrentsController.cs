using System.IO;
using System.Net;
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
        private readonly IBitTorrentEngine _torrentEngine;

        private static readonly Dictionary<string, Action<ITorrentManager>> Actions =
            new Dictionary<string, Action<ITorrentManager>>(); 

        public TorrentsController(IBitTorrentEngine torrentEngine)
        {
            _torrentEngine = torrentEngine;

            Actions["start"] = t => t.Start();
            Actions["stop"] = t => t.Stop();
        }

        public object Get()
        {
            return new
                {
                    label = (from l in _torrentEngine.Managers.Values.GroupBy(m => m.Label)
                             where !String.IsNullOrEmpty(l.Key)
                             select new object[]
                                 {
                                     l.Key,
                                     l.Count()
                                 }),
                    torrents = (from m in _torrentEngine.Managers.Values
                                select new object[]
                                    {
                                        m.InfoHash,
                                        m.State,
                                        m.Torrent.Name,
                                        m.Torrent.Size,
                                        (int) m.Progress*10,
                                        m.DownloadedBytes,
                                        m.UploadedBytes,
                                        (m.DownloadedBytes == 0 ? 0 : ((m.UploadedBytes/m.DownloadedBytes)*10)),
                                        m.UploadSpeed,
                                        m.DownloadSpeed,
                                        m.ETA.TotalSeconds,
                                        m.Label,
                                        m.Peers.Count(p => !p.IsSeeder),
                                        m.Trackers.Sum(tr => tr.Incomplete),
                                        m.Peers.Count(p => p.IsSeeder),
                                        m.Trackers.Sum(tr => tr.Complete),
                                        -1, // availability
                                        -1, // queue position
                                        m.RemainingBytes,
                                        "", // download url
                                        "", // rss feed url
                                        (m.State == TorrentState.Error ? "Error: --" : ""),
                                        -1, // stream id
                                        m.StartTime.ToUnixTime(),
                                        (m.CompletedTime.HasValue ? m.CompletedTime.Value.ToUnixTime() : -1),
                                        "", // app update url
                                        (m.Torrent.Files.Length > 1
                                             ? m.SavePath
                                             : Path.Combine(m.SavePath, m.Torrent.Name)),
                                        m.Complete
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

        public HttpResponseMessage Put([FromBody] Dictionary<string, object> body, [FromUri] string[] id = null)
        {
            if (id == null || id.Length == 0)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            foreach (var infoHash in id)
            {
                if (!_torrentEngine.Managers.ContainsKey(infoHash))
                    continue;

                var tm = _torrentEngine.Managers[infoHash];

                foreach (var key in body.Keys)
                {
                    switch (key.ToLowerInvariant())
                    {
                        case "action":
                            if (Actions.ContainsKey(body[key].ToString()))
                                Actions[body[key].ToString()](tm);
                            break;

                    }
                }
            }

            using (var response = Request.CreateResponse(HttpStatusCode.NoContent))
            {
                return response;
            }
        }
    }
}
