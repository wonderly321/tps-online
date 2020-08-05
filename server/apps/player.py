from message import Message
import binascii
import logging

class GameStatus:
    def __init__(self):
        self.Reset()

    def Reset(self):
        self.gaming = False
        self.health = 100
        self.gun_bullet = 10
        self.bag_bullet = 100
        self.position = (0, 0, 0)
        self.score = 0

    def Move(self, x, y, z):
        self.position = (x, y, z)

    def Demage(self, hurt):
        self.health -= hurt
        self.health = max(0, self.health)

    def Shoot(self, blt):
        self.gun_bullet -= blt
        self.gun_bullet = max(0, self.gun_bullet)

    def Reload(self):
        tmp = 10 - self.gun_bullet
        tmp = min(tmp, self.bag_bullet)
        self.gun_bullet += tmp
        self.bag_bullet -= tmp


class Player:
    def __init__(self, user_id, broker, name):
        self.user_id = user_id
        self.broker = broker
        broker.player = self
        self.name = name
        self.room = None
        self.game_status = GameStatus()
        self.reconnect_info = None

    def Reconnect(self, info):
        print(info)
        self.reconnect_info = info

    def Renew(self, broker):
        self.broker = broker
        if self.reconnect_info:
            self.Send("Reconnect", self.reconnect_info)
            self.reconnect_info = None

    def Send(self, msg_type, msg_dict):
        if self.broker.active:
            msg = Message.response(msg_type, msg_dict)
            logging.info('<%s> Broadcast %s'%(self.name, binascii.b2a_hex(msg).decode('utf-8')))
            self.broker.sock.send(msg)
        else:
            self.LostConnect()

    def LostConnect(self):
        if self.room and self.game_status.gaming == False:
            self.room.LeaveRoom(self)


class PlayerPool:
    uid2player = {}

    @classmethod
    def NewPlayer(cls, user_id, broker, name):
        if user_id not in cls.uid2player:
            cls.uid2player[user_id] = Player(user_id, broker, name)
        else:
            cls.uid2player[user_id].Renew(broker)

    @classmethod
    def Get(cls, user_id):
        return cls.uid2player.get(user_id, None)
