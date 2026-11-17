# Client gRPC Python — PythonClient

Client gRPC scritto in Python che consuma lo **stesso** contratto protobuf usato da
`ServerApp` e `ClientApp` (.NET). Il contratto è definito in un unico file condiviso:

```
Shared/Protos/greet.proto
```

Dimostra tutti e 4 i tipi di comunicazione gRPC:

| Tipo | RPC nel `.proto` | Descrizione |
|---|---|---|
| Unary | `SayHello` | il client invia un messaggio, il server risponde uno |
| Server streaming | `StreamGreetings` | il client invia uno, il server risponde con una sequenza |
| Client streaming | `SendGreetings` | il client invia una sequenza, il server risponde uno |
| Bidirezionale | `Chat` | entrambi inviano e ricevono sequenze in contemporanea |

---

## Prerequisiti

- **Python 3.8 o superiore** (consigliato 3.11 / 3.12)
- **pip** aggiornato (`python -m pip install --upgrade pip`)
- **ServerApp** in esecuzione sull'endpoint in chiaro `http://localhost:63175`

---

## Librerie ufficiali usate

| Pacchetto | Scopo |
|---|---|
| `grpcio` | Runtime gRPC: gestisce canali, chiamate, streaming |
| `grpcio-tools` | Include `protoc` + il plugin Python: genera il codice client dal `.proto` |
| `mypy-protobuf` | Plugin `protoc` che genera i type stub `.pyi` completi per messaggi **e** servizi gRPC |

Documentazione ufficiale: <https://grpc.io/docs/languages/python/>

---

## Installazione dell'ambiente

### 1. Crea e attiva un virtual environment

Dalla cartella `PythonClient/`:

```
python -m venv .venv
```

Su **Windows** attiva con:

```
.venv\Scripts\activate
```

Su **Linux / macOS** attiva con:

```
source .venv/bin/activate
```

### 2. Installa le dipendenze

Con il venv attivo:

```
pip install -r requirements.txt
```

Questo installa `grpcio` (runtime), `grpcio-tools` (compilatore) e `mypy-protobuf` (type stub).

---

## Come vengono generati gli stub dal `.proto`

### Cos'è uno stub gRPC

Il `.proto` definisce il contratto del servizio (messaggi e RPC). Per usarlo da Python,
`protoc` legge il `.proto` e genera questi file:

| File generato | Contenuto | Committato |
|---|---|---|
| `greet_pb2.py` | Classi runtime per i messaggi (`HelloRequest`, `HelloReply`) | No — generato a runtime |
| `greet_pb2.pyi` | Type stub messaggi: `name: str`, `message: str` — letto da Pylance | **Sì** |
| `greet_pb2_grpc.py` | `GreeterStub` e `GreeterServicer` runtime con tutti e 4 gli RPC | No — generato a runtime |
| `greet_pb2_grpc.pyi` | Type stub servizi: tipo di ritorno di `SayHello`, `StreamGreetings`, ecc. — letto da Pylance | **Sì** |

> I file `.py` non vengono committati: vengono ricreati ogni avvio.
> I file `.pyi` vengono committati: Pylance li legge **staticamente**, senza eseguire il codice.

### Perché servono due librerie per la generazione

`grpcio-tools` (il comando ufficiale su grpc.io) genera correttamente i file `.py` a runtime,
ma i suoi type stub (`--pyi_out`) coprono solo i messaggi (`greet_pb2.pyi`).
Il file `greet_pb2_grpc.pyi` — che descrive i tipi di ritorno di `stub.SayHello(...)` —
**non viene generato** da `--pyi_out`.

Senza `greet_pb2_grpc.pyi`, Pylance non conosce il tipo di ritorno degli RPC e lo inferisce
come `Any`, rendendo `response` e `response.message` non tipizzati nell'editor.

`mypy-protobuf` (`--mypy_out` e `--mypy_grpc_out`) risolve esattamente questo problema:
genera entrambi i file `.pyi` con le annotazioni complete.

### Generazione automatica all'avvio

`client.py` **rigenera automaticamente** i file `.py` ogni volta che viene avviato,
prima di importarli. La funzione `_genera_stub_dal_proto()` esegue internamente:

