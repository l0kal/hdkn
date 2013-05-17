using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HdknPlugins.AutoAdd.Http.Dto
{
    public class FolderDto
    {
        public int? Id { get; set; }
        public string Path { get; set; }
        public string Label { get; set; }
        public string ExcludeFilter { get; set; }
        public string IncludeFilter { get; set; }
        public bool? AutoStart { get; set; }
    }
}
