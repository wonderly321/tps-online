import logging
import binascii
from threading import Thread
from message import Message
from network.handler import Handler

class Broker:
    def __init__(self, client_sock, addr, server):
        self.sock = client_sock
        self.addr = addr
        self.active = True
        self.server = server
        self.t = Thread(target=self._run)
        self.t.start()
        self.player = None

    def __del__(self):
        self.sock.close()

    def _handle(self, recv_data):
        logging.info('<%s> recv %s'%(self.addr, binascii.b2a_hex(recv_data).decode('utf-8')))
        msg = Message.request(recv_data)
        logging.info('<%s> request %s'%(self.addr, msg.data))
        raw_byte_resp = Handler.handle(msg, self)
        if not raw_byte_resp: return
        logging.info('<%s> response %s'%(self.addr, binascii.b2a_hex(raw_byte_resp).decode('utf-8')))
        self.sock.send(raw_byte_resp)


    def _run(self):
        while True:
            try:
                data = self.sock.recv(4096)
                if data:
                    t = Thread(target=self._handle, args=(data,))
                    t.start()
                else:
                    logging.info('<%s> disconnect!'%(self.addr))
                    self.sock.close()
                    self.active = False
                    if self.player:
                        self.player.LostConnect()
                    break
            except Exception as e:
                logging.error('<%s> recv data exception: %s'%(self.addr,repr(e)))
                self.sock.close()
                self.active = False
                if self.player:
                    self.player.LostConnect()
                break

