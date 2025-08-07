using System;
using System.Threading.Tasks;
using GrpcDemo;
using Grpc.Net.Client;

public static class ServerStreamClient
{
    public static async Task Run(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        var call = client.StreamGreetings(new HelloRequest { Name = "Anna" });

        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine(response.Message);
        }
    }
}