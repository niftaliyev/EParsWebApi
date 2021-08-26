using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Jobs
{
    //Класс для упрощения работы с графиком
    public abstract class JobScheduleService<TJob> : IHostedService
        where TJob : IJob
    {
        private const string DEFAULT_JOB_GROUP = "group1";
        private const string DEFAULT_TRIGGER_GROUP = "group1";

        private readonly IServiceProvider _serviceProvider;

        public JobScheduleService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler(cancellationToken);

            scheduler.JobFactory = new DefaultJobFactory(_serviceProvider);

            await scheduler.Start(cancellationToken);

            var jobDetail = ConfigureJob(JobBuilder
                .Create<TJob>()
                .WithIdentity(typeof(TJob).Name, DEFAULT_JOB_GROUP))
                .Build();

            var trigger = ConfigureTrigger(TriggerBuilder
                .Create()
                .WithIdentity($"{typeof(TJob).Name}-trigger", DEFAULT_TRIGGER_GROUP))
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger);
        }

        /// <summary>
        /// Метод для настройки триггера срабатывания Job-ы.
        /// </summary>
        /// <param name="triggerBuilder"> Builder триггера. </param>
        /// <returns> Builder триггера. </returns>
        protected abstract TriggerBuilder ConfigureTrigger(TriggerBuilder triggerBuilder);

        /// <summary>
        /// Метод для настройки Job-ы.
        /// </summary>
        /// <param name="jobBuilder"> Builder Job-ы. </param>
        /// <returns> Builder Job-ы. </returns>
        protected virtual JobBuilder ConfigureJob(JobBuilder jobBuilder) => jobBuilder;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
