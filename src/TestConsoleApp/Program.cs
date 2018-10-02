using HttpServer.NetCore.Platform;
using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var httpServer = new HttpServer.HttpServer(64000, new HttpServer.NetCore.Platform.SocketListener(64000));
            httpServer.Start();

            while(Console.ReadKey().Key != ConsoleKey.Escape)
            {
            }

            httpServer.Stop();
            httpServer.Dispose();
        }
    }
}
