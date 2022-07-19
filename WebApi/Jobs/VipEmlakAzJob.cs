using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Jobs
{
    public class VipEmlakAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public VipEmlakAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var vipEmlakAzService = provider.GetRequiredService<VipEmlakAzParser>();

            await vipEmlakAzService.VipEmlakAzPars();
        }
    }
}
