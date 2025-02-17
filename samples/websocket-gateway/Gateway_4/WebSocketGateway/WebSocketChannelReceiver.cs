﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Gateway;
using WebSocketSharp.Server;

#region WebSockectGateway-ChannelReceiver

public class WebSocketChannelReceiver :
    IChannelReceiver
{
    public void Start(string address, int maxConcurrency, Func<DataReceivedOnChannelEventArgs, CancellationToken, Task> dataReceivedOnChannel)
    {
        var uri = new Uri(address);
        server = new WebSocketServer(uri.GetLeftPart(UriPartial.Authority));

        server.AddWebSocketService(uri.AbsolutePath,
            initializer: () =>
            {
                return new WebSocketMessageBehavior(dataReceivedOnChannel);
            });
        server.Start();
    }

    public Task Stop(CancellationToken token)
    {
        server?.Stop();
        return Task.CompletedTask;
    }

    WebSocketServer server;
}

#endregion