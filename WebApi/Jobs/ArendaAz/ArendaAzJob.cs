using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;
using WebApi.Services.ArendaAz;

namespace WebApi.Jobs.ArendaAz
{
    [DisallowConcurrentExecution]
    public class ArendaAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ArendaAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var arendaAzService = provider.GetRequiredService<ArendaAzParser>();

                await arendaAzService.ArendaAzPars();
            }
            catch (Exception e)
            {

                TelegramBotService.Sender($"Cannot enter parser --> arenda.az -- {e.Message}");
            }
           
        }
    }
}
