using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : MonoBehaviour
{
    private RectTransform recttr;

    [Header("-=Smooth Motion=-")]
    public bool isUseSmoothMotion;      //부드러운 이동모션을 사용할 것인지
    public Transform sFirstPos;
    public Transform sLaterPos;
    public float moveSpeed;             //이동 속도
    public bool sGoLate;                //시작은 FirstPos에서, 이 값이 참이면 Later로 거짓이면 First로 

    void Awake()
    {
        recttr = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //부드러운 이동 모션
        if (isUseSmoothMotion)
            recttr.position = sFirstPos.position;

        if (sLaterPos == null)
            sLaterPos = recttr;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isUseSmoothMotion)
        {
            SmoothMotion();
        }
    }

    void SmoothMotion()
    {
        //이동 모션
        if (sGoLate)
        {
            recttr.position = Vector3.Lerp(recttr.position, sLaterPos.position, moveSpeed * Time.smoothDeltaTime);
        }
        else
        {
            recttr.position = Vector3.Lerp(recttr.position, sFirstPos.position, moveSpeed * Time.smoothDeltaTime);
        }
    }

    //부드러운 이동 모션 활성화/비활성화
    public void SmoothGoFirst()
    {
        sGoLate = false;
    }
    public void SmoothGoLater()
    {
        sGoLate = true;
    }
}