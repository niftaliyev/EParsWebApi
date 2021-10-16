using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Jobs;

namespace WebApi.Extensions
{
    public static class JobExtensions
    {
        public static IServiceCollection AddJobs(this IServiceCollection services)
        {
            services.AddTransient<TapAzJob>();
            services.AddHostedService<TapAzScheduleService>();
            services.AddTransient<EmlakAzJob>();
            services.AddHostedService<EmlakAzScheduleService>();
            services.AddTransient<YeniEmlakAzJob>();
            services.AddHostedService<YeniEmlakAzScheduleService>();
            return services;
        }
    }
}
