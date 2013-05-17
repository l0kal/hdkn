using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using HdknPlugins.AutoAdd.Http.Dto;
using Hadouken.Common.Data;
using HdknPlugins.AutoAdd.Data.Models;
using System.Net.Http;
using System.Globalization;

namespace HdknPlugins.AutoAdd.Http.Api
{
    public class FoldersController : ApiController
    {
        private readonly IDataRepository _repository;

        public FoldersController(IDataRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<FolderDto> Get()
        {
            return (from folder in _repository.List<Folder>()
                    select new FolderDto
                        {
                            Id = folder.Id,
                            AutoStart = folder.AutoStart,
                            ExcludeFilter = folder.ExcludeFilter,
                            IncludeFilter = folder.IncludeFilter,
                            Label = folder.Label,
                            Path = folder.Path
                        });
        }

        public HttpResponseMessage Post([FromBody] FolderDto dto)
        {
            var f = new Folder
                {
                    AutoStart = (dto.AutoStart ?? false),
                    ExcludeFilter = dto.ExcludeFilter,
                    IncludeFilter = dto.IncludeFilter,
                    Label = dto.Label,
                    Path = dto.Path
                };

            _repository.Save(f);

            using (var response = Request.CreateResponse(HttpStatusCode.Created))
            {
                response.Headers.Location = new Uri(Request.RequestUri, f.Id.ToString(CultureInfo.InvariantCulture));
                return response;
            }
        }

        public HttpResponseMessage Put([FromUri] int id, [FromBody] FolderDto dto)
        {
            var f = _repository.Single<Folder>(id);

            if (f == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            
            f.AutoStart = (dto.AutoStart ?? f.AutoStart);
            f.ExcludeFilter = (dto.ExcludeFilter ?? f.ExcludeFilter);
            f.IncludeFilter = (dto.IncludeFilter ?? f.IncludeFilter);
            f.Label = (dto.Label ?? f.Label);
            f.Path = (dto.Path ?? f.Path);

            _repository.Update(f);

            using (var response = Request.CreateResponse(HttpStatusCode.NoContent))
            {
                return response;
            }
        }

        public HttpResponseMessage Delete(int id)
        {
            var f = _repository.Single<Folder>(id);

            if (f == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            _repository.Delete(f);

            using (var response = Request.CreateResponse(HttpStatusCode.NoContent))
            {
                return response;
            }
        }
    }
}
