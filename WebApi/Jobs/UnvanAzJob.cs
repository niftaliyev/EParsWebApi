using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;
using WebApi.Services.UnvanAz;

namespace WebApi.Jobs
{
    [DisallowConcurrentExecution]
    public class UnvanAzJob :IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public UnvanAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var unvanAzService = provider.GetRequiredService<UnvanAzParser>();
                await unvanAzService.UnvanAzPars();
            }
            catch (Exception e)
            {

                TelegramBotService.Sender($"cannot enter parser --> unvan.az -- {e.Message}");
            }
           
        }
    }
}
