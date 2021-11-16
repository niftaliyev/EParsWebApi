using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.EmlakAz;

namespace WebApi.Jobs
{
    public class EmlakAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public EmlakAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var emlakAzService = provider.GetRequiredService<EmlakAzParser>();

            await emlakAzService.EmlakAzPars();
        }
    }
}
