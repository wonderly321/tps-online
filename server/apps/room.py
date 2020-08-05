import time
import logging
import threading

from message import Message
from apps.player import PlayerPool

# RoomStatus = Enum('RoomStatus', ('WAIT', 'GAME', 'EMPTY'))

class Room:
    def __init__(self, room_id, player, max_num):
        self.room_id = room_id
        self.players = [player]
        self.max_num = max_num
        self.status = "WAIT"
        self.master = player
        t = threading.Thread(target=self.update_room_content)
        t.start()

    def JoinRoom(self, player):
        if self.status == "GAME":
            return -3
        if self.status == "EMPTY":
            return -1
        with threading.Lock():
            if len(self.players) < self.max_num:
                self.players.append(player)
                player.room = self
                return self.room_id
            else:
                return -2

    def LeaveRoom(self, player):
        if player in self.players:
            self.players.remove(player)
        player.room = None
        player.game_status.Reset()
        if len(self.players) == 0:
            self.status = "EMPTY"
        return 0

    def StartGame(self):
        self.status = "GAME"
        for player in self.players:
            player.game_status.gaming = True

    def BroadcastRoom(self, msg_type, data):
        for each_player in self.players:
            each_player.Send(msg_type, data)

    def update_room_content(self):
        while self.players and self.status == "WAIT":
            time.sleep(1)
            content = '\n'.join([x.name for x in self.players])
            room_msg = {
                'msg_type': 0x3001,
                'content_length': len(content),
                'content': content
            }
            for each_player in self.players:
                each_player.Send("Room", room_msg)
        if self.status == "GAME":
            time.sleep(0.5)
            room_msg = {
                'msg_type': 0x3001,
                'content_length': 0,
                'content': ""
            }
            for each_player in self.players:
                each_player.Send("Room", room_msg)
            time.sleep(0.5)


class RoomPool:
    rooms = {}
    @classmethod
    def CreateRoom(cls, user_id, max_num):
        player = PlayerPool.Get(user_id)
        with threading.Lock():
            room_id = 1000 + len(cls.rooms)
            room = Room(room_id, player, max_num)
            cls.rooms[room_id] = room
            player.room = room
        return {'resp_type': 0x5003, 'room_id': room_id, 'max_num': room.max_num}

    @classmethod
    def FindRoom(cls, room_id):
        with threading.Lock():
            if room_id in cls.rooms:
                return cls.rooms[room_id]
            return None

    @classmethod
    def JoinRoom(cls, user_id, room_id):
        player = PlayerPool.Get(user_id)
        room = cls.FindRoom(room_id)
        if not room:
            return {'resp_type': 0x5004, 'room_id': -1, 'max_num': 0}
        room_id = room.JoinRoom(player)
        return {'resp_type': 0x5004, 'room_id': room_id, 'max_num': room.max_num}

    @classmethod
    def ExitRoom(cls, user_id, room_id):
        player = PlayerPool.Get(user_id)
        room = cls.FindRoom(room_id)
        room.LeaveRoom(player)

    @classmethod
    def StartGame(cls, room_id):
        room = cls.FindRoom(room_id)
        room.StartGame()

    @classmethod
    def HandleMsg(cls, **kwargs):
        room = cls.FindRoom(kwargs['roomID'])
        player = PlayerPool.Get(kwargs['selfID'])
        if kwargs['inner_msg_type'] == 0x3003: # Move
            player.game_status.Move(kwargs['new_x'], kwargs['new_y'], kwargs['new_z'])
            room.BroadcastRoom("PlayerMove", kwargs)
        elif kwargs['inner_msg_type'] == 0x3004: # Shoot
            player.game_status.Shoot(kwargs['bullets'])
            resp = kwargs.copy()
            resp['bullets'] = player.game_status.gun_bullet
            player.Send("PlayerShoot", resp)
            if kwargs['playerID'] > 0:
                hurtPlayer = PlayerPool.Get(kwargs['playerID'])
                if hurtPlayer.game_status.health == 0: return
                hurtPlayer.game_status.Demage(kwargs['hurt'])
                hurtPlayer.Send("PlayerHurt",{
                    'msg_type': kwargs['msg_type'],
                    'frameID': kwargs['frameID'],
                    'selfID': hurtPlayer.user_id,
                    'roomID': kwargs['roomID'],
                    'inner_msg_len': 8,
                    'inner_msg_type': 0x3005,
                    'hp': hurtPlayer.game_status.health
                })
                if hurtPlayer.game_status.health == 0:
                    player.game_status.score += 1
                    room.BroadcastRoom("PlayerScore",{
                        'msg_type': kwargs['msg_type'],
                        'frameID': kwargs['frameID'],
                        'selfID': 0,
                        'roomID': kwargs['roomID'],
                        'inner_msg_len': 4*6 + len(player.name) + len(hurtPlayer.name),
                        'inner_msg_type': 0x3006,
                        'score': player.game_status.score,
                        'killer': player.user_id,
                        'dead': hurtPlayer.user_id,
                        'killer_len': len(player.name),
                        'dead_len': len(hurtPlayer.name),
                        'killer_name': player.name,
                        'dead_name': hurtPlayer.name
                    })
        elif kwargs['inner_msg_type'] == 0x3007: # Reload
            player = PlayerPool.Get(kwargs['selfID'])
            player.game_status.Reload()
            player.Send("PlayerReload", {
                'msg_type': kwargs['msg_type'],
                'frameID': kwargs['frameID'],
                'selfID': kwargs['selfID'],
                'roomID': kwargs['roomID'],
                'inner_msg_len': 12,
                'inner_msg_type': 0x3007,
                'gun': player.game_status.gun_bullet,
                'bag': player.game_status.bag_bullet
            })
        elif kwargs['inner_msg_type'] == 0x3008: # Jump
            room = cls.FindRoom(kwargs['roomID'])
            room.BroadcastRoom("PlayerJump", kwargs)
