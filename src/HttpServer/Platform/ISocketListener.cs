// Copyright (c) Peter Nylander.  All rights reserved.

using System;
using System.Threading.Tasks;

namespace HttpServer.Platform
{
    public interface ISocketListener : IDisposable
    {
        event EventHandler<ISocket> ConnectionReceived;

        Task BindServiceNameAsync(string localServiceName);
    }
}
