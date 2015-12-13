using System.Web.Http;

namespace PowerGraphNet.Controllers
{
    public sealed class AuthController : ApiController
    {
        [HttpGet]
        [Route("api/auth")]
        public string GetAuth()
        {
            return @" |  __ \      | | | \ | |         | |  ";
        }
    }
}