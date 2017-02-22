﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Jobs;
using Foundatio.Jobs.Commands;
using Foundatio.Logging;
using Foundatio.Logging.NLog;
using Foundatio.ServiceProviders;
using Microsoft.Extensions.CommandLineUtils;

namespace Foundatio.CronJob {
    public class Program {
        public static int Main(string[] args) {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog();
            var logger = loggerFactory.CreateLogger<Program>();

            var getServiceProvider = new Lazy<IServiceProvider>(() => ServiceProvider.FindAndGetServiceProvider(typeof(Sample1Job), loggerFactory));
            return JobCommands.Run(args, getServiceProvider, loggerFactory);
        }
    }

    [Job(Description = "Sample 1 job", Interval = "5s")]
    public class Sample1Job : IJob {
        private readonly ILogger _logger;

        public Sample1Job(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.CreateLogger<Sample1Job>();
        }

        public Task<JobResult> RunAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _logger.Info($"Sample1Job Run {Thread.CurrentThread.ManagedThreadId}");
            return Task.FromResult(JobResult.Success);
        }
    }

    [Job(Description = "Sample 2 job")]
    public class Sample2Job : IJob {
        private readonly ILogger _logger;

        public Sample2Job(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.CreateLogger<Sample2Job>();
        }

        public string CustomArg { get; set; }

        public Task<JobResult> RunAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _logger.Info($"Sample2Job Run CustomArg={CustomArg} {Thread.CurrentThread.ManagedThreadId}");
            return Task.FromResult(JobResult.Success);
        }

        public static void Configure(JobCommandContext context) {
            var app = context.Application;
            var argOption = app.Option("-c|--custom-arg", "This is a custom job argument.", CommandOptionType.SingleValue);

            app.OnExecute(() => {
                var job = context.ServiceProvider.Value.GetService(typeof(Sample2Job)) as Sample2Job;
                job.CustomArg = argOption.Value();
                return new JobRunner(job, context.LoggerFactory, runContinuous: true, interval: TimeSpan.FromSeconds(1)).RunInConsole();
            });
        }
    }

    [Job(Description = "Excluded job", IsContinuous = false)]
    public class ExcludeMeJob : IJob {
        private readonly ILogger _logger;

        public ExcludeMeJob(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.CreateLogger<Sample2Job>();
        }

        public Task<JobResult> RunAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _logger.Info($"ExcludeMeJob Run {Thread.CurrentThread.ManagedThreadId}");
            return Task.FromResult(JobResult.Success);
        }
    }
}
