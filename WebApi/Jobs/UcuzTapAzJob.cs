using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Services.UcuzTapAz;

namespace WebApi.Jobs
{
    public class UcuzTapAzJob :IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public UcuzTapAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var ucuztapAzService = provider.GetRequiredService<UcuzTapAzParser>();

            await ucuztapAzService.UcuzTapAzPars();
        }
    }
}
