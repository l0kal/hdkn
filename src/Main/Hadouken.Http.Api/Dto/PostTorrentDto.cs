using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Http.Api.Dto
{
    public class PostTorrentDto
    {
        public bool AutoStart { get; set; }
        public string Data { get; set; }
        public string Label { get; set; }
    }
}
