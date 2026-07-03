"""
Client gRPC Python per il servizio Greeter definito in Shared/Protos/greet.proto.
Replica i 4 tipi di comunicazione del ClientApp .NET:
  1) Unary            -> SayHello
  2) Server streaming -> StreamGreetings
  3) Client streaming -> SendGreetings
  4) Bidirectional    -> Chat

Usa lo stesso endpoint del client C#: HTTPS su localhost:63174 con certificato
self-signed (validazione disabilitata), identico a DangerousAcceptAnyServerCertificateValidator.

La parte client di comunicazione (gli stub greet_pb2.py e greet_pb2_grpc.py)
viene generata automaticamente all'avvio a partire dal file .proto condiviso,
così il client resta sempre allineato al contratto del servizio.
"""
import os
import ssl
import sys

import grpc


def _genera_stub_dal_proto() -> None:
    """Genera gli stub gRPC e i type stub dal file .proto.

    Produce 4 file nella cartella PythonClient:
      greet_pb2.py          -- classi runtime dei messaggi (protoc)
      greet_pb2.pyi         -- type stub messaggi (mypy-protobuf)
      greet_pb2_grpc.py     -- GreeterStub/Servicer runtime (grpc plugin)
      greet_pb2_grpc.pyi    -- type stub servizi gRPC (mypy-protobuf grpc plugin)

    I percorsi sono assoluti, quindi funziona anche quando la working
    directory è diversa da PythonClient (es. avvio da Visual Studio).
    """
    base_dir = os.path.dirname(os.path.abspath(__file__))
    proto_dir = os.path.normpath(os.path.join(base_dir, "..", "Shared", "Protos"))
    proto_file = os.path.join(proto_dir, "greet.proto")

    if not os.path.isfile(proto_file):
        raise FileNotFoundError(f"File .proto non trovato: {proto_file}")

    # Individua la cartella Scripts/bin del venv corrente partendo dall'eseguibile
    # Python in uso. In questo modo i plugin protoc-gen-mypy vengono trovati anche
    # quando Visual Studio non aggiunge il venv al PATH di sistema.
    scripts_dir = os.path.dirname(sys.executable)
    ext = ".exe" if sys.platform == "win32" else ""
    plugin_mypy      = os.path.join(scripts_dir, f"protoc-gen-mypy{ext}")
    plugin_mypy_grpc = os.path.join(scripts_dir, f"protoc-gen-mypy_grpc{ext}")

    from grpc_tools import protoc

    args = [
        "grpc_tools.protoc",
        f"-I{proto_dir}",
        f"--python_out={base_dir}",
        f"--grpc_python_out={base_dir}",
            f"--plugin=protoc-gen-mypy={plugin_mypy}",
        ]

    args.append(proto_file)

    exit_code = protoc.main(args)
    if exit_code != 0:
        raise RuntimeError(
            "Generazione degli stub gRPC dal file .proto fallita "
            f"(codice {exit_code})."
        )

    # Rende importabili gli stub appena generati.
    if base_dir not in sys.path:
        sys.path.insert(0, base_dir)


# Genera la parte client di comunicazione PRIMA di importarla.
_genera_stub_dal_proto()

import greet_pb2  # noqa: E402  (import dopo la generazione degli stub)
import greet_pb2_grpc  # noqa: E402

# Stesso endpoint del client C#: HTTPS con certificato self-signed accettato.
SERVER_ADDRESS = "localhost:63174"


def unary(stub: greet_pb2_grpc.GreeterStub) -> None:
	print("Client invia: Mario")
	response = stub.SayHello(greet_pb2.HelloRequest(name="Mario"))
	print("Ricevo dal server: " + response.message)


def server_streaming(stub: greet_pb2_grpc.GreeterStub) -> None:
	for response in stub.StreamGreetings(greet_pb2.HelloRequest(name="Anna")):
		print("Ricevo dal server: " + response.message)


def client_streaming(stub: greet_pb2_grpc.GreeterStub) -> None:
	def requests():
		for name in ("Uno", "Due", "Tre"):
			print("Invio dal client: " + name)
			yield greet_pb2.HelloRequest(name=name)

	response = stub.SendGreetings(requests())
	print("Risposta dal server: " + response.message)


def bidi_streaming(stub: greet_pb2_grpc.GreeterStub) -> None:
	def requests():
		for name in ("A", "B", "C"):
			print("Client: " + name)
			yield greet_pb2.HelloRequest(name=name)

	for response in stub.Chat(requests()):
		print("Server: " + response.message)


def main() -> None:
	# Recupera il certificato self-signed direttamente dal server (senza validarlo),
	# esattamente come DangerousAcceptAnyServerCertificateValidator nel client C#.
	host, port_str = SERVER_ADDRESS.split(":")
	server_cert_pem = ssl.get_server_certificate((host, int(port_str))).encode()
	ssl_credentials = grpc.ssl_channel_credentials(root_certificates=server_cert_pem)

	with grpc.secure_channel(SERVER_ADDRESS, ssl_credentials) as channel:
		stub = greet_pb2_grpc.GreeterStub(channel)
		actions = {
			"1": unary,
			"2": server_streaming,
			"3": client_streaming,
			"4": bidi_streaming,
		}
		while True:
			print("\nScegli un tipo di comunicazione:")
			print("1) Unary")
			print("2) Server Streaming")
			print("3) Client Streaming")
			print("4) Bidirectional Streaming")
			print("0) Esci")
			choice = input("> ").strip()
			if choice == "0":
				break
			action = actions.get(choice)
			if action is None:
				print("Scelta non valida.")
				continue
			try:
				action(stub)
			except grpc.RpcError as err:
				print(f"Errore gRPC: {err.code()} - {err.details()}")
				print(f"Verifica che ServerApp sia in esecuzione su https://{SERVER_ADDRESS}")


if __name__ == "__main__":
	main()
