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
- **ServerApp** in esecuzione su `https://localhost:63174`

---

## Librerie usate

| Pacchetto | Scopo |
|---|---|
| `grpcio` | Runtime gRPC: gestisce canali, chiamate, streaming |
| `grpcio-tools` | Include `protoc` + il plugin Python: genera i file `.py` runtime dal `.proto` |
| `mypy-protobuf` | Plugin aggiuntivo per `protoc`: genera i file `.pyi` con le annotazioni di tipo per l'IDE |

Documentazione ufficiale: <https://grpc.io/docs/languages/python/>

---

## Installazione dell'ambiente

Dalla cartella `PythonClient/`, crea e attiva un virtual environment:

```
python -m venv .venv
```

Su **Windows**:

```
.venv\Scripts\activate
```

Su **Linux / macOS**:

```
source .venv/bin/activate
```

Installa le dipendenze con il venv attivo:

```
pip install -r requirements.txt
```

---

## Come funziona: file generati, comandi e responsabilità

Il file `.proto` è la fonte di verità. Da esso `protoc` genera 4 file distinti, ognuno
con uno scopo preciso e un comando dedicato.

### Mappa completa: comando → file generato → chi lo usa

```
greet.proto
│
├── --python_out          → greet_pb2.py          (Python a runtime)
├── --grpc_python_out     → greet_pb2_grpc.py      (Python a runtime)
├── --mypy_out            → greet_pb2.pyi          (Pylance / IDE)
└── --mypy_grpc_out       → greet_pb2_grpc.pyi     (Pylance / IDE)
```

### Dettaglio di ogni file

| File | Comando che lo genera | Contenuto | Chi lo usa | Committato |
|---|---|---|---|---|
| `greet_pb2.py` | `--python_out` | Classi runtime: `HelloRequest`, `HelloReply` costruite dinamicamente dal descrittore binario | Python a **runtime** | ❌ |
| `greet_pb2_grpc.py` | `--grpc_python_out` | `GreeterStub` (client) e `GreeterServicer` (server) con i 4 RPC | Python a **runtime** | ❌ |
| `greet_pb2.pyi` | `--mypy_out` | Annotazioni statiche dei messaggi: `name: str`, `message: str` | **Pylance** / IDE | ✅ |
| `greet_pb2_grpc.pyi` | `--mypy_grpc_out` | Annotazioni statiche dei servizi: tipo di ritorno di `SayHello` → `HelloReply`, ecc. | **Pylance** / IDE | ✅ |

### Perché i `.py` non vengono committati e i `.pyi` sì

I file `.py` vengono **rigenerati automaticamente** ogni volta che `client.py` si avvia,
quindi non ha senso tenerli nel repo.

I file `.pyi` vengono **letti da Pylance staticamente**, senza eseguire il codice.
Se non fossero nel repo, clonando il progetto l'IDE non avrebbe le informazioni di tipo
finché non si avvia almeno una volta `client.py`. Committandoli, i tipi funzionano subito.

### Perché servono due librerie (`grpcio-tools` + `mypy-protobuf`)

`grpcio-tools` è la libreria ufficiale gRPC. Il suo flag `--pyi_out` genera solo
`greet_pb2.pyi` (i messaggi), ma **non genera `greet_pb2_grpc.pyi`** (i servizi).

Senza `greet_pb2_grpc.pyi`, Pylance non conosce il tipo di ritorno di `stub.SayHello(...)`
e lo mostra come `Any`. Anche `response.message` diventa `Any`.

`mypy-protobuf` aggiunge i due flag `--mypy_out` e `--mypy_grpc_out` che generano
entrambi i `.pyi` con tutte le annotazioni complete.

| Flag | Libreria che lo fornisce | File generato |
|---|---|---|
| `--pyi_out` | `grpcio-tools` (ufficiale) | solo `greet_pb2.pyi` |
| `--mypy_out` | `mypy-protobuf` | `greet_pb2.pyi` (più completo) |
| `--mypy_grpc_out` | `mypy-protobuf` | `greet_pb2_grpc.pyi` ← quello che mancava |

### Generazione automatica all'avvio

`client.py` esegue automaticamente `protoc` all'avvio tramite la funzione
`_genera_stub_dal_proto()`, prima di importare qualsiasi stub. Il comando equivalente è:

```
python -m grpc_tools.protoc
	-I Shared/Protos
	--python_out=PythonClient
	--grpc_python_out=PythonClient
	--mypy_out=PythonClient
	--mypy_grpc_out=PythonClient
	Shared/Protos/greet.proto
```

Se `mypy-protobuf` non è installato nel venv, i plugin `.pyi` vengono saltati
automaticamente (la generazione non fallisce) e vengono usati i `.pyi` committati nel repo.

---

## Avvio del client

### 1. Avvia ServerApp

Da Visual Studio: tasto **F5** sul progetto `ServerApp`.

Da terminale (nella root della solution):

```
dotnet run --project ServerApp
```

Il server espone `https://localhost:63174`, lo stesso endpoint usato dal client C#.

### 2. Avvia il client Python

Con il venv attivo, dalla cartella `PythonClient/`:

```
python client.py
```

All'avvio vengono rigenerati automaticamente i file `.py`, poi compare il menu:

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

Il client Python si connette su **`https://localhost:63174`**, lo stesso endpoint del
client C#. Il server Kestrel usa un certificato self-signed di sviluppo.

Il client C# lo gestisce con `DangerousAcceptAnyServerCertificateValidator`.
Il client Python fa l'equivalente con `ssl.get_server_certificate()`:

```
ssl.get_server_certificate(("localhost", 63174))    # scarica il cert da Kestrel
grpc.ssl_channel_credentials(root_certificates=…)   # lo usa come CA fidata
grpc.secure_channel("localhost:63174", creds)        # canale gRPC su TLS
```

TLS funziona senza installare certificati nel sistema operativo e senza modificare il server.

---

## Troubleshooting

### `FileNotFoundError: File .proto non trovato`

`Shared/Protos/greet.proto` non è raggiungibile. Verifica che la struttura della
solution non sia stata spostata rispetto alla cartella `PythonClient/`.

### `ModuleNotFoundError: No module named 'grpc_tools'`

Il venv non ha le dipendenze installate:

```
pip install -r requirements.txt
```

### `RuntimeError: Generazione degli stub gRPC dal file .proto fallita`

`protoc` ha restituito errore. Cause possibili:
- versione di `grpcio-tools` incompatibile con `protobuf` installata
- file `.proto` con errori di sintassi

Reinstalla le dipendenze:

```
pip install --upgrade -r requirements.txt
```

### `StatusCode.UNAVAILABLE` alla connessione

`ServerApp` non è avviato oppure sta ancora inizializzando. Verifica che il processo
sia in ascolto su `https://localhost:63174`.

### I campi o la `response` mostrano `Any` nell'IDE

I file `greet_pb2.pyi` e `greet_pb2_grpc.pyi` non sono presenti su disco.
Sono committati nel repo: se mancano, fai `git checkout` oppure avvia `client.py`
una volta per rigenerarli. Se il problema persiste dopo averli recuperati,
riavvia il Language Server dell'editor.
