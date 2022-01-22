using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Jobs
{
    public class LalafoAzScheduleService : JobScheduleService<LalalafoAzJob>
    {
        public LalafoAzScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        protected override TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder)
        {
            return triggerBuilder.WithSimpleSchedule(x => x.WithIntervalInHours(2).RepeatForever());
        }
    }
}
