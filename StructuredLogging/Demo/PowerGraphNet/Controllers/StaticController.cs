using System;
using System.Threading.Tasks;
using System.Web.Http;
using PowerGraphNet.Utils;

namespace PowerGraphNet.Controllers
{
    public sealed class StaticController : ApiController
    {
        [HttpGet]
        [Route("api/static")]
        public async Task<string> GetStatic()
        {
            var requestId = Request.Headers.GetAppRequestId();

            var text = Client.GetString(ClusterConfig.TextAddress, "api/text", requestId, WebServer.ServiceName);
            var image = Client.GetString(ClusterConfig.ImageAddress, "api/image", requestId, WebServer.ServiceName);

            return String.Format(
                "{1}{0}{2}{0}{3}",
                Environment.NewLine,
                @" | |  | | ___ | |_|  \| | _____  _| |_ ",
                await text,
                await image);

        }
    }
}