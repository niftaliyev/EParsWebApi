using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services;
using WebApi.Services.VipEmlakAz;

namespace WebApi.Jobs
{
    [DisallowConcurrentExecution]
    public class VipEmlakAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public VipEmlakAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var vipEmlakAzService = provider.GetRequiredService<VipEmlakAzParser>();

                await vipEmlakAzService.ParseSite();
            }
            catch (Exception e)
            {
                TelegramBotService.Sender($"cannot enter parser -->  vipemlak.az  -- {e.Message}");

            }

        }
    }
}
