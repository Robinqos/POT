using System.Net;
using System.Net.Sockets;

namespace KST.POT.AsyncExamples.U05AsyncTcpServer;

public static class AsyncTcpServer
{
    public static async Task Serve()
    {
        var listener = new TcpListener(IPAddress.Any, 7050);

        Console.WriteLine("Nacuvam na porte 7050, hovor");
        listener.Start();

        for (;;)
        {
            var socket = await listener.AcceptSocketAsync();
            Console.WriteLine($"Doslo spojenie z portu {socket.GetIPAddress()}");
            await ServeOne(socket);
        }
    }

    private static async Task ServeOne(Socket socket)
    {
        var buffer = new byte[1024];

        for (;;)
        {
            var count = await socket.ReceiveAsync(buffer, SocketFlags.None);
            if (count == 0)
                break;
            Console.WriteLine($"Dosli data {buffer.AsSpan()[..count].AsString()}");
            await socket.SendAsync(buffer[..count], SocketFlags.None);
        }

        socket.Close();

        Console.WriteLine("Disconnect");
    }
}
