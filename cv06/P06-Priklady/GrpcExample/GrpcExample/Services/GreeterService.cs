using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

namespace GrpcExample
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task EchoStream(IAsyncStreamReader<EchoRequest> requestStream, IServerStreamWriter<EchoResponse> responseStream, ServerCallContext context)
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                await responseStream.WriteAsync(new EchoResponse{Message = message.Message, Time = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())});
            }
        }


        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public override Task<WeatherReply> GetWeather(EmptyRequest request, ServerCallContext context)
        {
            var reply = new WeatherReply();
            var rng = new Random();

            reply.Forecasts.Add(Enumerable.Range(1, 10).Select(index => new WeatherForecast
            {
                
                DateTimeStamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime().AddDays(index)),
                TemperatureC = rng.Next(20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }));

            return Task.FromResult(reply);
        }
    }
}
