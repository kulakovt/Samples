using System.Web.Http;

namespace PowerGraphNet.Controllers
{
    public sealed class TextController : ApiController
    {
        [HttpGet]
        [Route("api/text")]
        public string GetText()
        {
            return @" | |  | |/ _ \| __| . ` |/ _ \ \/ / __|";
        }
    }
}