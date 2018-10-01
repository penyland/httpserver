// Copyright (c) Peter Nylander.  All rights reserved.

using HttpServer.Platform;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpServer.NetCore.Platform
{
    internal class SocketListener : ISocketListener
    {
        private readonly TcpListener socketListener;

        public SocketListener()
        {
            this.socketListener = new TcpListener(new IPEndPoint(IPAddress.Any, 80));
            //this.socketListener.ConnectionReceived += this.SocketListener_ConnectionReceived;

            this.socketListener.AcceptSocketAsync();
        }

        public event EventHandler<ISocket> ConnectionReceived;

        public Task BindServiceNameAsync(string localServiceName)
        {
            return Task.FromResult<object>(null);

            //return this.socketListener.BindServiceNameAsync(localServiceName).AsTask();
        }

        public void Dispose()
        {
            //this.socketListener.Dispose();
        }

        //private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        //{
        //    StreamSocket streamSocket = args.Socket;

        //    var socket = new Socket(streamSocket);

        //    this.ConnectionReceived?.Invoke(this, socket);
        //}
    }
}
