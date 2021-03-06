﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hadouken.Framework.Rpc;
using Hadouken.Plugins.Torrents.BitTorrent;
using Hadouken.Plugins.Torrents.Dto;

namespace Hadouken.Plugins.Torrents.Rpc
{
    public class TorrentsServices : IJsonRpcService
    {
        private readonly IBitTorrentEngine _torrentEngine;

        public TorrentsServices(IBitTorrentEngine torrentEngine)
        {
            _torrentEngine = torrentEngine;
        }

        [JsonRpcMethod("torrents.start")]
        public bool Start(string infoHash)
        {
            var manager = _torrentEngine.Get(infoHash);

            if (manager == null)
                return false;

            manager.Start();

            return true;
        }

        [JsonRpcMethod("torrents.pause")]
        public bool Pause(string infoHash)
        {
            var manager = _torrentEngine.Get(infoHash);

            if (manager == null)
                return false;

            manager.Pause();

            return true;
        }

        [JsonRpcMethod("torrents.stop")]
        public bool Stop(string infoHash)
        {
            var manager = _torrentEngine.Get(infoHash);

            if (manager == null)
                return false;

            manager.Stop();

            return true;
        }

        [JsonRpcMethod("torrents.list")]
        public IEnumerable<TorrentOverview> List()
        {
            var managers = _torrentEngine.Managers;
            return managers.Select(t => new TorrentOverview(t.Manager));
        }

        [JsonRpcMethod("torrents.details")]
        public TorrentDetails Details(string id)
        {
            var manager = _torrentEngine.Get(id);

            if (manager == null)
                return null;

            return new TorrentDetails(manager.Manager);
        }

        [JsonRpcMethod("torrents.addFile")]
        public TorrentOverview AddFile(byte[] data, string savePath, string label)
        {
            var manager = _torrentEngine.Add(data, savePath, label);

            if (manager == null)
                return null;

            return new TorrentOverview(manager.Manager);
        }

        [JsonRpcMethod("torrents.addUrl")]
        public TorrentOverview AddUrl(string url, string savePath, string label)
        {
            using (var client = new HttpClient())
            {
                var data = client.GetByteArrayAsync(url).Result;
                var manager = _torrentEngine.Add(data, savePath, label);

                if (manager == null)
                    return null;

                return new TorrentOverview(manager.Manager);
            }
        }

        [JsonRpcMethod("torrents.addMagnetLink")]
        public TorrentOverview AddMagnetLink(string magnetLink, string savePath, string label)
        {
            var manager = _torrentEngine.AddMagnetLink(magnetLink, savePath, label);

            if (manager == null)
                return null;

            return new TorrentOverview(manager.Manager);
        }
    }
}
