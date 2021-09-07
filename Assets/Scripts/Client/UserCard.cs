using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserCard : MonoBehaviour {

    [Header(" - Room Card Child GameObject")]
    public Text userName;
    public Text playCount;
    public Text winCount;
    public GameObject master;

    public bool isLock;

    // Start is called before the first frame update
    void Awake()
    {
        //각 컴포넌트 자동 추가
        userName = this.transform.GetChild(0).GetComponent<Text>();
        master = this.transform.GetChild(1).gameObject;
        playCount = this.transform.GetChild(2).GetComponent<Text>();
        winCount = this.transform.GetChild(3).GetComponent<Text>();
    }

    //방 최초 생성 시
    public void SetUserInfo(string _userName, bool _master, string _playCount = "0", string _winCount = "0")
    {
        userName.text = _userName;
        playCount.text = _playCount;
        winCount.text = _winCount;

        master.SetActive(_master);
    }

    //방 정보 수정 시
    public void SetUserInfo(bool _master)
    {
        master.SetActive(_master);
    }
}
