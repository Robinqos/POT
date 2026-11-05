using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcExample;

namespace GrpcNetClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            // The port number must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            var response = client.SayHello(new HelloRequest { Name = "test" });
            Console.WriteLine(response.Message);


            var duplexStream = client.EchoStream(Metadata.Empty);
            _ = Task.Factory.StartNew(
                async () =>
                {
                    await foreach (var msg in duplexStream.ResponseStream.ReadAllAsync(cts.Token))
                    {
                        Console.WriteLine($"{msg.Time}: {msg.Message}");
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);


            while (true)
            {
                var message = Console.ReadLine();
                if (message == "Bye")
                {
                    cts.Cancel();
                    await duplexStream.RequestStream.CompleteAsync();
                    return;
                }

                await duplexStream.RequestStream.WriteAsync(new EchoRequest { Message = message });
            }

        }
    }
}
