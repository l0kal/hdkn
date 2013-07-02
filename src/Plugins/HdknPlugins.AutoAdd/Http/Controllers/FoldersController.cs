using Hadouken.Data;
using HdknPlugins.AutoAdd.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace HdknPlugins.AutoAdd.Http.Controllers
{
    public class FoldersController : ApiController
    {
        private readonly IDataRepository _dataRepository;

        public FoldersController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public IEnumerable<WatchedFolder> Get()
        {
            return _dataRepository.List<WatchedFolder>();
        } 
    }
}
