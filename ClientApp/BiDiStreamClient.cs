using System;
using System.Threading.Tasks;
using GrpcDemo;
using Grpc.Net.Client;
using Grpc.Core;

public static class BiDiStreamClient
{
    public static async Task Run(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        using var call = client.Chat();

        var readTask = Task.Run(async () =>
        {
            while (await call.ResponseStream.MoveNext())
            {
                var response = call.ResponseStream.Current;
                Console.WriteLine("Server: " + response.Message);
            }
        });

        foreach (var name in new[] { "A", "B", "C" })
        {
            Console.WriteLine("Client: " + name);
            await call.RequestStream.WriteAsync(new HelloRequest { Name = name });
            await Task.Delay(500);
        }

        await call.RequestStream.CompleteAsync();
        await readTask;
    }
}