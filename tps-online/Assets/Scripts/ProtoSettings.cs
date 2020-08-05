using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProtoSettings {
    #region REQUEST
    public const int REQ_LOGIN = 0x7001;
    public const int REQ_REGISTER = 0x7002;
    public const int REQ_CREATE_ROOM = 0x7003;
    public const int REQ_JOIN_ROOM = 0x7004;
    public const int REQ_EXIT_ROOM = 0X7005;
    public const int REQ_START_GAME = 0X7006;
    #endregion

    #region RESPONSE
    public const int RESP_LOGIN = 0x5001;
    public const int RESP_REGISTER = 0x5002;
    public const int RESP_CREATE_ROOM = 0x5003;
    public const int RESP_JOIN_ROOM = 0x5004;
    public const int RESP_RECONNECT = 0X5005;
    #endregion

    #region MSG
    public const int MSG_ROOM = 0x3001;
    public const int MSG_BROADCAST = 0x3002;
    public const int MSG_PLAYER_MOVE = 0x3003;
    public const int MSG_PLAYER_SHOOT = 0x3004;
    public const int MSG_PLAYER_HURT = 0x3005;
    public const int MSG_PLAYER_SCORE = 0x3006;
    public const int MSG_PLAYER_RELOAD = 0x3007;
    public const int MSG_PLAYER_JUMP = 0x3008;

    #endregion

}
