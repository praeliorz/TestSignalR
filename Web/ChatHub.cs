using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestSignalR.Web
{
    public interface IChatHub
    {
        Task Connected();
        Task TagChangesMessage(IEnumerable<string> message);
    }

    public class ChatHub : Hub
    {
        public const string GroupName = "default";

        public static ConcurrentDictionary<string, byte> Subscribers { get; } = new ConcurrentDictionary<string, byte>();

        private readonly ILogger logger;


        public ChatHub(ILogger<ChatHub> logger) : base()
        {
            this.logger = logger;
        }

        public async Task ConnectAsync(string connectionKey)
        {
            logger?.LogInformation("ConnectAsync. ConnectionId: {0}. Time: {1}.", Context.ConnectionId, DateTime.Now.ToString("HH:mm:ss.ffff"));
            await Clients.Caller.SendAsync("Connected");
        }

        public override async Task OnConnectedAsync()
        {
            logger?.LogInformation("OnConnectedAsync. ConnectionId: {0}. Time: {1}.", Context.ConnectionId, DateTime.Now.ToString("HH:mm:ss.ffff"));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            logger?.LogInformation("OnDisconnectedAsync. ConnectionId: {0}. Time: {1}.", Context.ConnectionId, DateTime.Now.ToString("HH:mm:ss.ffff"));
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToGroupTagChanges(string connectionKey, string tagGroups)
        {
            string groupName = ChatHub.GroupName;
            Subscribers.TryAdd(Context.ConnectionId, 0);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task UnsubscribeFromGroupTagChanges(string connectionKey, string tagGroups)
        {
            var groupName = ChatHub.GroupName;
            Subscribers.TryRemove(Context.ConnectionId, out byte _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}