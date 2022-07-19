using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.EvinAz;

namespace WebApi.Jobs
{
    public class EvinAzJob :IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public EvinAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var evinAzService = provider.GetRequiredService<EvinAzParser>();

            await evinAzService.EvinAzPars();
        }
    }
}
