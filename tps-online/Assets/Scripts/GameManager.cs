using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    private Client client;
    // Player
    private Dictionary<int, GameObject> id2obj;
    public int selfID;

    // Game
    private int frameID = 0;
    private Queue<MessageFactory.GameMsg> queue;
    public int gameStatus = 0;
    private StateManager stateMgr;

    //Time
    public float AccumulateTime;//real time + deltatime
    public float Interpolation;
    public int roomID;
    
    // Reconnect
    public MessageFactory.ReconnectInfo reconnectInfo;

    
    void Awake()
    {
        client = Client.client;
        queue = new Queue<MessageFactory.GameMsg>();
        GameObject.DontDestroyOnLoad(this);
    }

    void Start()
    {
        id2obj = new Dictionary<int, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameStatus <= 0) return;
        if(stateMgr == null) stateMgr = GameObject.Find("StateManager").GetComponent<StateManager>();
        AccumulateTime = AccumulateTime + Time.deltaTime;
        Interpolation = AccumulateTime / Time.fixedDeltaTime - frameID;
    }

    void FixedUpdate()
    {
        if(gameStatus <= 0) return;
        if(gameStatus == 2) { //reconnect
            roomID = reconnectInfo.roomID;
            stateMgr.HP = reconnectInfo.hp;
            stateMgr.gunBullet = reconnectInfo.gun;
            stateMgr.bagBullet = reconnectInfo.bag;
            stateMgr.score = reconnectInfo.score;
            var player = GameObject.Find("SelfPlayer/Graphics/player");
            id2obj[selfID] = player;
            player.GetComponent<PlayerMotorSelf>().playerID = selfID;
            player.GetComponent<PlayerMotorSelf>().logicPosition = reconnectInfo.position;
            gameStatus = 1;
        }
        frameID += 1;
        byte[] msg_with_headers = new byte[0];
        client.readSocket(ref msg_with_headers);
        if(msg_with_headers.Length < 20) return;
        Debug.Log(BitConverter.ToString(msg_with_headers));
        int begin = 0;
        while(begin < msg_with_headers.Length) {
            MessageFactory.GameMsg gameMsg = new MessageFactory.GameMsg(msg_with_headers, begin);
            if(gameMsg.frameID > 0) {
                queue.Enqueue(gameMsg);
            }
            begin += gameMsg.Length;
        }
        
        while(queue.Count > 0) {
            var _gameMsg = queue.Peek() as MessageFactory.GameMsg;           
            if(_gameMsg.frameID + 5 <= frameID) {
                _handle_msg(_gameMsg);
                queue.Dequeue();
            }
            else break;
        }
    }

    private void _handle_msg(MessageFactory.GameMsg gameMsg) 
    {
        if(gameMsg.selfID == 0) {
            var msg = gameMsg.msg;
            int inner_msg_type = BitConverter.ToInt32(msg, 0);
            switch(inner_msg_type) {
                case ProtoSettings.MSG_PLAYER_SCORE:
                    int score = BitConverter.ToInt32(msg, 4);
                    int killer_id = BitConverter.ToInt32(msg, 8);
                    int dead_id = BitConverter.ToInt32(msg, 12);
                    int killer_len = BitConverter.ToInt32(msg, 16);
                    int dead_len = BitConverter.ToInt32(msg, 20);
                    String killer_name = System.Text.Encoding.Default.GetString(msg, 24, killer_len);
                    String dead_name = System.Text.Encoding.Default.GetString(msg, 24+killer_len, dead_len);
                    if(killer_id == selfID) {
                        stateMgr.score = score;
                    }
                    stateMgr.contentQueue.Enqueue(killer_name + " killed " + dead_name + ", get 1 score.");
                    var obj = id2obj[dead_id];
                    obj.GetComponent<PlayerMotor>().OtherDie();
                    break;
            }
        }
        else if(id2obj.ContainsKey(gameMsg.selfID)) {
            GameObject obj = id2obj[gameMsg.selfID];
            if(gameMsg.selfID == selfID)
                obj.GetComponent<PlayerMotorSelf>().HandleMsg(gameMsg.msg);
            else
                obj.GetComponent<PlayerMotor>().HandleMsg(gameMsg.msg);
        }
        else {
            Debug.Log("new player show up");
            if(gameMsg.selfID == selfID) {
                var player = GameObject.Find("SelfPlayer/Graphics/player");
                id2obj[gameMsg.selfID] = player;
                player.GetComponent<PlayerMotorSelf>().playerID = gameMsg.selfID;
                player.GetComponent<PlayerMotorSelf>().HandleMsg(gameMsg.msg);
            }
            else {
                GameObject PlayerPrefab = Resources.Load("Prefabs/Player") as GameObject;
                GameObject Player = Instantiate(PlayerPrefab);
                var player = Player.transform.Find("Graphics/otherPlayer").gameObject;
                id2obj[gameMsg.selfID] = player;
                player.GetComponent<PlayerMotor>().playerID = gameMsg.selfID;
                player.GetComponent<PlayerMotor>().HandleMsg(gameMsg.msg);
            }
        }
    }

    public void LeaveRoom()
    {
        gameStatus = 0;
        var msg = MessageFactory.SetExitRoomMsg(selfID, roomID);
        client.writeSocket(msg);
        roomID = 0;
        queue.Clear();
        AccumulateTime = 0;
        frameID = 0;
        id2obj.Clear();
        stateMgr = null;
        SceneManager.LoadScene("Scenes/welcome");
    }

    public void SendMsg(byte[] msg)
    {
        byte[] msg_with_headers = MessageFactory.SetHeaders(
            frameID,
            selfID,
            roomID,
            msg
        );
        client.writeSocket(msg_with_headers);
    }

    void OnApplicationQuit()
    {
        client.closeSocket();
    }
}
