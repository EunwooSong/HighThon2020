using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;

public class Socket_Manager : MonoBehaviour
{
    //=-- Socet --=
    public static Socket_Manager instance = null;
    public Client socket { get; private set; }

    private string socketURL = "http://15.164.163.141:51234/";
    private string socketStateMsg;

    public bool roomMgr;
    public bool lobbyMgr;

    [Header(" - Buffers")]
    public string chatBuffer = null;
    public string leaveBuffer = null;
    public string playerBuffer = null;
    public string buffer = null;
    public string gameStartBuffer = null;
    public List<string> inGame_PlayerInit = null;       //게임 시작시 플레이어를 생성하기 위한 정보가 들어있음
    public List<string> inGame_PlayerState = null;      //게임의 플레이어의 정보가 들어있음 (닉네임, 위치, 기본 공격, 스킬 사용)
    public List<string> inGame_Spawn_Buffer = null;     //게임에서 생성되는 오브젝트들의 정보가 담겨있음
    public List<string> inGame_Score_Buffer = null;     //게임 스코어 버퍼
    public string timerBuffer = "";                   //게임 타이머
    public string gameOver = null;                      //게임 끝

    void Awake()
    {
        Application.runInBackground = true;

        if (instance == null)
        {
            instance = this;

            socketStateMsg = "Socket is not Connect - " + System.DateTime.Now.ToString();

            socket = new Client(socketURL);
            socket.Opened += SocketOpened;
            socket.Connect();
        }

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        roomMgr = false;
        lobbyMgr = false;

        //Buffer Set
        inGame_PlayerInit = new List<string>();
        inGame_PlayerState = new List<string>();
        inGame_Spawn_Buffer = new List<string>();
        inGame_Score_Buffer = new List<string>();
    }

    public void Start()
    {
        //=-------=[Lobby Manager]=-------=
        //Add Socket - Chat Func
        socket.On("getMessage", (data) =>
        {
            Debug.Log("Get Message");
            chatBuffer = data.Json.args[0].ToString();
            Debug.Log(chatBuffer);
        });

        //Add Socket - Leave Room
        socket.On("getLeaveMessage", (data) =>
        {
            Debug.Log("Get Leave Message");
            leaveBuffer = data.Json.args[0].ToString();
            Debug.Log(leaveBuffer);
        });

        //Add Socket - Get Player Info Func
        socket.On("RoomLoad", (data) =>
        {
            Debug.Log("Room Load");
            playerBuffer = data.Json.args[0].ToString();
            Debug.Log(playerBuffer);
        });

        //=-------=[Room Manager]=-------=
        //Add Socket - Send Main Room
        socket.On("sendMainRoom", (data) => {
            Debug.Log("Send Main Room");
            buffer = data.Json.args[0].ToString();
            Debug.Log(buffer);
        });

        //=-------=[Game Manager]=-------=
        //Add Socket - Game Start
        socket.On("GetStart", (data) =>{
            gameStartBuffer = data.Json.args[0].ToString();
        });

        //Add Socket - Game Init(Spawn Player)
        socket.On("GetPlayersData", (data) =>{
            inGame_PlayerInit.Add(data.Json.args[0].ToString());
        });

        //Add Socket - Game Progress(Get Players Data)
        socket.On("GetPlayerState", (data) => {
            inGame_PlayerState.Add(data.Json.args[0].ToString());
        });

        //Add Socket - Game Progress(Get Spawn Obj Data)
        socket.On("GetSpawnObj", (data) => {
            inGame_Spawn_Buffer.Add(data.Json.args[0].ToString());
        });

        //Add Score - Game Progress(Player Score)
        socket.On("GetScore", (data) => {
            Debug.Log(data.Json.args[0].ToString());
            inGame_Score_Buffer.Add(data.Json.args[0].ToString());
        });

        //Add Socket - Game Timer
        socket.On("GetGameTime", (data) =>
        {
            timerBuffer = data.Json.args[0].ToString();
        });

        //Add Socket - Game Over
        socket.On("GetGameOver", (data) =>
        {
            gameOver = data.Json.args[0].ToString();
        });
    }

    //ConnectSocket / 설정한 URL로 소켓 접속 시작
    public void ConnectSocket()
    {
        socket.Connect();
    }

    //DisconnectSocket / 소켓 연결 종료
    public void DisconnectSocket()
    {
        socket.Close();
        socketStateMsg = "Disconnected Socket - " + System.DateTime.Now.ToString();
    }

    //GetSocketConnectedMessage / 소켓이 연결되었는지 가져옴(string), 소켓 상태 메세지 불러옴
    public string GetSocketConnectedMessage()
    {
        return socketStateMsg;
    }

    //SocketOpened / 소켓이 접속되면 호출되는 함수
    private void SocketOpened(object sender, System.EventArgs e)
    {
        Debug.Log("Socket Opened - " + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        socketStateMsg = "Server Connected - " + System.DateTime.Now.ToString();
    }

    private void OnApplicationQuit()
    {
        Debug.Log(GetSocketConnectedMessage());
    }
}
