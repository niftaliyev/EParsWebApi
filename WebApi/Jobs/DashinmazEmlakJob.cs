using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading.Tasks;
using WebApi.Services.DashinmazEmlak;

namespace WebApi.Jobs
{
    public class DashinmazEmlakJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public DashinmazEmlakJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var dashinmazEmlak = provider.GetRequiredService<DashinmazEmlakParser>();

            await dashinmazEmlak.DashinmazEmlakPars();
        }
    }
}
