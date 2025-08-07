using System;
using System.Threading.Tasks;
using GrpcDemo;
using Grpc.Net.Client;

public static class UnaryClient
{
    public static async Task Run(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        Console.WriteLine("Client invia: Mario");
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "Mario" });
        Console.WriteLine("Ricevo dal server: " + reply.Message);
    }
}