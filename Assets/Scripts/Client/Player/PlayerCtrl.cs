using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerCtrl : MonoBehaviour
{
    [Header(" - Player Type")]
    public string nickname;
    public bool isMine = false;
    private int playerType;

    [Header(" - Player Movement")]
    public Vector2 pastPos;
    public Vector2 moveDir;
    public float moveSpeed = 5.0f;

    public Rigidbody2D rigid;
    //받은 정보를 처리하기 위함
    private Vector2 online_Dir;

    [Header(" - Load Finish")]
    public bool isLoaded;
    // Use this for initialization

    [Header(" - Skill Bool")]
    //Attack
    public bool canAttack;
    public bool isAttack;
    public bool isHit;

    //Skill
    public bool canSkill;
    public bool isSkill;

    [Header(" - Coin UI")]
    public int CurrentCoin;
    public Text coinUI;
    public Animator _anim;

    void Awake()
    {
        isLoaded = false;

        _anim = GetComponent<Animator>();
        coinUI = transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //UI Update
        coinUI.text = nickname + " / " + CurrentCoin;

        if (!isMine)
            return;

        switch(playerType)
        {
            case 0:
                break;
            case 1:
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isMine)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
            Attack();

        Move();
    }

    void LateUpdate()
    {
        Animation();

        if (!isMine)
            return;
    }

    //플레이어 이동
    void Move()
    {
        //공격시 이동 X -> 직선으로 날라가기 위함
        if (isAttack || isHit)
            return;

        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        rigid.velocity = moveDir.normalized * moveSpeed;
    }

    void Attack()
    {
        if (isAttack || moveDir == Vector2.zero || !canAttack || isHit)
            return;

        //Debug.Log("isPlayerAttack");
        rigid.AddForce(moveDir.normalized * 2800.0f);
        StartCoroutine(KeepAttack());
    }

    void Skill_1()
    {
        Debug.Log("Use Skill - Player Type 1");
    }

    void Skill_2()
    {
        Debug.Log("Use Skill - Player Type 2");
    }

    //에니메이션 실행
    void Animation()
    {
        _anim.SetFloat("MoveDir_X", rigid.velocity.x);
        _anim.SetFloat("MoveDir_Y", rigid.velocity.y);
    }
    
    IEnumerator KeepAttack()
    {
        canAttack = false;
        isAttack = true;
        yield return new WaitForSeconds(0.28f);
        isAttack = false;

        yield return new WaitForSeconds(1.0f);
        canAttack = true;
    }

    IEnumerator KeepHit()
    {
        isHit = true;
        yield return new WaitForSeconds(0.3f);
        isHit = false;

        rigid.velocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag.Equals("Player"))
        {
            PlayerCtrl p = coll.gameObject.GetComponent<PlayerCtrl>();

            if(p.isAttack)
            {
                //충돌하면 나를 움직임
                GameObject target = coll.gameObject;
                
                //방향 구하기
                Vector2 inNormal = Vector3.Normalize(transform.position - target.transform.position);

                //나를 이동
                rigid.AddForce(inNormal * 2800.0f);
                StartCoroutine(KeepHit());
            }   

            isAttack = false;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag.Equals("Coin"))
        {
            Destroy(coll.gameObject);
            CurrentCoin += Random.Range(100, 500);
        }
    }
}
