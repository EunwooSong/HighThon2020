using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour {

    private bool isGameOver = false;

    [Header("[Player Data]")]
    public List<GameObject> playerPrefab;
    public GameObject CoinPrefab;
    public GameObject OccPrefab;

    private Dictionary<string, GameObject> players;
    private List<int> player_Rank;
    public GameObject isMine_Obj;                       //내가 조종할 플레이어

    [Header("[Spawn Point(Coins)]")]
    public Transform[] coinSpawnPoints;
    public Transform[] happyBoxSpawnPoints;

    [Header("[Player Spawn Point]")]
    public Transform[] playerSpawnPoint;

    [Header("[Player Score]")]
    int currentScore;       //현재 가지고 있는 코인
    int totalScore;         //저장한 코인 총합
    int rank;               //지금 몇 위인지

    [Header("[UI Controller]")]
    public Sprite CanAttack;
    public Sprite CantAttack;
    public Sprite CanSkill;
    public Sprite CantSkill;
    public Image attack_ImageField;
    public Image skill_ImageField;

    public Text current_PlayerScore_Field;
    public Text total_PlayerSocre_Field;
    public Text playerRank_Field;
    public Text gameTimer_Field;

    void Awake()
    {
        //Add Player Prefab
        playerPrefab.Add(Resources.Load("Prefabs/Player_1") as GameObject); //플레이어 타입 1
        playerPrefab.Add(Resources.Load("Prefabs/Player_2") as GameObject); //플레이어 타입 2

        CoinPrefab = Resources.Load("Prefabs/Coin") as GameObject;
        OccPrefab = Resources.Load("Prefabs/Occ") as GameObject;
        CanAttack = Resources.Load("Image/Skill_1") as Sprite;
        CantAttack = Resources.Load("Image/Skill_0") as Sprite;
        //CanSkill = Resources.Load("") as Sprite;
        //CantSkill = Resources.Load("") as Sprite;

        //Get UI Component
        attack_ImageField   = GameObject.Find("Attack").GetComponent<Image>();
        //skill_ImageField    = GameObject.Find("Skill").GetComponent<Image>();
        playerRank_Field    = GameObject.Find("RankText").GetComponent<Text>();
        gameTimer_Field     = GameObject.Find("Game_Timer").GetComponent<Text>();
        current_PlayerScore_Field   = GameObject.Find("Current_Coin_Count_Text").GetComponent<Text>();
        total_PlayerSocre_Field     = GameObject.Find("Total_Coin_Count_Text").GetComponent<Text>();

        //Set Dictionary
        players = new Dictionary<string, GameObject>();
        player_Rank = new List<int>();
    }

    // Use this for initialization
    void Start() {

        //플레이어의 위치 전송
        SendPlayerSpawn();

        StartCoroutine(SendPlayerData());           //Send Player Data
        StartCoroutine(Spawn_Coin());               //Send Spawn Coin

        if (UserData.Get_RoomMater())
            StartCoroutine(Master_Send_Game_Timer());   //Send Game Timer(Only Master)

        Spawn_Occupation();
    }

    // Update is called once per frame
    void Update() {
        if (isGameOver)
            return;

        InitPlayersData();  //플레이어 생성 - 아마도 한 번만 실행
        SetPlayerData();    //플레이어 설정
        Set_Obj_Master();   //생성하는 오브젝트 배치

        //UI 설정
        Set_UI();
        Set_Score();        //스코어 설정
        Set_Timer();        //타이머 설정

        GameOver_Get();     //게임 종료
    }

    void Set_UI()
    {
        PlayerCtrl p = isMine_Obj.GetComponent<PlayerCtrl>();
        current_PlayerScore_Field.text = "" + p.CurrentCoin;
        total_PlayerSocre_Field.text = "" + totalScore;
        playerRank_Field.text = "" + rank;



        if (p.canAttack)
        {
            attack_ImageField.sprite = CanAttack;
        }
        else
        {
            attack_ImageField.sprite = CantAttack;
        }
    }

    //플레이어 생성 - 위치, 타입 Emit
    void SendPlayerSpawn()
    {
        int type = Random.Range(0, playerPrefab.Count - 1); //타입 랜덤 설정 (추후 UserData)
        int pos = Random.Range(0, playerSpawnPoint.Length - 1); //플레이어의 위치 설정
        string nickname = UserData.Get_userID();

        JSONObject json = new JSONObject();
        json.AddField("nickname", nickname);
        json.AddField("type", type);
        json.AddField("pos", pos);

        string form = UserData.Get_roomID() + "/" + json.ToString();
        Socket_Manager.instance.socket.Emit("PlayerSpawnPoint", form);
    }

    //받은 플레이어의 정보를 바탕으로 생성
    void InitPlayersData()
    {
        int count = Socket_Manager.instance.inGame_PlayerInit.Count;
        if (count <= 0)
            return;

        for(int i = 0; i < count; i++)
        {
            Debug.Log(i + "_Init Data : " + Socket_Manager.instance.inGame_PlayerInit[i]);
            JSONObject json = new JSONObject(Socket_Manager.instance.inGame_PlayerInit[i]);

            int pos = (int)json.GetField("pos").n;
            int type = (int)json.GetField("type").n;
            string nickname = json.GetField("nickname").str;

            GameObject obj = Instantiate(playerPrefab[type]);
            obj.transform.position = playerSpawnPoint[pos].position;

            Debug.Log("Creating Player . . . " + nickname);
            if(nickname.Equals(UserData.Get_userNickname()))
            {
                isMine_Obj = obj;
                isMine_Obj.GetComponent<PlayerCtrl>().isMine = true;
                Camera.main.transform.parent.GetComponent<CameraMovement>().target = isMine_Obj.transform;
                
                Debug.Log("My Obj - " + nickname);
            }

            obj.GetComponent<PlayerCtrl>().nickname = nickname;
            players.Add(nickname, obj);
        }

        //버퍼 초기화
        Socket_Manager.instance.inGame_PlayerInit.Clear();
        Debug.Log("Created " + count + "Players!");
    }

    //플레이어 위치 보내기 - 위치, 스킬 사용, 등등, 물리 힘
    IEnumerator SendPlayerData()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f / 50.0f);

            if (isMine_Obj != null)
            {
                PlayerCtrl pCtrl = isMine_Obj.GetComponent<PlayerCtrl>();
                bool isAttack = pCtrl.isAttack;
                bool isSkill = pCtrl.isSkill;
                Vector2 pos = pCtrl.transform.position;

                JSONObject json = new JSONObject();
                json.AddField("nickname", UserData.Get_userID());
                json.AddField("pos", pos.ToString());
                json.AddField("attack", isAttack);
                json.AddField("skill", isSkill);
                json.AddField("rigid", pCtrl.rigid.velocity.ToString());

                string form = UserData.Get_roomID() + "/" + json.ToString();
                Socket_Manager.instance.socket.Emit("SendPlayerState", form);
            }

            if (isGameOver)
                break;
        }
    }

    //플레이어 상태(위치) 저장 설정
    void SetPlayerData()
    {
        int count = Socket_Manager.instance.inGame_PlayerState.Count;

        if (count <= 0)
            return;

        for(int i = 0; i < count; i++)
        {
            JSONObject json = new JSONObject(Socket_Manager.instance.inGame_PlayerState[i]);
            
            string nickname = json.GetField("nickname").str;
            string pos_str = json.GetField("pos").str;
            bool isAttack = json.GetField("attack").b;
            bool isSkill = json.GetField("skill").b;
            string rigid_str = json.GetField("rigid").str;

            if (nickname.Equals(UserData.Get_userNickname()))
                continue;

            string[] pos_arr = pos_str.Substring(1, pos_str.Length - 2).Split(',');
            Vector2 pos = new Vector2(float.Parse(pos_arr[0]), float.Parse(pos_arr[1]));

            string[] rigid_arr = rigid_str.Substring(1, rigid_str.Length - 2).Split(',');
            Vector2 rigid_vel = new Vector2(float.Parse(rigid_arr[0]), float.Parse(rigid_arr[1]));

            GameObject tmp;
            players.TryGetValue(nickname, out tmp);

            PlayerCtrl p = tmp.GetComponent<PlayerCtrl>();
            tmp.transform.position = pos;   //Set Pos
            p.isAttack = isAttack;          //Set Attack
            p.isSkill = isSkill;            //Set Skill
            p.rigid.velocity = rigid_vel;   //Set Vel
        }

        Socket_Manager.instance.inGame_PlayerState.Clear();
    }

    //코인 생성 - 서버에게 이를 보냄
    IEnumerator Spawn_Coin()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f);

            int type = 0;       // 0 -> Coin
            int pos = Random.Range(0, coinSpawnPoints.Length - 1);

            JSONObject json = new JSONObject();
            json.AddField("type", type);
            json.AddField("pos", pos);

            string form = UserData.Get_roomID() + "/" + json.ToString();
            Socket_Manager.instance.socket.Emit("SendSpawnObj", form);
        }
    }

    //점령지 생성 -> Score를 먹었다는 것이 호출되면 실행, 기본 1개 제작 ??
    void Spawn_Occupation()
    {
        int type = 1;       // 0 -> Coin
        int pos = Random.Range(0, happyBoxSpawnPoints.Length - 1);

        JSONObject json = new JSONObject();
        json.AddField("type", type);
        json.AddField("pos", pos);

        string form = UserData.Get_roomID() + "/" + json.ToString();
        Socket_Manager.instance.socket.Emit("SendSpawnObj", form);
    }

    //오브젝트 배치 총 관리
    void Set_Obj_Master()
    {
        int count = Socket_Manager.instance.inGame_Spawn_Buffer.Count;

        if (count <= 0)
            return;

        for(int i = 0; i < count; i ++)
        {
            JSONObject json = new JSONObject(Socket_Manager.instance.inGame_Spawn_Buffer[i]);

            int type = (int)json.GetField("type").n;
            int pos = (int)json.GetField("pos").n;

            switch(type)
            {
                case 0:
                    Set_Coin(pos);
                    break;
                case 1:
                    Set_Occupation(pos);
                    break;
            }
        }

        Socket_Manager.instance.inGame_Spawn_Buffer.Clear();
    }

    //코인 생성
    void Set_Coin(int pos)
    {
        GameObject c = Instantiate(CoinPrefab);
        c.transform.position = coinSpawnPoints[pos].position;
    }

    //점령지 생성
    void Set_Occupation(int pos)
    {
        //점령지가 생성되고 그곳에 있는 플레이어의 수가 한 명이면 SendScore
        GameObject t = Instantiate(OccPrefab);
        t.transform.position = happyBoxSpawnPoints[pos].position;
    }

    //스코어를 받아서 저장 / 설정
    void Set_Score()
    {
        //플레이어의 정보들이 다 들어옴\
        int count = Socket_Manager.instance.inGame_Score_Buffer.Count;
        if (count <= 0)
            return;

        Debug.Log("Socre / ");
        for(int i = 0; i < count; i++)
        {
            JSONObject json = new JSONObject(Socket_Manager.instance.inGame_Score_Buffer[i]);

            for(int j = 0; j < json.GetField("value").list.Count; j++)
            {
                JSONObject each = json.GetField("value").list[j];

                string nickname = each.GetField("nickname").str;
                int t_score = (int)each.GetField("score").n;

                if(nickname.Equals(UserData.Get_userID()))
                {
                    rank = j + 1;
                    totalScore = t_score;
                }

                Debug.Log(nickname + " : " + t_score);
            }
        }

        Socket_Manager.instance.inGame_Score_Buffer.Clear();
        Debug.Log("Set Score Succeeded!! - " + rank);

        if(UserData.Get_RoomMater())
            Spawn_Occupation();
    }

    //게임 타임 전송
    IEnumerator Master_Send_Game_Timer()
    {
        int game_sec = 30;

        while(true)
        {
            string form = UserData.Get_roomID() + "/" + game_sec;
            Socket_Manager.instance.socket.Emit("SendGameTime", form);
            game_sec--;

            yield return new WaitForSeconds(1.0f);

            if (game_sec < 0)
                break;
        }

        GameOver_Send();
    }

    //받은 게임 타임 설정
    void Set_Timer()
    {
        if (Socket_Manager.instance.timerBuffer == null)
            return;

        string timer = Socket_Manager.instance.timerBuffer;

        gameTimer_Field.text = timer;

        Socket_Manager.instance.timerBuffer = null;
    }

    //게임 종료 메서드
    void GameOver_Send()
    {
        Socket_Manager.instance.socket.Emit("GameOver", UserData.Get_roomID());
        Debug.Log("GameOver!! - " + UserData.Get_roomID());
    }

    //게임 종료 메서드를 받으면 할 내용
    void GameOver_Get()
    {
        if (Socket_Manager.instance.gameOver.Length <= 0 || Socket_Manager.instance.gameOver == null)
            return;

        string pw = Socket_Manager.instance.gameOver;

        isGameOver = true;

        UserData.Set_RoomPW(pw);

        Socket_Manager.instance.gameOver = "";
        StartCoroutine(MoveScene());
    }

    //씬 이동
    IEnumerator MoveScene()
    {
        //Show PopUp
        Debug.Log("Your Rank - " + rank);
        yield return new WaitForSeconds(1.0f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("RoomLobby");
    }
}