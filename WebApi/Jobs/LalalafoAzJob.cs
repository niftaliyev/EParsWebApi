using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.LalafoAz;

namespace WebApi.Jobs
{
    public class LalalafoAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public LalalafoAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var lalafoAzService = provider.GetRequiredService<LalafoAzParser>();

            await lalafoAzService.LalafoAzPars();
        }
    }
}
