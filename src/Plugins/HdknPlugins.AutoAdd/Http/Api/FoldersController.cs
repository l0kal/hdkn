using System.Web.Http;

namespace HdknPlugins.AutoAdd.Http.Api
{
    public class FoldersController : ApiController
    {
        public object Get()
        {
            return new
                {
                    folders = new object[] {}
                };
        }
    }
}
