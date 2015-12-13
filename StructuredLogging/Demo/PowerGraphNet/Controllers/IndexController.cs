using System;
using System.Threading.Tasks;
using System.Web.Http;
using PowerGraphNet.Utils;

namespace PowerGraphNet.Controllers
{
    public sealed class IndexController : ApiController
    {
        [HttpGet]
        [Route("index")]
        public async Task<string> GetIndex()
        {
            var requestId = Request.Headers.GetAppRequestId();

            var auth = Client.GetString(ClusterConfig.AuthAddress, "api/auth", requestId, WebServer.ServiceName);
            var stat = Client.GetString(ClusterConfig.StaticAddress, "api/static", requestId, WebServer.ServiceName);
            var ad = Client.GetString(ClusterConfig.AdAddress, "api/ad", requestId, WebServer.ServiceName);

            return String.Format(
                "{1}{0}{2}{0}{3}{0}{4}{0}",
                Environment.NewLine,
                "  _____        _   _   _           _   ",
                await auth,
                await stat,
                await ad);
        }
    }
}