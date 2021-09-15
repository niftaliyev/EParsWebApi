using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Proxy
{
    public class ProxysHttpClient
    {
        private readonly HttpClient _client;
        public ProxysHttpClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> PostResponse(string url, FormUrlEncodedContent form)
        {
            var response = await _client.PostAsync(url, form);

            return response;
        }
    }
}