// Copyright (c) Peter Nylander.  All rights reserved.

using HttpServer.Platform;
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace HttpServer.Universal.Platform
{
    internal class SocketListener : ISocketListener
    {
        private readonly StreamSocketListener socketListener;

        public SocketListener()
        {
            this.socketListener = new StreamSocketListener();
            this.socketListener.ConnectionReceived += this.SocketListener_ConnectionReceived;
        }

        public event EventHandler<ISocket> ConnectionReceived;

        public Task BindServiceNameAsync(string localServiceName)
        {
            return this.socketListener.BindServiceNameAsync(localServiceName).AsTask();
        }

        public void Dispose()
        {
            this.socketListener.Dispose();
        }

        private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamSocket streamSocket = args.Socket;

            var socket = new Socket(streamSocket);

            this.ConnectionReceived?.Invoke(this, socket);
        }
    }
}
