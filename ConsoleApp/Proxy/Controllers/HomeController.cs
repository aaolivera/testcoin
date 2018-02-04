using Proxy.Models;
using System.Net;
using System.Web.Mvc;

namespace Proxy.Controllers
{
    public class HomeController : Controller
    {
        public HttpWebResponseResult Index(string url = null)
        {
            return ExternalGet(url);
        }

        private HttpWebResponseResult ExternalGet(string url)
        {
            var getRequest = (HttpWebRequest)WebRequest.Create(url);
            var getResponse = (HttpWebResponse)getRequest.GetResponse();

            return new HttpWebResponseResult(getResponse);
        }
    }
}