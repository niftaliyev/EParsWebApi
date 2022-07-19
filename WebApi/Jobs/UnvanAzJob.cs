using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.UnvanAz;

namespace WebApi.Jobs
{
    public class UnvanAzJob :IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public UnvanAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var unvanAzService = provider.GetRequiredService<UnvanAzParser>();

            await unvanAzService.UnvanAzPars();
        }
    }
}
