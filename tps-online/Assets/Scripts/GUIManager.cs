using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {
    public InputField nameInput;
    public InputField passwordInput;
    public InputField hostInput;
    public InputField portInput;
    public InputField roomCapacityInput;
    public InputField roomIDInput; 
    public Text showRoomCapacity;
    public Text showRoomID;
    public InputField roomContent;

    private Client client;

    // canvas
    private GameObject LoginCanvas;
    private GameObject ConnectCanvas;

    private GameObject MainCanvas;
    private GameObject RoomCanvas;
    private GameObject nowActiveCanvas;
    private GameManager gameManager;

    private AudioSource audioPlayer;

	// Use this for initialization
	void Start () {
        client = Client.client;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ConnectCanvas = GameObject.Find("ConnectCanvas");
        LoginCanvas = GameObject.Find("LoginCanvas");    
        MainCanvas = GameObject.Find("MainCanvas");
        RoomCanvas = GameObject.Find("RoomCanvas");
        LoginCanvas.SetActive(false);
        MainCanvas.SetActive(false);
        RoomCanvas.SetActive(false);
        hostInput.text = "localhost";
        portInput.text = "5000";
        audioPlayer = GetComponent<AudioSource>();
        audioPlayer.Play(0); 
   }

    void SetActiveCanvas(GameObject canvas)
    {
        if(nowActiveCanvas == canvas) return;
        ConnectCanvas.SetActive(false);
        LoginCanvas.SetActive(false);
        MainCanvas.SetActive(false);
        RoomCanvas.SetActive(false);
        canvas.SetActive(true);
        nowActiveCanvas = canvas;
    }
	
	// Update is called once per frame
	void Update () {
        if(gameManager.roomID > 0) {
            SetActiveCanvas(RoomCanvas);
        }
        else if(gameManager.selfID > 0) {
            SetActiveCanvas(MainCanvas);
        }
        byte[] result = new byte[0];
        client.readSocket(ref result);
        if(result.Length < 2) return;
        Debug.Log(BitConverter.ToString(result));
        int begin = 0;
        int responseType = System.BitConverter.ToInt32(result, begin);
        begin += sizeof(int);
        switch (responseType)
        {
            case ProtoSettings.RESP_RECONNECT:
                Debug.Log("reconnect to game");
                gameManager.selfID = System.BitConverter.ToInt32(result, 4);
                gameManager.gameStatus = 2;
                gameManager.reconnectInfo = new MessageFactory.ReconnectInfo(result, 8);
                SceneManager.LoadScene("Scenes/main");
                break;
            case ProtoSettings.RESP_LOGIN:                
            case ProtoSettings.RESP_REGISTER:
                handle_login_response(result, begin);
                break;
            case ProtoSettings.RESP_CREATE_ROOM:
                gameManager.roomID = System.BitConverter.ToInt32(result, begin);
                Debug.Log(gameManager.roomID);
                showRoomID.text = gameManager.roomID.ToString();
                begin += sizeof(int);                
                showRoomCapacity.text = System.BitConverter.ToInt32(result, begin).ToString();
                roomCapacityInput.text = "";
                MainCanvas.SetActive(false);
                RoomCanvas.SetActive(true);
                nowActiveCanvas = RoomCanvas;
                break;
            case ProtoSettings.RESP_JOIN_ROOM:
                int  rID = System.BitConverter.ToInt32(result, begin);
                switch(rID)
                {
                    case -1:
                    //not exist
                        break;
                    case -2:
                    //room full
                        break;
                    case -3:
                    // game already on
                        break;
                    default:
                    {
                        gameManager.roomID = rID;
                        showRoomID.text = gameManager.roomID.ToString();
                        begin += sizeof(int);                
                        showRoomCapacity.text = System.BitConverter.ToInt32(result, begin).ToString();
                        roomIDInput.text = "";
                        MainCanvas.SetActive(false);
                        RoomCanvas.SetActive(true);         
                        nowActiveCanvas = RoomCanvas;
                        var btn = GameObject.Find("StartButton");
                        if(btn != null) btn.SetActive(false);
                    }break;

                }
                break;
            case ProtoSettings.MSG_ROOM:
                int contentLength = System.BitConverter.ToInt32(result, begin);
                if(contentLength == 0)
                {
                    gameManager.gameStatus = 1;
                    SceneManager.LoadScene("Scenes/main");
                }
                begin += sizeof(int);
                String str = System.Text.Encoding.Default.GetString(result, begin, contentLength);
                roomContent.text = str;
                break;

        }             
    }
    public void handle_login_response(byte[] result, int begin)
    {
        int code = System.BitConverter.ToInt32(result, begin);
        Debug.Log(code);
        switch (code)
        {
            case -1:
                // wrong password
                break;
            case -2:
                // no such user
                break;
            case -3:
                // user is already online
                break;
            case -4:
                // user already exists(register error)
                break;
            default:
                gameManager.selfID = code;
                LoginCanvas.SetActive(false);
                MainCanvas.SetActive(true);
                nowActiveCanvas = RoomCanvas;
                break;
                // success
        }
    }
    public void Connect()
    {
        String host = hostInput.text;
        Int32 port = Int32.Parse(portInput.text);
        client.setupSocket(host, port);
        if(client.socket_ready) {
            ConnectCanvas.SetActive(false);
            LoginCanvas.SetActive(true);
            nowActiveCanvas = RoomCanvas;
        }
    }
    public void Login()
    {
        string userName = nameInput.text;
        string password = passwordInput.text;
        byte[] msg = MessageFactory.SetLoginMsg(userName, password);
        Debug.Log("send msg: " + BitConverter.ToString(msg));
        client.writeSocket(msg);
    }

    public void Register()
    {
        string userName = nameInput.text;
        string password = passwordInput.text;
        if(userName.Length > 0 && password.Length > 0)
        {
            byte[] msg = MessageFactory.SetRegisterMsg(userName, password);
            Debug.Log("send msg: " + BitConverter.ToString(msg));
            client.writeSocket(msg);
        }
    }

    public void Test1Login()
    {
        byte[] msg = MessageFactory.SetLoginMsg("netease1", "123");
        client.writeSocket(msg);
    }

    public void Test2Login()
    {
        byte[] msg = MessageFactory.SetLoginMsg("netease2", "123"); 
        client.writeSocket(msg);
    }

    public void Test3Login()
    {
        byte[] msg = MessageFactory.SetLoginMsg("netease3", "123");
        client.writeSocket(msg);
    }

    public void CreateRoom()
    {
        Int32 roomCapacity = Int32.Parse(roomCapacityInput.text);
        byte[] msg = MessageFactory.SetCreateRoomMsg(roomCapacity,gameManager.selfID);
        client.writeSocket(msg);
    }
    public void JoinRoom()
    {
        Int32 roomID = Int32.Parse(roomIDInput.text);
        byte[] msg = MessageFactory.SetJoinRoomMsg(roomID,gameManager.selfID);
        client.writeSocket(msg);
    }
    public void ExitRoom()
    {
        byte[] msg = MessageFactory.SetExitRoomMsg(gameManager.selfID, gameManager.roomID);
        client.writeSocket(msg);
        RoomCanvas.SetActive(false);
        MainCanvas.SetActive(true);
    }
    public void StartGame()
    {
        byte[] msg = MessageFactory.SetStartGameMsg(gameManager.roomID);
        client.writeSocket(msg);
    }
    
}