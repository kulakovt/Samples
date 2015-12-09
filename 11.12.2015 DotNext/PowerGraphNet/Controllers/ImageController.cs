using System.Web.Http;

namespace PowerGraphNet.Controllers
{
    public sealed class ImageController : ApiController
    {
        [HttpGet]
        [Route("api/image")]
        public string GetImage()
        {
            return @" | |__| | (_) | |_| |\  |  __/>  <| |_ ";
        }
    }
}