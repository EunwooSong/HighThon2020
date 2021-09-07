using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//Lobby Scene에서 서버 주소 입력시
//Main Room Scene에서 그 주소로 연결

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;
    private string serverURL = "http://15.164.163.141/api/";

    [Header(" - Room Group")]
    public GameObject roomPrefab;
    public Dictionary<string, GameObject> roomInfoGroup;
    public GameObject roomsContent;
    public Text Show_JoinRoomInfo;
    public Button btn_Refresh;
    public Button btn_wantToJoin;

    [Header(" - Join Room Password")]
    public RoomCard wantToJoin;             //Want To Join Room Info
    public GameObject PopUp_PasswordField;  //페스워드 그룹 최상단
    public GameObject passwordField;        //GameObjects / Password Field
    public Text PopUp_JoinRoomInfo;
    public InputField input_JoinRoomPW_Field;
    public Button btn_JoinRoom;
    public Button btn_Cancel;

    [Header(" - Create Room")]
    public InputField input_Roomname_Field;
    public InputField input_RoomPW_Field;
    public Button btn_CreatRoom;

    [Header(" - Message")]
    public Text serverLog;
    public Text connecedSocket;

    [Header(" - UserInfo")]
    public Text signIn_ID;

    [Header(" - Buffer")]
    string buffer = null;

    void Awake()
    {
        instance = this;

        //Get Btn Refresh
        btn_Refresh             = GameObject.Find("RoomsInfo").transform.GetChild(0).GetComponent<Button>();
        btn_wantToJoin          = GameObject.Find("RoomsInfo").transform.GetChild(1).GetComponent<Button>();

        //Get Create Room
        input_Roomname_Field    = GameObject.Find("CreatRoom").transform.GetChild(0).GetComponent<InputField>();
        input_RoomPW_Field      = GameObject.Find("CreatRoom").transform.GetChild(1).GetComponent<InputField>();
        btn_CreatRoom           = GameObject.Find("CreatRoom").transform.GetChild(2).GetComponent<Button>();

        //Get Server, Socket Log
        serverLog       = GameObject.Find("ServerLog").GetComponent<Text>();
        connecedSocket  = GameObject.Find("Socket State Log").GetComponent<Text>();

        //Get SignIn ID
        signIn_ID = GameObject.Find("SignIn_UserInfo").GetComponent<Text>();

        //Get RoomsInfo (Room Card Parent)
        roomsContent        = GameObject.Find("Content");
        Show_JoinRoomInfo   = GameObject.Find("WantToJoin_Roomname").GetComponent<Text>();

        //Get PasswordField(PopUp)
        PopUp_PasswordField = GameObject.Find("PopUp_InputPassword");
        passwordField       = PopUp_PasswordField.transform.GetChild(0).gameObject;
        PopUp_JoinRoomInfo      = passwordField.transform.GetChild(0).GetComponent<Text>();
        input_JoinRoomPW_Field  = passwordField.transform.GetChild(1).GetComponent<InputField>();
        btn_JoinRoom            = passwordField.transform.GetChild(2).GetComponent<Button>();
        btn_Cancel              = passwordField.transform.GetChild(3).GetComponent<Button>();

        PopUp_PasswordField.SetActive(false);

        //Set Dictionary Obj
        roomInfoGroup = new Dictionary<string, GameObject>();
    }

    void Start()
    {
        //Get Room Card Prefabs
        roomPrefab = Resources.Load("Prefabs/RoomCard") as GameObject;

        //Set UserID to 
        signIn_ID.text += UserData.Get_userID();

        //Add Button Event
        btn_CreatRoom.onClick.AddListener(CreateRoom);              //방 생성
        btn_wantToJoin.onClick.AddListener(ShowPasswordField);      //패스워드 입력
        btn_Cancel.onClick.AddListener(Close_PWField);              //패스워드 입력창 닫기
        btn_JoinRoom.onClick.AddListener(JoinRoom);                 //방 접속 시도
        btn_Refresh.onClick.AddListener(Refresh);                   //방 목록 새로고침

        buffer = null;

        //방 데이터 불러오기 소켓 추가
        Refresh();
    }

    void Update()
    {
        GetBuffer();
        MainRoomLoad();     //방 화면(목록) 로드
                            //룸 삭제 요청 받아옴
                            //룸 생성 API 보내기

        //Update Want To Join Room Info
        if (wantToJoin != null)
            Show_JoinRoomInfo.text = wantToJoin.roomName.text;

        //Update Socket Log
        connecedSocket.text = Socket_Manager.instance.GetSocketConnectedMessage();
    }

    public void Refresh()
    {
        JSONObject mes = new JSONObject();
        mes.AddField("mes", "Hello, World!");

        Debug.Log("Refresh List! " + mes);
        Socket_Manager.instance.socket.Emit("MainLoad", mes.str);
    }

    void MainRoomLoad()
    {
        Set_Room();
    }

    public void CreateRoom()
    {
        StartCoroutine(CreateRoom_Post());
    }

    public void ShowPasswordField()
    {
        if (wantToJoin == null)
            return;

        if (!wantToJoin.isLock) {
            JoinRoom();
            return;
        }

        PopUp_JoinRoomInfo.text = wantToJoin.roomName.text;
        PopUp_PasswordField.SetActive(true);
    }

    public void JoinRoom()
    {
        //방 참가 요청
        StartCoroutine(JoinRoom_Post(input_JoinRoomPW_Field.text));
        Close_PWField();
    }

    public void Close_PWField()
    {
        input_JoinRoomPW_Field.text = "";
        PopUp_PasswordField.SetActive(false);
    }

    public void GetBuffer()
    {
        buffer = Socket_Manager.instance.buffer;

        Socket_Manager.instance.buffer = null;
    }

    IEnumerator CreateRoom_Post()
    {
        //프레임 종료시 서버 요청
        yield return new WaitForEndOfFrame();

        //Set Send Data
        WWWForm form = new WWWForm();
        form.AddField("roomname", input_Roomname_Field.text);
        form.AddField("nickname", UserData.Get_userID());
        form.AddField("userdata", UserData.Get_UserData());
        form.AddField("password", input_RoomPW_Field.text);
        form.AddField("personnel", 4);

        using (var w = UnityWebRequest.Post(serverURL + "room/create", form))
        {
            yield return w.SendWebRequest();

            if (w.isNetworkError || w.isHttpError)
            {
                serverLog.text = w.error;
                Debug.LogError(w.error);
            }

            else
            {
                JSONObject json = new JSONObject(w.downloadHandler.text);
                serverLog.text = json.GetField("mes").str;

                if (json.GetField("result").b)
                {
                    UserData.Set_roomID(json.GetField("mes").str);
                    //방으로 접속
                    UnityEngine.SceneManagement.SceneManager.LoadScene("RoomLobby");
                    Debug.Log(UserData.Get_roomID());
                }
                else
                {
                    //실패
                    Debug.Log("Create Failed... ");
                }
            }
        }
    }

    IEnumerator JoinRoom_Post(string userinput_PW)
    {
        string inputPassword = userinput_PW;

        Debug.Log("passwordField : " + inputPassword);
        yield return new WaitForEndOfFrame();

        //Set Send Data
        WWWForm form = new WWWForm();

        form.AddField("_id", wantToJoin.GetRoomID());
        form.AddField("password", inputPassword);
        form.AddField("nickname", UserData.Get_userNickname());
        form.AddField("userData", UserData.Get_UserData());

        Debug.Log(form.data.ToString());

        using(var w = UnityWebRequest.Post(serverURL + "room/join", form))
        {
            yield return w.SendWebRequest();

            if(w.isNetworkError || w.isHttpError)
            {
                serverLog.text = w.error;
                Debug.Log(w.error);
            }

            else
            {
                JSONObject json = new JSONObject(w.downloadHandler.text);

                if(json.GetField("result").b)
                {
                    UserData.Set_roomID(wantToJoin.GetRoomID());

                    Debug.Log("Connecting Room. . .");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("RoomLobby");
                }
                else
                {
                    Debug.Log("Join Failed. . .");
                    serverLog.text = json.GetField("mes").str;
                }
            }
        }
    }

    void Set_Room()
    {
        if (buffer == null || buffer.Length <= 0)
            return;

        JSONObject d = new JSONObject(buffer);

        RoomCard tmp = null;
        GameObject obj = null;

        if (d.GetField("value").list == null)
            return;

        for (int i = 0; i < d.GetField("value").list.Count; i++)
        {
            JSONObject room = d.GetField("value").list[i];

            //If Has Key -> Fixing Room Info
            if (roomInfoGroup.TryGetValue(room.GetField("_id").str, out obj))
            {
                tmp = obj.GetComponent<RoomCard>();

                tmp.SetRoomInfo(room.GetField("connectedUsers").n.ToString(), room.GetField("progress").b);
                Debug.Log("Fixing Room . . . Succeeded / " + room.GetField("_id").str + room.GetField("connectedUsers").str);
                continue;
            }

            //Else -> Creating Room Card
            //Create Room Card GameObject
            obj = Instantiate(roomPrefab) as GameObject;
            obj.transform.parent = roomsContent.GetComponent<RectTransform>();
            obj.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);

            //Set Each Room Info
            tmp = obj.GetComponent<RoomCard>();
            tmp.SetRoomInfo(room.GetField("roomname").str, room.GetField("personnel").str,
                                    room.GetField("connectedUsers").n.ToString(), room.GetField("passwordLock").b,
                                    room.GetField("progress").b, room.GetField("_id").str);

            //Add Room Card At RoomInfoGroup Dictionary
            roomInfoGroup.Add(room.GetField("_id").str, obj);
            Debug.Log("Creating Room . . . Succeeded / " + room.GetField("_id").str + room.GetField("connectedUsers").n.ToString());
        }

        //Clear Buffer
        buffer = null;
    }
}
