using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccCtrl : MonoBehaviour
{
    PlayerCtrl p;
    float max_time = 3.0f;
    float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Send_Score()
    {
        if (!p.isMine)
        {
            p.CurrentCoin = 0;
            return;
        }

        string nickname = UserData.Get_userID();
        int get_Coin = p.CurrentCoin;

        string form = UserData.Get_roomID() + "/" + nickname + "/" + get_Coin;
        Socket_Manager.instance.socket.Emit("SendScore", form);

        p.CurrentCoin = 0;
        Debug.Log("SendSocre : " + form);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag.Equals("Player"))
            if (p != null)
                p = coll.GetComponent<PlayerCtrl>();
    }

    void OnTriggerStay2D(Collider2D coll)
    {
        if (p != null)
            timer += Time.deltaTime;

        else if (p == null)
        {
            p = coll.GetComponent<PlayerCtrl>();
            timer = 0.0f;
        }

        if (timer >= max_time)
        {
            Send_Score();
            Destroy(gameObject);
        }
        
        Debug.Log("Test OCC : " + timer);
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        timer = 0.0f;
        p = null;
    }
}
