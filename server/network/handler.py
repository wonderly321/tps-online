from apps.users.bl.users import Users
from apps.room import RoomPool
from apps.player import PlayerPool
from message import Message

class Handler:
    router = {
        'LoginMessage': (Users.login, 'Login'),
        'RegisterMessage': (Users.register, 'Login'),
        'CreateRoomMessage': (RoomPool.CreateRoom, 'JoinRoom'),
        'JoinRoomMessage': (RoomPool.JoinRoom, 'JoinRoom'),
        'ExitRoomMessage': (RoomPool.ExitRoom, ''),
        'StartGameMessage': (RoomPool.StartGame, ''),
        'PlayerMoveMessage': (RoomPool.HandleMsg, ''),
        'PlayerShootMessage': (RoomPool.HandleMsg, ''),
        'PlayerReloadMessage': (RoomPool.HandleMsg, ''),
        'PlayerJumpMessage':(RoomPool.HandleMsg, ''),
    }
    @classmethod
    def handle(cls, msg, broker):
        (func, resp_type) = cls.router[msg.__class__.__name__]
        resp_dict = func(**msg.data)
        if resp_type == '': return None
        elif resp_type == 'Login' and resp_dict['resp_code'] > 0:
            player = PlayerPool.Get(resp_dict['resp_code'])
            if player:
                player.Renew(broker)
            else:
                PlayerPool.NewPlayer(resp_dict['resp_code'], broker, msg.data['name'])
        return Message.response(resp_type, resp_dict)

