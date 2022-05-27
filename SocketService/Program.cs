using System;
using System.Threading.Tasks;
using System.Threading;

namespace SocketService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client mode");
            SocketClient client = new SocketClient();
            var server = client.SearchServer();
            Console.WriteLine("server found:" + server.Name);
            Console.ReadLine();
        }
    }
}
