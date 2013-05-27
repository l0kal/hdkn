using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public class Torrent
    {
        public string InfoHash { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
    }
}
