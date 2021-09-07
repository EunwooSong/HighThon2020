using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LobbyManager : MonoBehaviour {
    private string serverURL = "http://15.164.163.141/api/";

    [Header(" - Chat Group")]
    public Text chat_Text_Field;
    public InputField input_Chat_Message;
    public Text signIn_ID;
    public Button btn_Send_Chat;

    [Header(" - Users Group")]
    public GameObject userInfo_Prefab;
    public GameObject userInfo_Content;
    public Text show_Roomname;
    public Button btn_Room_Leave;

    Dictionary<string, GameObject> userInfoGroup;

    [Header(" - Server Log")]
    public Text socketState;
    public Text serverLog;
    private string chatBuffer;
    private string leaveBuffer;
    private string playerBuffer;

    [Header(" - Game Start")]
    public GameObject btn_gameStart;

    private bool isSend;

    void Awake()
    {
        //Get Chat Group Component
        chat_Text_Field     = GameObject.Find("MessageTextField").transform.GetChild(0).GetComponent<Text>();
        input_Chat_Message  = GameObject.Find("ChatField").transform.GetChild(0).GetComponent<InputField>();
        signIn_ID           = GameObject.Find("ChatField").transform.GetChild(1).GetComponent<Text>();
        btn_Send_Chat       = GameObject.Find("ChatField").transform.GetChild(2).GetComponent<Button>();

        //Get Users Group Component
        userInfo_Prefab     = Resources.Load("Prefabs/UserCard") as GameObject;
        userInfo_Content    = GameObject.Find("Content").gameObject;
        show_Roomname       = GameObject.Find("RoomsInfo").transform.GetChild(0).GetComponent<Text>();
        btn_Room_Leave      = GameObject.Find("RoomsInfo").transform.GetChild(1).GetComponent<Button>();

        userInfoGroup = new Dictionary<string, GameObject>();

        //Get ServerLog
        socketState = GameObject.Find("Socket State Log").GetComponent<Text>();
        serverLog   = GameObject.Find("ServerLog").GetComponent<Text>();

        //Get GameStart Btn
        btn_gameStart = GameObject.Find("GameStart").gameObject;

        isSend = false;

        //Clear Buffer
        chatBuffer = null;
        playerBuffer = null;
        leaveBuffer = null;
    }

    void Start()
    {
        //ADD Button Event
        btn_gameStart.GetComponent<Button>().onClick.AddListener(GameStart);
        btn_Room_Leave.onClick.AddListener(LeaveRoom);
        btn_Send_Chat.onClick.AddListener(Send_Message);

        //UI Set
        signIn_ID.text = UserData.Get_userID();
        show_Roomname.text = UserData.Get_roomID();
    }

    void Update()
    {
        GetBuffer();
        Chat_Manager();     //Chat Manager
        UsersInfo_Manager();//Update UserInfo
        Move_InGame();      //Start Game

        //Update Socket State
        socketState.text = Socket_Manager.instance.GetSocketConnectedMessage();

        if (!isSend)
        {
            string send = UserData.Get_roomID();
            Debug.Log(send);
            Socket_Manager.instance.socket.Emit("Join", send);

            isSend = true;
        }

        //Update RoomMaster - GameStart Btn
        btn_gameStart.SetActive(UserData.Get_RoomMater());
    }

    //Send Message
    void Send_Message()
    {
        Socket_Manager.instance.socket.Emit("sendMessage", UserData.Get_roomID() + "/" + UserData.Get_userNickname() + "/" + input_Chat_Message.text);
    }

    //Chat Message
    void Chat_Manager()
    {
        if (chatBuffer == null)
            return;

        string[] mes = chatBuffer.Split('/');
        chat_Text_Field.text += mes[1] + " : " + mes[2] + "\n";

        chatBuffer = null;
    }

    //User Card Manager
    void UsersInfo_Manager()
    {
        UserCard tmp = null;
        GameObject obj = null;

        if (leaveBuffer != null)
            if (userInfoGroup.TryGetValue(leaveBuffer, out obj))
            {
                Destroy(obj);
                userInfoGroup.Remove(leaveBuffer);
            }

        if (playerBuffer == null)
            return;

        Debug.Log(playerBuffer);

        JSONObject d = new JSONObject(playerBuffer);

        for(int i = 0; i < d.GetField("player").list.Count; i++)
        {
            JSONObject user = d.GetField("player").list[i];

            //IF Has Key -> Fixing Room Info
            if (userInfoGroup.TryGetValue(user.GetField("nickname").str, out obj))
            {
                tmp = obj.GetComponent<UserCard>();

                if (UserData.Get_userID().Equals(user.GetField("nickname").str))
                    UserData.Set_RoomMaster(user.GetField("master").b);

                tmp.SetUserInfo(user.GetField("master").b);
                Debug.Log("Fixing User . . . Succeeded / " + user.GetField("nickname").str);
                continue;
            }

            //Else -> Creating Room Card
            //Create Room Card GameObject
            obj = Instantiate(userInfo_Prefab) as GameObject;
            obj.transform.parent = userInfo_Content.GetComponent<RectTransform>();
            obj.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);

            //Set Each Room Info
            tmp = obj.GetComponent<UserCard>();
            tmp.SetUserInfo(user.GetField("nickname").str, user.GetField("master").b);

            userInfoGroup.Add(user.GetField("nickname").str, obj);

            //Set Master
            if (UserData.Get_userID().Equals(user.GetField("nickname").str))
                UserData.Set_RoomMaster(user.GetField("master").b);

            Debug.Log("Creating User . . . Succeeded / " + user.GetField("nickname"));
        }

        playerBuffer = null;
    }

    void LeaveRoom()
    {
        //Post, Socket
        StartCoroutine(LeaveRoom_Post());
    }

    void GameStart()
    {
        StartCoroutine(GameStart_Post());
        Debug.Log("GameStart!!");
    }

    void GetBuffer()
    {
        chatBuffer = Socket_Manager.instance.chatBuffer;
        playerBuffer = Socket_Manager.instance.playerBuffer;
        leaveBuffer = Socket_Manager.instance.leaveBuffer;

        Socket_Manager.instance.chatBuffer = null;
        Socket_Manager.instance.playerBuffer = null;
        Socket_Manager.instance.leaveBuffer = null;
    }

    IEnumerator LeaveRoom_Post()
    {
        yield return new WaitForEndOfFrame();

        WWWForm form = new WWWForm();
        form.AddField("_id", UserData.Get_roomID());
        form.AddField("nickname", UserData.Get_userID());
        form.AddField("master", UserData.Get_RoomMater().ToString());

        using (var w = UnityWebRequest.Post(serverURL + "room/leave", form))
        {
            yield return w.SendWebRequest();

            if (w.isNetworkError || w.isHttpError)
                Debug.LogError(w.error);

            else
            {
                JSONObject json = new JSONObject(w.downloadHandler.text);

                if (json.GetField("result").b)
                {
                    string send = UserData.Get_roomID() + "/" + UserData.Get_userID();

                    Socket_Manager.instance.socket.Emit("RoomLeave", send);

                    UserData.Set_roomID(null);

                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainLobby");
                }

                else
                {
                    serverLog.text = "Left Failed...! " + json.GetField("mes").str;
                    Debug.Log(serverLog.text);
                }
            }
        }
    }

    IEnumerator GameStart_Post()
    {
        yield return new WaitForEndOfFrame();

        WWWForm form = new WWWForm();
        form.AddField("_id", UserData.Get_roomID());

        using (var w = UnityWebRequest.Post(serverURL + "room/start", form))
        {
            yield return w.SendWebRequest();

            if(w.isNetworkError || w.isNetworkError)
            {
                Debug.LogError(w.error);
                serverLog.text = w.error;
            }
            else
            {
                JSONObject json = new JSONObject(w.downloadHandler.text);
                
                if(json.GetField("result").b)
                {
                    Socket_Manager.instance.socket.Emit("SendStart", UserData.Get_roomID());
                    Debug.Log("SendStart / " + UserData.Get_roomID());
                }
                else
                {
                    Debug.Log("?????");
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        LeaveRoom();
    }

    public void Move_InGame()
    {
        if (Socket_Manager.instance.gameStartBuffer == null || Socket_Manager.instance.gameStartBuffer.Length == 0)
            return;

        Debug.Log(Socket_Manager.instance.gameStartBuffer);
        Socket_Manager.instance.gameStartBuffer = null; 
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
    }
}
