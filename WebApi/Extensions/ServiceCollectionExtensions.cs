using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Options;

namespace WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxyServerConfigurations(this IServiceCollection services, Action<ProxyServerOptions> options)
        {
            services.Configure<ProxyServerOptions>(options);
            return services;
        }
    }
}
