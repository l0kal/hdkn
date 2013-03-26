using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Http;
using System.Diagnostics;

namespace HdknPlugins.AutoUpdate.Http.Api
{
    [ApiAction("autoupdate-sourceupdate")]
    public class SourceUpdate : ApiAction
    {
        public override ActionResult Execute()
        {
            var url = BindModel<string>();

            var p = new Process();
            p.StartInfo.FileName = "msiexec";
            p.StartInfo.Arguments = String.Format("/i {0} /quiet /l*v {1}", url, "%PROGRAMDATA%\\Hadouken\\AutoUpdater\\" + Guid.NewGuid() + ".txt");
            p.StartInfo.Verb = "runas";

            var started = p.Start();

            return Json(started);
        }
    }
}
