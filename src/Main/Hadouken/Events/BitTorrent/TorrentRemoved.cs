using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public class TorrentRemoved
    {
        public string InfoHash { get; set; }
    }
}
