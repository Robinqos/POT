using System.Net;
using System.Net.Sockets;

namespace KST.POT.AsyncExamples.U04TAPTcpServer;

public static class TAPTcpServer
{
    public static Task Serve()
    {
        var listener = new TcpListener(IPAddress.Any, 7050);

        Console.WriteLine("Nacuvam na porte 7050, hovor");
        listener.Start();
        Task<Socket> acceptTask = listener.AcceptSocketAsync();
        return acceptTask.ContinueWith(Accepted).Unwrap();

        Task Accepted(Task<Socket> task)
        {
            var socket = task.Result;

            Console.WriteLine($"Doslo spojenie z portu {socket.GetIPAddress()}");

            var acceptTask = listener.AcceptSocketAsync();

            return Task.WhenAll(
                acceptTask.ContinueWith(Accepted).Unwrap(), 
                ServeOne(socket)
            );
        }
    }

    private static Task ServeOne(Socket socket)
    {
        var buffer = new byte[1024];

        var receiveTask = socket.ReceiveAsync(buffer, SocketFlags.None);
        return receiveTask.ContinueWith(Received).Unwrap();

        Task Received(Task<int> task)
        {
            var count = task.Result;

            if (count == 0)
            {
                socket.Close();
                Console.WriteLine("Disconnect");
                return Task.CompletedTask;
            }

            Console.WriteLine($"Dosli data {buffer.AsSpan()[..count].AsString()}");

            var sendTask = socket.SendAsync(buffer[..count], SocketFlags.None);
            return sendTask.ContinueWith(Sent).Unwrap();
        }

        Task Sent(Task<int> task)
        {
            var receiveTask = socket.ReceiveAsync(buffer, SocketFlags.None);
            return receiveTask.ContinueWith(Received).Unwrap();
        }
    }
}
