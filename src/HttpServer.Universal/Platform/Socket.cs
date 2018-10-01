// Copyright (c) Peter Nylander.  All rights reserved.

using HttpServer.Platform;
using Windows.Networking.Sockets;

namespace HttpServer.Universal.Platform
{
    public class Socket : ISocket
    {
        private readonly StreamSocket streamSocket;

        public Socket(StreamSocket streamSocket)
        {
            this.streamSocket = streamSocket;
        }
    }
}
