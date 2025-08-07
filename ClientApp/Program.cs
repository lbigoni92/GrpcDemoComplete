using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;

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


        while (true)
        {
            Console.WriteLine("Scegli un tipo di comunicazione:");
            Console.WriteLine("1) Unary");
            Console.WriteLine("2) Server Streaming");
            Console.WriteLine("3) Client Streaming");
            Console.WriteLine("4) Bidirectional Streaming");
            Console.WriteLine("0) Esci");

            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await UnaryClient.Run(channel);
                    break;
                case "2":
                    await ServerStreamClient.Run(channel);
                    break;
                case "3":
                    await ClientStreamClient.Run(channel);
                    break;
                case "4":
                    await BiDiStreamClient.Run(channel);
                    break;
                case "0":
                    return;
            }
        }
    }
}