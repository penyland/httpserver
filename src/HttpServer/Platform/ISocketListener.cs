// Copyright (c) Peter Nylander.  All rights reserved.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpServer.Platform
{
    public interface ISocketListener
    {
        event EventHandler<TcpClient> ConnectionReceived;

        Task BindServiceNameAsync(string localServiceName);

        void Start();

        void Stop();
    }
}
