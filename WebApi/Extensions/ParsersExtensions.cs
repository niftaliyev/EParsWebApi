using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;
using WebApi.Services.EmlakAz;
using WebApi.Services.TapAz;

namespace WebApi.Extensions
{
    public static class ParsersExtensions
    {
        public static IServiceCollection AddParsers(this IServiceCollection services)
        {
            services.AddTransient<TapAzParser>();
            services.AddTransient<EmlakBaza>();
            services.AddTransient<TapAzImageUploader>();
            services.AddTransient<EmlakAzParser>();
            services.AddTransient<EmlakAzImageUploader>();
            return services;
        }
    }
}
