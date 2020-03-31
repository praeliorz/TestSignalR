'use strict';

root.createSignalRConnection = function ()
{
    let url = root.buildUrl('chatHub');
    return new signalR.HubConnectionBuilder().withUrl(url).configureLogging(signalR.LogLevel.Information).build();
};

root.getConnectionId = function ()
{
    const url = root.app.connection.connection.transport.url || root.app.connection.connection.transport.webSocket.url;
    const r = /.*\=(.*)/;
    return r.exec(url)[1];
};