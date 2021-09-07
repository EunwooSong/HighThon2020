using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocketTEST : MonoBehaviour {
    private string serverURL = "http://15.164.163.141/api/";

    private Text test_UI;
    private Button emit_Btn;

    private string buffer;
    // Use this for initialization

    void Awake()
    {
        //Add
        test_UI = GameObject.Find("test_UI").GetComponent<Text>();
        emit_Btn = GameObject.Find("emit_Btn").GetComponent<Button>();
    }

    void Start () {

        //방 데이터 불러오기 소켓 추가
        Socket_Manager.instance.socket.On("sendMainRoom", (data) => {
            buffer = data.Json.args[0].ToString();
        });

        emit_Btn.onClick.AddListener(Send);
    }
	
	// Update is called once per frame
	void Update () {
        test_UI.text = buffer;
	}

    void Send()
    {
        JSONObject mes = new JSONObject();
        mes.AddField("mes", "Hello, World!");

        Debug.Log("Refresh List! " + mes);
        Socket_Manager.instance.socket.Emit("MainLoad", mes.str);
    }
}
