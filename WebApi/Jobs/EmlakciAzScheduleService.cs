using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Jobs
{
    public class EmlakciAzScheduleService : JobScheduleService<EmlakciAzJob>
    {
        public EmlakciAzScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        protected override TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder)
        {
            return triggerBuilder.WithSimpleSchedule(x => x.WithIntervalInHours(5).RepeatForever());
        }
    }
}
