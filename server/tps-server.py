from threading import Thread
from network.tcp_server import TcpServer
import logging
logging.basicConfig(filename='log.txt', filemode='w', level=logging.INFO, format='%(asctime)s [%(levelname)s]%(message)s')

class Server:
    def __init__(self, port=5000):
        self.tcpServer = TcpServer(port)

    def run(self):
        tcpServerThread = Thread(target=self.tcpServer.run)
        tcpServerThread.start()


if __name__ == '__main__':
    server = Server()
    server.run()
