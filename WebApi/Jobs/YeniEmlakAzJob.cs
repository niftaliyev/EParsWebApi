using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.YeniEmlakAz;

namespace WebApi.Jobs
{
    public class YeniEmlakAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public YeniEmlakAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var recordService = provider.GetRequiredService<YeniEmlakAzParser>();

            await recordService.YeniEmlakAzPars();
        }
    }
}
