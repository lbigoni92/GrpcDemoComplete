using System;
using System.Threading.Tasks;
using GrpcDemo;
using Grpc.Net.Client;

public static class UnaryClient
{
    public static async Task Run(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "Mario" });
        Console.WriteLine(reply.Message);
    }
}