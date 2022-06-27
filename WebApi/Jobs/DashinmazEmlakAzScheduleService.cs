using Quartz;
using System;

namespace WebApi.Jobs
{
    public class DashinmazEmlakAzScheduleService : JobScheduleService<DashinmazEmlakJob>
    {
        public DashinmazEmlakAzScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        protected override TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder)
        {
            return triggerBuilder.WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever());
        }
    }
}
