using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public class HttpClientCreater
    {
        public HttpClient Create(string ipadress)
        {
            return new HttpClient(new HttpClientHandler
            {
                Proxy = new WebProxy
                {
                    Address = new Uri($"http://{ipadress}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                userName: "UA_NA424420",
                password: "SnJwT6jujL")
                }
            }, disposeHandler: true);
        }
    }
}
