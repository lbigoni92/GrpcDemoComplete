using Grpc.Net.Client;
using GrpcDemo;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var httpHandler = new HttpClientHandler//only for test
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var channel = GrpcChannel.ForAddress("https://localhost:63174", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        var client = new Greeter.GreeterClient(channel);


        while (true)
        {
            Console.WriteLine("Scegli un tipo di comunicazione:");
            Console.WriteLine("1) Unary");
            Console.WriteLine("2) Server Streaming");
            Console.WriteLine("3) Client Streaming");
            Console.WriteLine("4) Bidirectional Streaming");
            Console.WriteLine("0) Esci");
            Console.WriteLine("C#>");

            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await UnaryClient.Run(client);
                    break;
                case "2":
                    await ServerStreamClient.Run(client);
                    break;
                case "3":
                    await ClientStreamClient.Run(client);
                    break;
                case "4":
                    await BiDiStreamClient.Run(client);
                    break;
                case "0":
                    return;
            }
        }
    }
}