import struct

class BaseMessage:
    def set_data(self, data):
        self.data = data
        return self

    def encode(self):
        raw_bytes = bytes()
        fmt = ''
        for each_fmt in self.proto[1]:
            if callable(each_fmt):
                fmt += each_fmt()
            else:
                fmt += each_fmt
        values = [self.data[x] for x in self.proto[0]]
        return struct.pack(fmt, *values)

    def decode(self, raw_bytes):
        if not self.proto:
            raise Exception("%s no proto configured"%(self.__class__.__name__))
        st = 0
        self.data = {}
        for i, key in enumerate(self.proto[0]):
            fmt = self.proto[1][i]
            if callable(fmt):
                fmt = fmt()
            length = struct.calcsize(fmt)
            self.data[key] = struct.unpack(fmt, raw_bytes[st:st+length])[0]
            st += length
        return self


class LoginMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['name_len', 'pwd_len', 'name', 'pwd'],
            [
                'i',
                'i',
                lambda: '%ds'%(self.data['name_len']),
                lambda: '%ds'%(self.data['pwd_len']),
            ])


class LoginResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['resp_type', 'resp_code'],
            ['i', 'i']
        )


class RegisterMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['name_len', 'pwd_len', 'name', 'pwd'],
            [
                'i',
                'i',
                lambda: '%ds'%(self.data['name_len']),
                lambda: '%ds'%(self.data['pwd_len']),
            ])

class CreateRoomMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['user_id', 'max_num'],
            ['i','i']
        )


class JoinRoomMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['user_id', 'room_id'],
            ['i', 'i']
        )

class JoinRoomResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['resp_type', 'room_id', 'max_num'],
            ['i', 'i', 'i']
        )
        
class ExitRoomMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['user_id', 'room_id'],
            ['i', 'i']
        )

class StartGameMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['room_id'],
            ['i']
        )
        
class RoomResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'content_length', 'content'],
            ['i', 'i', lambda: '%ds'%(self.data['content_length'])]
        )

class PlayerMoveMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'new_x', 'new_y', 'new_z', 'input_x', 'input_y','rotate_x', 'rotate_y', 'rotate_z', 'input_mouseX'],
            ['i'] * 15
        )

class PlayerMoveResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'new_x', 'new_y', 'new_z', 'input_x', 'input_y','rotate_x', 'rotate_y', 'rotate_z', 'input_mouseX'],
            ['i'] * 15
        )

class PlayerShootMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'playerID', 'bullets', 'hurt'],
            ['i'] * 9
        )

class PlayerShootResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'playerID', 'bullets', 'hurt'],
            ['i'] * 9
        )

class PlayerHurtResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'hp'],
            ['i'] * 7
        )

class PlayerScoreResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'score', 'killer', 'dead', 'killer_len', 'dead_len', 'killer_name', 'dead_name'],
            ['i'] * 11 + [
                lambda: '%ds'%self.data['killer_len'],
                lambda: '%ds'%self.data['dead_len']
            ]
        )

class PlayerReloadMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type'],
            ['i'] * 6
        )

class PlayerReloadResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type', 'gun', 'bag'],
            ['i'] * 8
        )

class PlayerJumpMessage(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type'],
            ['i'] * 6
        )

class PlayerJumpResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['msg_type', 'frameID', 'selfID', 'roomID', 'inner_msg_len', 'inner_msg_type'],
            ['i'] * 6
        )

class ReconnectResponse(BaseMessage):
    def __init__(self):
        self.proto = (
            ['resp_type', 'resp_code', 'room_id', 'pos_x', 'pos_y', 'pos_z', 'hp', 'gun', 'bag', 'score'],
            ['i'] * 10
        )