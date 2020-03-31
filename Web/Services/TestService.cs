using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TestSignalR.Web.Services
{
    public class TestService : IHostedService
    {
        private readonly string clientId = Guid.NewGuid().ToString();
        private readonly System.Timers.Timer sampleDataTimer = new System.Timers.Timer();
        private readonly IHubContext<ChatHub> hubContext;
        private readonly ILogger logger;
        private readonly BufferBlock<object> messageQueue = new BufferBlock<object>();
        private readonly CancellationTokenSource stopping = new CancellationTokenSource();

        public TestService(IHubContext<ChatHub> hubContext, ILogger<TestService> logger)
        {
            this.hubContext = hubContext;
            this.logger = logger;

            // Setup message queues.
            _ = Task.Run(() => ProcessMessageQueue(messageQueue));

            // Setup timer for sample data.
            sampleDataTimer.AutoReset = false;
            sampleDataTimer.Elapsed += (s, e) =>
            {
                try
                {
                    var @event = new List<string>();
                    for (var i = 0; i < 500; i++)
                    {
                        @event.Add($"test{i}");
                    }
                    messageQueue.Post(@event);
                }
                finally
                {
                    sampleDataTimer.Start();
                }
            };
            sampleDataTimer.Interval = 10;
            sampleDataTimer.Start();
        }

        private async Task ProcessMessageQueue(BufferBlock<object> messageQueue)
        {
            try
            {
                while (!stopping.IsCancellationRequested && await messageQueue.OutputAvailableAsync(stopping.Token))
                {
                    var request = await messageQueue.ReceiveAsync(stopping.Token);
                    if (request != null)
                    {
                        logger?.LogTrace("Start ProcessMessageQueue. Time: {0}.", DateTime.Now.ToString("HH:mm:ss.ffff"));

                        var stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();

                        await ProcessRequest(request);

                        stopwatch.Stop();
                        var responseTime = stopwatch.ElapsedMilliseconds;

                        logger?.LogTrace("End ProcessMessageQueue. ResponseTime: {0}. Time: {1}.", responseTime, DateTime.Now.ToString("HH:mm:ss.ffff"));
                        if (responseTime > 1000)
                        {
                            logger?.LogWarning("ProcessMessageQueue delay. ResponseTime: {0}. Time: {1}.", responseTime, DateTime.Now.ToString("HH:mm:ss.ffff"));
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore.
            }
            catch (Exception exc)
            {
                logger?.LogError(exc, "Error processing message queue. Time: {0}.", DateTime.Now.ToString("HH:mm:ss.ffff"));
            }
        }

        private async Task ProcessRequest(object message)
        {
            try
            {
                logger?.LogTrace("Start ProcessRequest. Class: {0}. Time: {1}.", message?.GetType().Name, DateTime.Now.ToString("HH:mm:ss.ffff"));

                switch (message)
                {
                    case List<string> request:
                        var groupName = ChatHub.GroupName;
                        logger?.LogTrace("Publish tag changes. ChatHub Group: {0}. Tag Count: {1}.", groupName, request.Count);
                        var chatHub = hubContext.Clients.Group(groupName);
                        if (chatHub != null)
                        {
                            foreach (var batch in request.Batch(100))
                            {
                                logger?.LogTrace("Start TagChangesMessage. ChatHub Group: {0}. Time: {1}.", groupName, DateTime.Now.ToString("HH:mm:ss.ffff"));
                                await chatHub.SendAsync("TagChangesMessage", batch);
                                logger?.LogTrace("End TagChangesMessage. ChatHub Group: {0}. Time: {1}.", groupName, DateTime.Now.ToString("HH:mm:ss.ffff"));
                            }
                        }
                        break;
                }

                logger?.LogTrace("End ProcessRequest. Class: {0}. Time: {1}.", message?.GetType().Name, DateTime.Now.ToString("HH:mm:ss.ffff"));
            }
            catch (Exception exc)
            {
                logger?.LogError(exc, "Error in ProcessRequest. Class: {0}. Time: {1}.", message?.GetType().Name, DateTime.Now.ToString("HH:mm:ss.ffff"));
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger?.LogInformation($"StartAsync {nameof(TestService)}.");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger?.LogInformation($"StopAsync {nameof(TestService)}.");
            if (!cancellationToken.IsCancellationRequested)
            {
                stopping.Cancel();
                messageQueue.Complete();
                await Task.WhenAll(messageQueue.Completion);
            }
        }
    }
}