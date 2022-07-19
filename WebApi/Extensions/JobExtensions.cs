using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Jobs;
using WebApi.Jobs.ArendaAz;

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
            services.AddTransient<BinaAzJob>();
            services.AddHostedService<BinaAzScheduleService>();
            services.AddTransient<EmlakciAzJob>();
            services.AddHostedService<EmlakciAzScheduleService>();
            services.AddTransient<LalalafoAzJob>();
            services.AddHostedService<LalafoAzScheduleService>();
            services.AddTransient<DashinmazEmlakJob>();
            services.AddHostedService<DashinmazEmlakAzScheduleService>();
            services.AddTransient<UcuzTapAzJob>();
            services.AddHostedService<UcuzTapAzScheduleService>();
            services.AddTransient<EmlaktapAzJob>();
            services.AddTransient<EmlaktapAzScheduleService>();
            services.AddTransient<UnvanAzJob>();
            services.AddTransient<UnvanAzScheduleService>();
            services.AddTransient<EvinAzJob>();
            services.AddTransient<EvinAzScheduleService>();
            services.AddTransient<VipEmlakAzJob>();
            services.AddTransient<VipEmlakAzScheduleService>();
            services.AddTransient<ArendaAzJob>();
            services.AddTransient<ArendaAzScheduleService>();
            return services;
        }
    }
}
