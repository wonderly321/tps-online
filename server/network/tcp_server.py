import socket
import logging
from network.broker import Broker

class TcpServer:
    def __init__(self, port):
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.server_socket.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
        self.server_socket.bind(("0.0.0.0", port))
        self.server_socket.listen(10)
        self.broker_list = []
    
    def run(self):
        while True:
            client_sock, addr = self.server_socket.accept()
            logging.info('client <%s,%s> connected' % addr)
            self.broker_list.append(Broker(client_sock, addr, self))
