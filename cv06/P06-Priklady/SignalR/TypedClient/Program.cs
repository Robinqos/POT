using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedClientInterfaces;

namespace TypedClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chathub")
                .Build();

            connection.On<string, string>(nameof(IChatClient.ReceiveMessage), (user, message) =>
            {
                Console.WriteLine($"{user}: {message}");
            });

            await connection.StartAsync();

            Console.Write("Name: ");
            var username = Console.ReadLine();
            Console.Write("Message: ");
            var message = Console.ReadLine();
            while (!message.Equals("end"))
            {
                await connection.SendAsync("SendMessage", username, message);
                message = Console.ReadLine();
            }
        }
    }
}