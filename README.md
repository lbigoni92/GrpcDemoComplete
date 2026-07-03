# gRPC Demo Solution

Soluzione dimostrativa di **gRPC in .NET 8** con un client aggiuntivo in Python.
Tutti i progetti condividono un unico contratto definito in `Shared/Protos/greet.proto`.

---

## Struttura del progetto

```
GrpcDemo.sln
├── ServerApp/                     Server gRPC ASP.NET Core
│   ├── Program.cs                 Configurazione Kestrel ed endpoint
│   └── GreeterService.cs          Implementazione dei 4 RPC
│
├── ClientApp/                     Client gRPC C# (.NET 8)
│   ├── Program.cs                 Menu di selezione e canale gRPC
│   ├── UnaryClient.cs
│   ├── ServerStreamClient.cs
│   ├── ClientStreamClient.cs
│   └── BiDiStreamClient.cs
│
├── PythonClient/                  Client gRPC Python
│   ├── client.py                  Unico file da eseguire — genera gli stub e avvia il menu
│   ├── requirements.txt           grpcio, grpcio-tools, mypy-protobuf
│   ├── greet_pb2.pyi              Type stub messaggi — committato, letto da Pylance
│   ├── greet_pb2_grpc.pyi         Type stub servizi gRPC — committato, letto da Pylance
│   └── README.md                  Guida dettagliata installazione, generazione e connessione
│
└── Shared/
    └── Protos/
        └── greet.proto            Contratto unico condiviso da tutti i progetti
```

---

## Il contratto — `greet.proto`

Definisce il servizio `Greeter` con 4 RPC:

| RPC | Tipo | Direzione |
|---|---|---|
| `SayHello` | Unary | client → server, server risponde uno |
| `StreamGreetings` | Server streaming | client → server, server risponde N |
| `SendGreetings` | Client streaming | client invia N → server risponde uno |
| `Chat` | Bidirectional streaming | client e server si scambiano N messaggi |

---

## Rigenerazione degli stub .NET

I file C# vengono rigenerati automaticamente al build grazie al tag `<Protobuf>` nei `.csproj`.
Non è necessario nessun passo manuale.

---

## Client C# — `ClientApp`

Si connette su **`https://localhost:63174`** (HTTPS, certificato self-signed accettato con
`DangerousAcceptAnyServerCertificateValidator`). Avviare prima `ServerApp`, poi `ClientApp`.

---

## Client Python — `PythonClient`

Si connette sullo **stesso endpoint del client C#**: `https://localhost:63174`.
All'avvio, `client.py` rigenera automaticamente gli stub runtime dal `.proto` tramite
`grpc_tools.protoc`, senza nessun passo manuale.

I file `.pyi` (`greet_pb2.pyi`, `greet_pb2_grpc.pyi`) sono committati nel repo e vengono
letti da Pylance per fornire i tipi corretti nell'IDE (`name: str`, `response: HelloReply`, ecc.).

Per tutti i dettagli su installazione, generazione e funzionamento dei file:
→ [`PythonClient/README.md`](PythonClient/README.md)

---

## Test rapido con grpcurl (solo Unary)

```
grpcurl -insecure -d '{"name": "Mario"}' localhost:63174 GrpcDemo.Greeter/SayHello
```
