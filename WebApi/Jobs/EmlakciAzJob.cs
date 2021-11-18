using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.EmlakciAz;

namespace WebApi.Jobs
{
    public class EmlakciAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public EmlakciAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var emalkciAzService = provider.GetRequiredService<EmlakciAzParser>();

            await emalkciAzService.EmlakciAzPars();
        }
    }
}
