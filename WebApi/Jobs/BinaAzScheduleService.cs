using Quartz;
using System;


namespace WebApi.Jobs
{
    public class BinaAzScheduleService : JobScheduleService<BinaAzJob>
    {
        public BinaAzScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        protected override TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder)
        {
            return triggerBuilder.WithSimpleSchedule(x => x.WithIntervalInMinutes(2).RepeatForever());
        }
    }
}