```
python -m grpc_tools.protoc
	-I <path>/Shared/Protos
	--python_out=<path>/PythonClient
	--grpc_python_out=<path>/PythonClient
	--mypy_out=<path>/PythonClient
	--mypy_grpc_out=<path>/PythonClient
	<path>/Shared/Protos/greet.proto
```

In questo modo il client è sempre allineato al contratto: se il `.proto` cambia,
basta riavviare `client.py` e gli stub vengono rigenerati senza nessun passo manuale.

### Significato delle opzioni di `protoc`

| Opzione | Effetto |
|---|---|
| `-I <dir>` | Cartella di ricerca del `.proto` |
| `--python_out=<dir>` | Genera `greet_pb2.py` — classi runtime dei messaggi |
| `--grpc_python_out=<dir>` | Genera `greet_pb2_grpc.py` — `GreeterStub` e `GreeterServicer` runtime |
| `--mypy_out=<dir>` | Genera `greet_pb2.pyi` — type stub messaggi (via `mypy-protobuf`) |
| `--mypy_grpc_out=<dir>` | Genera `greet_pb2_grpc.pyi` — type stub servizi gRPC (via `mypy-protobuf`) |

---

## Avvio del client

### 1. Avvia ServerApp

Da Visual Studio: tasto **F5** sul progetto `ServerApp`.

Da terminale (nella root della solution):

```
dotnet run --project ServerApp
```

Il server si mette in ascolto su:
- `https://localhost:63174` — HTTPS, usato dal client .NET
- `http://localhost:63175` — HTTP/2 in chiaro (h2c), usato da questo client Python

### 2. Avvia il client Python

Con il venv attivo, dalla cartella `PythonClient/`:

```
python client.py
```

### 3. Scegli un tipo di comunicazione dal menu

```
Scegli un tipo di comunicazione:
1) Unary
2) Server Streaming
3) Client Streaming
4) Bidirectional Streaming
0) Esci
```

---

## Connessione HTTPS con certificato self-signed

Il client Python usa lo **stesso endpoint del client C#**: `https://localhost:63174`.

Il server Kestrel usa un certificato self-signed di sviluppo. Il client .NET lo gestisce con
`DangerousAcceptAnyServerCertificateValidator`. Il client Python fa l'equivalente con
`ssl.get_server_certificate()`: recupera il certificato dal server prima di aprire il canale
gRPC e lo passa direttamente come `root_certificates`. In questo modo TLS funziona senza
dover installare o fidarsi del certificato nel sistema operativo.

```
ssl.get_server_certificate(("localhost", 63174))   # legge il cert dal server
grpc.ssl_channel_credentials(root_certificates=…)  # lo usa come CA di fiducia
grpc.secure_channel("localhost:63174", creds)       # canale HTTPS/gRPC
```

Non è necessaria nessuna modifica al server.

---

## Troubleshooting

### `FileNotFoundError: File .proto non trovato`

La cartella `Shared/Protos/greet.proto` non è raggiungibile dal percorso di `client.py`.
Verifica di trovarti nella cartella giusta e che la struttura della solution non sia
stata spostata.

### `ModuleNotFoundError: No module named 'grpc_tools'`

Le dipendenze non sono installate nel venv attivo. Esegui:

```
pip install -r requirements.txt
```

### `RuntimeError: Generazione degli stub gRPC dal file .proto fallita`

`protoc` ha restituito un errore. Cause possibili:
- `mypy-protobuf` non è installato nel venv (`pip install -r requirements.txt`)
- Il file `.proto` contiene errori di sintassi
- La versione di `grpcio-tools` è incompatibile con la versione di `protobuf` installata

Reinstalla le dipendenze:

```
pip install --upgrade -r requirements.txt
```

### `StatusCode.UNAVAILABLE` / `Expected SETTINGS frame`

`ServerApp` non è avviato, oppure la porta `63175` non è configurata in HTTP/2-only.
Controlla `ServerApp/Program.cs` e verifica che il server sia in esecuzione.

### I campi o la `response` mostrano `Any` nell'IDE

Pylance ha bisogno dei file `.pyi` su disco per fare l'analisi statica dei tipi.
Verifica che `greet_pb2.pyi` e `greet_pb2_grpc.pyi` siano presenti nella cartella
`PythonClient/`. Se mancano, avvia `client.py` almeno una volta: la generazione
automatica li ricrea. Se il problema persiste, riavvia il Language Server dell'editor.
