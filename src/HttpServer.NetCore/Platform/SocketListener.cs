// Copyright (c) Peter Nylander.  All rights reserved.

using HttpServer.Platform;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpServer.NetCore.Platform
{
    public class SocketListener : ISocketListener
    {
        private readonly TcpListener socketListener;

        public SocketListener(int port)
        {
            this.socketListener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
        }

        public event EventHandler<TcpClient> ConnectionReceived;

        public async Task BindServiceNameAsync(string localServiceName)
        {
            TcpClient tcpClient = await this.socketListener.AcceptTcpClientAsync();
            this.ConnectionReceived?.Invoke(this, tcpClient);
        }

        public void Start()
        {
            this.socketListener.Start();
        }

        public void Stop()
        {
            this.socketListener.Stop();
        }
    }
}
