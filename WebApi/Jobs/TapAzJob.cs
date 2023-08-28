using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading.Tasks;
using WebApi.Services.TapAz;

namespace WebApi.Jobs
{
    [DisallowConcurrentExecution]
    public class TapAzJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public TapAzJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var tapAzService = provider.GetRequiredService<TapAzParser>();

            await tapAzService.TapAzPars();
        }
    }
}
