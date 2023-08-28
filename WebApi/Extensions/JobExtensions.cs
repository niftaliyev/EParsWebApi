using Microsoft.Extensions.DependencyInjection;
using WebApi.Jobs;
using WebApi.Jobs.ArendaAz;

namespace WebApi.Extensions
{
    public static class JobExtensions
    {
        public static IServiceCollection AddJobs(this IServiceCollection services)
        {
            services.AddTransient<ArendaAzJob>();
            services.AddHostedService<ArendaAzScheduleService>();
            services.AddTransient<TapAzJob>();
            services.AddHostedService<TapAzScheduleService>();
            services.AddTransient<EmlakAzJob>();
            services.AddHostedService<EmlakAzScheduleService>();
            services.AddTransient<YeniEmlakAzJob>();
            services.AddHostedService<YeniEmlakAzScheduleService>();
            services.AddTransient<BinaAzJob>();
            services.AddHostedService<BinaAzScheduleService>();

            services.AddTransient<LalalafoAzJob>();
            services.AddHostedService<LalafoAzScheduleService>();
            //services.AddTransient<UcuzTapAzJob>();
            //services.AddHostedService<UcuzTapAzScheduleService>();

            services.AddTransient<UnvanAzJob>();
            services.AddHostedService<UnvanAzScheduleService>();

            services.AddTransient<VipEmlakAzJob>();
            services.AddHostedService<VipEmlakAzScheduleService>();

            return services;
        }
    }
}
