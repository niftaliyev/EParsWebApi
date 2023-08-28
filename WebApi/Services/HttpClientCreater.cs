using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using WebApi.Options;

namespace WebApi.Services
{
    public class HttpClientCreater
    {
        private readonly string Username;
        private readonly string Password;
        private readonly ProxyServerOptions proxyServerOptions;

        public HttpClientCreater(IOptions<ProxyServerOptions> options)
        {
            proxyServerOptions = options.Value;
            Username = proxyServerOptions.Username;
            Password = proxyServerOptions.Password;
        }
        //public HttpClient Create(string ipadress)
        //{
        //    try
        //    {
        //        return new HttpClient(new HttpClientHandler
        //        {
        //            Proxy = new WebProxy
        //            {
        //                Address = new Uri($"http://{ipadress}"),
        //                BypassProxyOnLocal = false,
        //                UseDefaultCredentials = false,
        //                Credentials = new NetworkCredential(
        //                              userName: Username,
        //                              password: Password)
        //            }
        //        }, disposeHandler: true);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
          
        //}


        public HttpClient Create(string ipadress)
        {
            try
            {
                //var httpClient = new HttpClient(new HttpClientHandler {  Proxy = new WebProxy("p.webshare.io:80"),Credentials  = new NetworkCredential(
                //                      userName: "omjofiuv-rotate",
                //                      password: "ouwtpatu6rf3")
                //});

                return new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy
                    {
                        Address = new Uri($"http://{ipadress}"),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(
                                      userName: Username,
                                      password: Password)
                    }
                }, disposeHandler: true);

                //return httpClient;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
