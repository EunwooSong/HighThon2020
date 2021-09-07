using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    Transform tr;
    public Transform target;
    public float zOffset;
    public float movespeed;

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
        zOffset = tr.position.z;
        //타겟을 찾아야함 / 플레이어가 생성되면 게임 메니저가 플레이어의 정보를 파악해서 타겟 설정
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null)
            return;

        tr.position = Vector3.Lerp(tr.position, target.position + Vector3.forward * zOffset, movespeed * Time.deltaTime);
    }
}
