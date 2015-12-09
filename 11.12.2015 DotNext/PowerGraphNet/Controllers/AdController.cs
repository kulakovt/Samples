using System;
using System.Threading;
using System.Web.Http;

namespace PowerGraphNet.Controllers
{
    public sealed class AdController : ApiController
    {
        [HttpGet]
        [Route("api/ad")]
        public string GetAd()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1.1));

            return @" |_____/ \___/ \__|_| \_|\___/_/\_\\__|";
        }
    }
}