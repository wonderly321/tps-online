from apps.users.model.user_model import *
from apps.player import PlayerPool

class Users:
    @classmethod
    def login(cls, name, pwd, **kwargs):
        user = fetch_user_by_name(name)
        if not user:
            return {'resp_type': 0x5001, 'resp_code': -2}
        user = dict(zip(['id', 'name', 'password'], user))
        if user['password'] != pwd:
            return {'resp_type': 0x5001, 'resp_code': -1}
        player = PlayerPool.Get(user['id'])
        if not player:
            return {'resp_type': 0x5001, 'resp_code': user['id']}
        if player.broker.active:
            return {'resp_type': 0x5001, 'resp_code': -3}
        gameStatus = player.game_status
        if gameStatus.gaming == False:
            return {'resp_type': 0x5001, 'resp_code': user['id']}
        ret = {
            'resp_type': 0x5005,
            'resp_code': user['id'],
            'room_id': player.room.room_id,
            'pos_x': gameStatus.position[0],
            'pos_y': gameStatus.position[1],
            'pos_z': gameStatus.position[2],
            'hp': gameStatus.health,
            'gun': gameStatus.gun_bullet,
            'bag': gameStatus.bag_bullet,
            'score': gameStatus.score
        }
        player.Reconnect(ret)
        return {'resp_type': 0x5001, 'resp_code': user['id']}

    @classmethod
    def register(cls, name, pwd, **kwargs):
        user = fetch_user_by_name(name)
        if user:
            return {'resp_code': -4}
        user = insert_new_user(name, pwd)
        user = dict(zip(['id', 'name', 'password'], user))
        return {'resp_type': 0x5002, 'resp_code': user['id']}

    @classmethod
    def check_online(cls, user_id):
        player = PlayerPool.Get(user_id)
        if player and player.broker.active:
            return True
        return False
