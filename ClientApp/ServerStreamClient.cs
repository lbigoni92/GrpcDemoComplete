using System;
using System.Threading.Tasks;
using GrpcDemo;
using Grpc.Net.Client;
using Grpc.Core;

public static class ServerStreamClient
{
    public static async Task Run(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        var call = client.StreamGreetings(new HelloRequest { Name = "Anna" });

        while (await call.ResponseStream.MoveNext())
        {
            var response = call.ResponseStream.Current;
            Console.WriteLine("Ricevo dal server: "+response.Message);
        }

    }
}