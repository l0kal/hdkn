using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Hadouken.Configuration;
using Hadouken.Common.IO;

namespace Hadouken.Http.Api
{
    public class DirectoriesController : ApiController
    {
        private readonly IKeyValueStore _keyValueStore;
        private readonly IFileSystem _fileSystem;

        public DirectoriesController(IKeyValueStore keyValueStore, IFileSystem fileSystem)
        {
            _keyValueStore = keyValueStore;
            _fileSystem = fileSystem;
        }

        public object Get()
        {
            var favorites = _keyValueStore.Get("paths.favorites", new string[] {});
            var defaultSavePath = _keyValueStore.Get<string>("paths.defaultSavePath");

            var dirs = (from dir in favorites
                        select new
                            {
                                path = dir,
                                available = (_fileSystem.RemainingDiskSpace(dir)/1024/1024)
                            }).ToList();

            var result = new List<object>();
            result.Add(new
                {
                    path = "Default download dir",
                    available = "" + _fileSystem.RemainingDiskSpace(defaultSavePath)/1024/1024
                });
            result.AddRange(dirs);

            return new
                {
                    directories = result.ToArray()
                };
        }
    }
}
