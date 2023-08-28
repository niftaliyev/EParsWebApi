using Quartz;
using System;

namespace WebApi.Jobs
{
    public class LalafoAzScheduleService : JobScheduleService<LalalafoAzJob>
    {
        public LalafoAzScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        protected override TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder)
        {
            return triggerBuilder.WithSimpleSchedule(x => x.WithIntervalInMinutes(3).RepeatForever());
        }
    }
}
