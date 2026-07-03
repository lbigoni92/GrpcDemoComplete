import grpc
import grpc.aio
from greet_pb2 import HelloReply, HelloRequest
from typing import Iterator

class GreeterStub:
    def __init__(self, channel: grpc.Channel) -> None: ...
    SayHello: grpc.UnaryUnaryMultiCallable[HelloRequest, HelloReply]
    StreamGreetings: grpc.UnaryStreamMultiCallable[HelloRequest, HelloReply]
    SendGreetings: grpc.StreamUnaryMultiCallable[HelloRequest, HelloReply]
    Chat: grpc.StreamStreamMultiCallable[HelloRequest, HelloReply]

class GreeterAsyncStub:
    def __init__(self, channel: grpc.aio.Channel) -> None: ...
    SayHello: grpc.aio.UnaryUnaryMultiCallable[HelloRequest, HelloReply]
    StreamGreetings: grpc.aio.UnaryStreamMultiCallable[HelloRequest, HelloReply]
    SendGreetings: grpc.aio.StreamUnaryMultiCallable[HelloRequest, HelloReply]
    Chat: grpc.aio.StreamStreamMultiCallable[HelloRequest, HelloReply]

class GreeterServicer:
    def SayHello(self, request: HelloRequest, context: grpc.ServicerContext) -> HelloReply: ...
    def StreamGreetings(self, request: HelloRequest, context: grpc.ServicerContext) -> Iterator[HelloReply]: ...
    def SendGreetings(self, request_iterator: Iterator[HelloRequest], context: grpc.ServicerContext) -> HelloReply: ...
    def Chat(self, request_iterator: Iterator[HelloRequest], context: grpc.ServicerContext) -> Iterator[HelloReply]: ...

def add_GreeterServicer_to_server(servicer: GreeterServicer, server: grpc.Server) -> None: ...
