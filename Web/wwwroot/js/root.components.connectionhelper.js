'use strict';

root.components.connectionHelper = function ()
{
    this.connectionKey = null;
    this.connections = [];
    this.reconnectBufferInMilliseconds = 5000;
    this.reconnectTimeout = null;
};

root.components.connectionHelper.prototype.connect = function ()
{
    // Create close callback.
    if (!root.app.connection.closedCallbacks.length)
        root.app.connection.onclose(this.reconnect.bind(this));

    // Create SignalR connection.
    let connectionState = new root.enums.signalRConnectionState(root.app.connection.connection.connectionState);
    if (connectionState.isConnected())
    {
        this.startup();
    }
    else
    {
        console.log('Starting SignalR.');
        root.app.connection.start()
            .then(this.startup.bind(this))
            .catch(this.reconnect.bind(this));
    }
};

root.components.connectionHelper.prototype.reconnect = function (message)
{
    root.app.connectionStatus.connecting(message);
    clearTimeout(this.reconnectTimeout);
    this.reconnectTimeout = setTimeout(this.connect.bind(this), this.reconnectBufferInMilliseconds);
};

root.components.connectionHelper.prototype.startup = function ()
{
    // Connect to OPC UA Server.
    if (this.connectionKey)
    {
        root.app.connection.invoke('ConnectAsync', this.connectionKey);
    }

    this.connections.forEach(this.subscribe, this);
};

root.components.connectionHelper.prototype.subscribe = function (connection)
{
    if (connection.connectionKey && connection.tagGroup)
    {
        root.app.connection.invoke(connection.subscribeMethod || 'SubscribeToGroupTagChanges', connection.connectionKey, connection.tagGroup)
            .then(root.app.connectionStatus.connected.bind(root.app.connectionStatus))
            .catch(this.reconnect.bind(this));
    }
};

root.components.connectionHelper.prototype.unsubscribe = function (connection)
{
    if (connection.connectionKey && connection.tagGroup)
    {
        root.app.connection.invoke(connection.unsubscribeMethod || 'UnsubscribeFromGroupTagChanges', connection.connectionKey, connection.tagGroup);
    }
};