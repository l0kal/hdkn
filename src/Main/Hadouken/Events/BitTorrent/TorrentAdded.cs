using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public class TorrentAdded
    {
        public string InfoHash { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }
}
