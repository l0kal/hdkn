using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Http;
using Hadouken.Data;
using HdknPlugins.AutoAdd.Data.Models;
using Hadouken;

namespace HdknPlugins.AutoAdd.Http.Api
{
    [Component]
    [ApiAction("autoadd-getwatchedfolders")]
    public class GetWatchedFolders : ApiAction
    {
        private readonly IDataRepository _dataRepository;

        public GetWatchedFolders(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public override ActionResult Execute()
        {
            return Json(new {folders = _dataRepository.List<WatchedFolder>()});
        }
    }
}
