using System;
using System.Threading.Tasks;
using GrpcDemo;
using Grpc.Net.Client;

public static class ClientStreamClient
{
    public static async Task Run(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        using var call = client.SendGreetings();

        foreach (var name in new[] { "Uno", "Due", "Tre" })
        {
            await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
            await Task.Delay(300);
        }

        await call.RequestStream.CompleteAsync();
        var response = await call;
        Console.WriteLine(response.Message);
    }
}