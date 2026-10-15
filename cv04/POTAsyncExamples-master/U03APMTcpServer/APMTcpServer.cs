using System.Net;
using System.Net.Sockets;

namespace KST.POT.AsyncExamples.U03APMTcpServer;

public static class APMTcpServer
{
    public static void Serve()
    {
        var listener = new TcpListener(IPAddress.Any, 7050);

        Console.WriteLine("Nacuvam na porte 7050, hovor");
        listener.Start();

        listener.BeginAcceptSocket(Accepted, null);

        void Accepted(IAsyncResult ar)
        {
            var socket = listener.EndAcceptSocket(ar);

            Console.WriteLine($"Doslo spojenie z portu {socket.GetIPAddress()}");

            listener.BeginAcceptSocket(Accepted, null);

            ServeOne(socket);
        }
    }

    private static void ServeOne(Socket socket)
    {
        var buffer = new byte[1024];

        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Received, null);

        void Received(IAsyncResult ar)
        {
            var count = socket.EndReceive(ar);

            if (count == 0)
            {
                socket.Close();
                Console.WriteLine("Disconnect");
                return;
            }

            Console.WriteLine($"Dosli data {buffer.AsSpan()[..count].AsString()}");

            socket.BeginSend(buffer, 0, count, SocketFlags.None, Sent, null);
        }

        void Sent(IAsyncResult ar)
        {
            socket.EndSend(ar);

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Received, null);
        }
    }
}
