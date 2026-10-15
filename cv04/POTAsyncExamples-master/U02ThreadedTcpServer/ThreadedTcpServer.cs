using System.Net;
using System.Net.Sockets;

namespace KST.POT.AsyncExamples.U02ThreadedTcpServer;

public static class ThreadedTcpServer
{
    public static void Serve()
    {
        var listener = new TcpListener(IPAddress.Any, 7050);

        Console.WriteLine("Nacuvam na porte 7050, hovor");
        listener.Start();

        for (;;)
        {
            var socket = listener.AcceptSocket();
            Console.WriteLine($"Doslo spojenie z portu {socket.GetIPAddress()}");
            new Thread(() => ServeOne(socket)).Start();
            //ThreadPool.QueueUserWorkItem(x => ServeOne(socket), null);
        }
    }

    private static void ServeOne(Socket socket)
    {
        Span<byte> buffer = stackalloc byte[1024];

        for (;;)
        {
            var count = socket.Receive(buffer);
            if (count == 0)
                break;
            Console.WriteLine($"Dosli data {buffer[..count].AsString()}");
            socket.Send(buffer[..count]);
        }

        socket.Close();

        Console.WriteLine("Disconnect");
    }
}
