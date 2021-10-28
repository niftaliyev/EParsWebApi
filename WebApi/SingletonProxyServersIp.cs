using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi
{
    public class SingletonProxyServersIp
    {
        static private string[] proxies = null;

        static public string[] Instance
        {
            get
            {
                if (proxies == null)
                    proxies = File.ReadAllLines("proxies.txt");
                return proxies;
            }
        }

        public List<int> Data { get; set; }

        private SingletonProxyServersIp()
        {
            proxies = File.ReadAllLines("proxies.txt");

        }
    }
}
