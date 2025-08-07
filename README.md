# gRPC Demo Solution

## 📁 Struttura del progetto

```
GrpcDemo.sln
├── ServerApp/
│   ├── Program.cs
│   └── GreeterService.cs
├── ClientApp/
│   ├── Program.cs
│   ├── UnaryClient.cs
│   ├── ServerStreamClient.cs
│   ├── ClientStreamClient.cs
│   └── BiDiStreamClient.cs
└── Shared/
    └── Protos/
        └── greet.proto
```

## 📜 Spiegazione del .proto

Definisce 4 metodi:
- Unary → `SayHello`
- Server streaming → `StreamGreetings`
- Client streaming → `SendGreetings`
- Bidirectional → `Chat`

## 🔄 Rigenerazione file dal .proto

I file saranno generati automaticamente durante il build grazie al tag `<Protobuf>` nei `.csproj`.

## 🧪 Chiamate da terminale (solo unary):
```bash
grpcurl -plaintext -d '{ "name": "Mario" }' localhost:5000 greet.Greeter/SayHello
```

Gli altri richiedono un client gRPC.