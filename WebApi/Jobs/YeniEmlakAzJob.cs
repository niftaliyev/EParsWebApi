using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;
using WebApi.Services;
using WebApi.Services.YeniEmlakAz;

namespace WebApi.Jobs
{
    [DisallowConcurrentExecution]
    public class YeniEmlakAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public YeniEmlakAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var recordService = provider.GetRequiredService<YeniEmlakAzParser>();


                await recordService.ParseSite();


            }
            catch (Exception e)
            {

                TelegramBotService.Sender($"cannot enter parser --> yeniemlak.az -- {e.Message}");

            }
          
        }
    }
}
