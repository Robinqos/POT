using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ClientWebSocket socket = new ClientWebSocket();
            Uri uri = new Uri("ws://localhost:5000/ws");
            var cts = new CancellationTokenSource();
            await socket.ConnectAsync(uri, cts.Token);

            Console.WriteLine(socket.State);

            _ = Task.Factory.StartNew(
                async () =>
                {
                    var rcvBytes = new byte[128];
                    var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    while (true)
                    {
                        WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, cts.Token);
                        byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                        string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                        Console.WriteLine("Received: {0}", rcvMsg);
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            while (true)
            {
                var message = Console.ReadLine();
                if (message == "Bye")
                {
                    cts.Cancel();
                    return;
                }
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                var sendBuffer = new ArraySegment<byte>(sendBytes);
                await
                    socket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true,
                        cancellationToken: cts.Token);
            }



        }
    }
}
