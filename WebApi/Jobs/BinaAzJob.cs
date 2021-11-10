using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.BinaAz;

namespace WebApi.Jobs
{
    public class BinaAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public BinaAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var binaAzService = provider.GetRequiredService<BinaAzParser>();

            await binaAzService.BinaAzPars();
        }
    }
}
