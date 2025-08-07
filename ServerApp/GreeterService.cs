using Grpc.Core;
using GrpcDemo;
using System.Threading.Tasks;

public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply { Message = $"Ciao {request.Name}" });
    }

    public override async Task StreamGreetings(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        for (int i = 0; i < 5; i++)
        {
            await responseStream.WriteAsync(new HelloReply { Message = $"Saluto {i+1} per {request.Name}" });
            await Task.Delay(500);
        }
    }

    public override async Task<HelloReply> SendGreetings(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        int count = 0;
        await foreach (var message in requestStream.ReadAllAsync())
        {
            count++;
        }
        return new HelloReply { Message = $"Ricevuti {count} messaggi" };
    }

    public override async Task Chat(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        await foreach (var req in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new HelloReply { Message = $"Echo: {req.Name}" });
        }
    }
}