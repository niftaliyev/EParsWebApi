using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Jobs
{
    public class UnvanAzScheduleService : JobScheduleService<UnvanAzJob>
    {
        public UnvanAzScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        protected override TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder)
        {
            return triggerBuilder.WithSimpleSchedule(x => x.WithIntervalInMinutes(15).RepeatForever());
        }
    }
}
