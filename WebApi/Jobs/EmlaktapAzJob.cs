using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.EmlaktapAz;

namespace WebApi.Jobs
{
    public class EmlaktapAzJob: IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public EmlaktapAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var emlaktapAzService = provider.GetRequiredService<EmlaktapAzParser>();

            await emlaktapAzService.EmlaktapAzPars();
        }
    }
}
