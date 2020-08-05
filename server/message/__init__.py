from message import *
import struct

MESSAGE_TYPES = {
    0x7001: 'Login',
    0x7002: 'Register',
    0x7003: 'CreateRoom',
    0x7004: 'JoinRoom',
    0x7005: 'ExitRoom',
    0x7006: 'StartGame',

    0x3001: 'Room',
    0x3002: 'Broadcast',
    0x3003: 'PlayerMove',
    0x3004: 'PlayerShoot',
    0x3005: 'PlayerHurt',
    0x3006: 'PlayerScore',
    0x3007: 'PlayerReload',
    0x3008: 'PlayerJump',
}


class Message:
    @classmethod
    def __create_message(cls, typename):
        return globals()[typename+'Message']()

    @classmethod
    def __create_response(cls, typename):
        return globals()[typename+'Response']()

    @classmethod
    def response(cls, typename, data):
        return cls.__create_response(typename).set_data(data).encode()

    @classmethod
    def request(cls, raw_bytes):
        message_type = struct.unpack('i', raw_bytes[:4])[0]
        if message_type == 0x3002:
            inner_message_type = struct.unpack('i', raw_bytes[20:24])[0]
            return cls.__create_message(
                MESSAGE_TYPES[inner_message_type]
            ).decode(raw_bytes)
        return cls.__create_message(
            MESSAGE_TYPES[message_type]
        ).decode(raw_bytes[4:])
