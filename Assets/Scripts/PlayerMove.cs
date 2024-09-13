using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMove : Subject
{
    Rigidbody2D rigid;
    SpriteRenderer[] spriteRenderers;
    Animator anim;
    CapsuleCollider2D capsuleCollider;
    [SerializeField]
    private HUDController _hudController;

    public int jump_power = 23;
    public int curlup_power = -10;

    public int maxHealth = 10;
    public int curHealth;

    [Header("Attack damage")]
    public Transform attackPoint;
    public LayerMask enemies;
    public float attackRange = 0.5f;
    public float shortDelay;
    public float longDelay;
    public int attackDamage;
    public float timerSet = 1.0f;
    public float resetTimer = 0.0f;

    [SerializeField] private int speed;

    [Header("Bullet")]
    public GameObject bossFingerBullet;




    private string curState;
    private int attackStack;
    private int runSpeed;
    private int hitCount;
    private bool isFall;
    public bool isJump;
    private bool isAttack;
    private bool isAirAttack;
    private bool isDamaged;
    private bool isCurlUp;
    private bool isHitOn;
    private bool isStun;

    private bool isParry;

    const string IDLE = "NewCrimson_Idle";
    const string WALK = "NewCrimson_Walk";
    const string RUN = "NewCrimson_Walk";
    const string JUMP_UP = "NewCrimson_Jump_Up";
    const string JUMP_DOWN = "NewCrimson_Jump_Down";
    const string CURLUP_UP = "NewCrimson_Jump_Up";
    const string CURLUP_DOWN = "NewCrimson_Jump_Down";
    const string ATTACK1 = "NewCrimson_Attack1";
    const string ATTACK2 = "NewCrimson_Attack2";
    const string ATTACK3 = "NewCrimson_Attack3";
    const string ATTACK4 = "NewCrimson_Attack4";
    const string AIRATK1 = "NewCrimson_AirAttack1";
    const string AIRATK2 = "NewCrimson_AirAttack2";
    const string AIRATK3 = "NewCrimson_AirAttack3";
    const string Parry = "NewCrimson_Parrying";
    const string GETDAMAGE = "NewCrimson_Damaged";
    const string DeadAni = "NewCrimson_Dead";


    void Awake()
    {
        //이유 없이 뜨는 dontdestroyonload 없애기
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

        rigid = GetComponent<Rigidbody2D>();
        // 자식에서 spriteRenderer를 가져오기 위해 GetComponentInChildren 사용해야함.
        // spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 자식에서 spriteRenderer를 가져오기 위해 GetComponentsInChildren 사용해야함.
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        attackStack = 0;
        //walkSpeed = 3;
        runSpeed = 6;

        curHealth = maxHealth;

        isJump = false;
        isAttack = false;
        isAirAttack = false;
        isCurlUp = false;
        isParry = false;
        isHitOn = false;
        isStun = false;

    }

    void OnEnable()
    {
        if (_hudController)
            Attach(_hudController);

        NotifyObservers();
    }

    void OnDisable()
    {
        if (_hudController)
            Detach(_hudController);
    }

    void Update()
    {


        float horiz = Input.GetAxis("Horizontal");
        if (horiz == 0 && !isAttack)
            rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;


        //일반 공격(Attack)
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (isJump == true && !isAttackState())
                AirAttack();
            else if (isIdleState())
            {
                if (attackStack == 2) //4 에서 2로 수정 123 줄도 마찬가지 
                    attackStack %= 2;
                isAttack = true;
                Attack();
            }
        }

        //1번 스킬(Skill1)
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (isIdleState())
            {
                isParry = true;
                isAttack = true;
                ChangeAnimation(Parry);
                // Parrying();
                Invoke("AttackComplete", 0.4f);
            }
        }

        //점프(Jump)
        if (Input.GetKey(KeyCode.W) && isIdleState())
        {
            Jump();
            isJump = true;
        }

        //스프라이트 좌우 바꾸기(Sprite X reverse)
        if (Input.GetButton("Horizontal") && !isAttackState() && !isStun)
        {
            bool isLeft = Input.GetAxisRaw("Horizontal") == -1;
            // foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            // {
            //     spriteRenderer.flipX = isLeft;
            // }

            // // attackPoint 위치 변경
            // attackPoint.localPosition = new Vector3(isLeft ? -1f : 1f, attackPoint.localPosition.y, attackPoint.localPosition.z);
            // Debug.Log(isLeft);
            // Debug.Log(attackPoint.localPosition);
            transform.localScale = new Vector3(isLeft ? -1 : 1, 1, 1); //Flip X

        }

        if (isIdleState())
        {
            //걷기 & 달리기 애니메이션(Walk & Run animatoration)
            if (Mathf.Abs(rigid.velocity.x) > 2)
            {
                ChangeAnimation(RUN);
            }
            /*
            else if(Mathf.Abs(rigid.velocity.x) > 2) {
                ChangeAnimation(WALK);
            }
            */
            else
            {
                ChangeAnimation(IDLE);
            }
        }

        // // 웅크리기
        // if (Input.GetKey(KeyCode.S) && isIdleState())
        // {
        //     isCurlUp = true;
        //     CurlUp();
        // }
    }

    void FixedUpdate()
    {
        //걷기 & 달리기(Walk & Run)
        RaycastHit2D hit = Physics2D.Raycast(rigid.position, Vector2.down, 2, LayerMask.GetMask("Floor"));
        bool onGround = hit.collider != null;

        // 캐릭터가 공중에 떠있으면 처리
        if (!onGround)
        {

        }

        if (!isAttackState())
        {
            rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * runSpeed, rigid.velocity.y);
        }



        //바닥에 닿았는지 확인(Check wether character is on the floor)
        if (rigid.velocity.y < 0)
        {
            isFall = true;


            Vector2 downVec = new Vector2(rigid.position.x, rigid.position.y);
            /*
            Debug.DrawRay(downVec, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D RayHit = Physics2D.Raycast(downVec, Vector3.down, 1, LayerMask.GetMask("Floor"));
            */
            //Debug.DrawRay(downVec, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D RayHit = Physics2D.BoxCast(downVec, capsuleCollider.bounds.size, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Floor"));
            Debug.DrawRay(downVec, Vector3.down * 0.1f, new Color(1, 0, 0));

            if (RayHit.collider != null)
            {
                if (RayHit.distance < 0.1f)
                {
                    isJump = false;
                    isFall = false;
                    if (isAirAttack == true)
                    {

                        ChangeAnimation(AIRATK3);
                        //rigid.velocity = new Vector2(rigid.velocity.x, -5);
                        Invoke("AttackComplete", 0.25f);
                    }

                    //isAirAttack = false;
                }
            }
            else
            {
                if (isAirAttack)
                {
                    ChangeAnimation(AIRATK2);
                }
                else
                {
                    ChangeAnimation(JUMP_DOWN);
                }
            }
        }
        if (Mathf.Abs(rigid.velocity.x) >= 2.5f || Mathf.Abs(rigid.velocity.y) != 0)
            attackStack = 0;

        if (isHitOn && Time.time > resetTimer)
        {
            resetTimer = Time.time + timerSet;
            hitCount = 0;
            isHitOn = false;
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "TrapDoor")
        {
            // if (transform.position.y > other.transform.position.y && isAirAttack == true)
            //     OnDestroyObj("TrapDoor", other.transform);
            Debug.Log("TrapDoor");
            GameManager trapDoor = other.GetComponent<GameManager>();
            trapDoor.LoadBossScene();
        }
        if (other.gameObject.tag == "Bullet")
        {
            if (isParry)
            {
                Parrying();
            }
            else
            {
                Damaged(1); // 총알에서 호출되어야 함 1은 임시로 정한 값
                Debug.Log("Bullet Hit");
            }
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Stun")
        {
            isStun = true;
            runSpeed = 0;
            Destroy(other.gameObject);

            //2초 후에 스턴 해제
            Invoke("StunComplete", 2f);
        }
    }

    void StunComplete()
    {
        isStun = false;
        runSpeed = 6;
    }


    //특수 물체 부수기(Destroy Speical Object)
    void OnDestroyObj(string DestroyType, Transform Obj)
    {
        switch (DestroyType)
        {
            case "TrapDoor":
                ObjectCtrl objectCtrl = Obj.GetComponent<ObjectCtrl>();
                objectCtrl.TrapDoorDestroy();
                break;
            case "Door":
                Debug.Log("hello");
                break;
        }
    }

    //점프(Jump)
    private void Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jump_power);
        ChangeAnimation(JUMP_UP);
    }


    // 웅크리기
    private void CurlUp()
    {
        isCurlUp = true;
        StartCoroutine(AdjustColliderForCurlUpGradually());
        ChangeAnimation(CURLUP_DOWN);
    }

    IEnumerator AdjustColliderForCurlUpGradually()
    {
        // 기존 콜라이더 사이즈 및 오프셋 저장
        Vector2 originalSize = capsuleCollider.size;
        Vector2 originalOffset = capsuleCollider.offset;

        // 새로운 사이즈 및 오프셋 설정
        Vector2 newSize = new Vector2(capsuleCollider.size.x, 0.5f);
        Vector2 newOffset = new Vector2(capsuleCollider.offset.x, -0.7f);

        float duration = 0.5f; // 변경에 걸리는 시간
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fraction = elapsed / duration;

            capsuleCollider.size = Vector2.Lerp(originalSize, newSize, fraction);
            capsuleCollider.offset = Vector2.Lerp(originalOffset, newOffset, fraction);

            yield return null;
        }

        // 최종 사이즈 및 오프셋 적용
        capsuleCollider.size = newSize;
        capsuleCollider.offset = newOffset;

        CurlUpComplete();
    }

    // 웅크리기 상태를 해제하고 콜라이더를 원래 상태로 되돌림
    private void CurlUpComplete()
    {
        isCurlUp = false;
        ResetCollider();
        ChangeAnimation(IDLE);
    }

    // 원래 콜라이더 사이즈 및 오프셋으로 되돌림
    void ResetCollider()
    {
        capsuleCollider.size = new Vector2(0.9f, 2f);
        capsuleCollider.offset = new Vector2(0f, -0.5f);
    }



    //공격(Attack)
    private void Attack()
    {
        switch (attackStack)
        {
            case 0:
                ChangeAnimation(ATTACK1);
                break;
            case 1:
                ChangeAnimation(ATTACK2);
                break;
                // case 2:
                //     ChangeAnimation(ATTACK1);
                //     break;
                // case 3:
                //     ChangeAnimation(ATTACK4);
                //     break;
        }
        //공격하면 공격하는 방향으로 조금씩 이동(If you attack, move little by little in the direction of attack)
        if (transform.localScale.x == -1)
            rigid.velocity = new Vector2(-2, rigid.velocity.y);
        else
            rigid.velocity = new Vector2(2, rigid.velocity.y);

        Collider2D[] damagedEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemies);

        foreach (Collider2D enemy in damagedEnemies)
        {
            Debug.Log(enemy.tag);
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("We Hit " + enemy.name);

                //만약 enemy에 Enemy 스크립트가 있으면 공격
                if (enemy.GetComponent<Enemy>() != null)
                {
                    if (enemy.GetComponent<Enemy>().isDying == false)
                        enemy.GetComponent<Enemy>().OnDamage(attackDamage);
                }
            }

            if (enemy.CompareTag("Boss"))
            {
                Debug.Log("Hit Boss");
                enemy.GetComponent<Boss>().OnDamage();
            }


            if (enemy.CompareTag("Finger"))
            {
                enemy.GetComponent<BossFinger>().Dead();
            }
        }

        if (attackStack == 1)
            //Invoke("AttackComplete", 0.238f);
            //Invoke("AttackComplete", 0.22f);
            Invoke("AttackComplete", shortDelay);
        else
            //Invoke("AttackComplete", 0.357f);
            //Invoke("AttackComplete", 0.34f);
            Invoke("AttackComplete", longDelay);
        attackStack++;
    }

    //공중 공격(Air Attack)
    // private void AirAttack()
    // {
    //     isAirAttack = true;
    //     rigid.velocity = new Vector2(0, 13);
    //     ChangeAnimation(AIRATK1);
    //     Invoke("AirFall", 0.3f);
    //     //Debug.Log("AirAttack");
    //     Collider2D[] damagedEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemies);
    //     foreach (Collider2D enemy in damagedEnemies)
    //     {
    //         if (enemy.CompareTag("Enemy"))
    //         {

    //             Debug.Log("We use air attack to Hit " + enemy.name);
    //             if (enemy.GetComponent<Enemy>() != null)
    //             {
    //                 if (enemy.GetComponent<Enemy>().isDying == false)
    //                     enemy.GetComponent<Enemy>().OnDamage(attackDamage);
    //             }
    //         }

    //         if (enemy.CompareTag("Boss"))
    //         {
    //             Debug.Log("Hit Boss");
    //             enemy.GetComponent<Boss>().OnDamage();
    //         }

    //     }
    // }

    // //공중 공격을 실행하면 빠르게 낙하(Drop quickly when you run 'AirAttack')
    // void AirFall()
    // {
    //     rigid.velocity = new Vector2(rigid.velocity.x, -20);

    // }
    private void AirAttack()
    {
        isAirAttack = true;
        rigid.velocity = new Vector2(0, 13);
        ChangeAnimation(AIRATK1);
        Invoke("AirFall", 0.3f);
        //Debug.Log("AirAttack");
        int isAirHit = 0;

        while (isAirHit == 0)
        {
            isAirHit = CheckForHit();
        }

    }


    private int CheckForHit()
    {
        Collider2D[] damagedEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemies);
        // 콜라이더에 enter를 감지
        foreach (Collider2D enemy in damagedEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("We use air attack to Hit " + enemy.name);
                if (enemy.GetComponent<Enemy>() != null)
                {
                    if (enemy.GetComponent<Enemy>().isDying == false)
                        enemy.GetComponent<Enemy>().OnDamage(attackDamage);

                    return 2;
                }
            }
        }

        return -1;
    }

    // 공중 공격을 실행하면 빠르게 낙하(Drop quickly when you run 'AirAttack')
    void AirFall()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, -20);
    }



    //스킬1 : 혈포
    void Skill1_BloodCannon()
    {

    }

    //스킬2 : 전기충격
    void Skill2_Electric()
    {

    }

    //스킬3 : 전기톱 돌진
    void Skill3_ChainSaw()
    {

    }

    void Dead()
    {
        ChangeAnimation(DeadAni);
        Destroy(gameObject, 1.5f);
    }

    //공격 끝(Attack Complete)
    private void AttackComplete()
    {
        isAttack = false;
        isAirAttack = false;
    }

    //가만히 있는 상태인지 체크(Check wether state is 'idle')
    public bool isIdleState()
    {
        return !(isJump || isAttack || isFall || isAirAttack || isDamaged || isCurlUp);
    }


    //공격 상태인지 체크(Check wether state is 'attack')
    public bool isAttackState()
    {
        return isAttack || isAirAttack;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    //움직임에 따라 애니메이션 전환(Change animation based on current state)
    void ChangeAnimation(string newState)
    {
        if (curState == newState)
        {
            return;
        }
        anim.Play(newState);
    }


    public void Parrying()
    {
        GameObject fingerBulletInstance = Instantiate(bossFingerBullet, attackPoint.position, attackPoint.rotation);
        fingerBulletInstance.GetComponent<Rigidbody2D>().velocity = Vector3.right * transform.localScale.x * 10f;
        Destroy(fingerBulletInstance, 3f);
        Invoke("DisableParry", 0.5f);
        Debug.Log("Parrying");
    }

    private void DisableParry()
    {
        isParry = false;
    }

    //피격 애니메이션(Damaged animation)
    public void Damaged(int damage) // 매개변수 추가해야함
    {
        // curHealth -= damage;
        ChangeAnimation(GETDAMAGE);
        isDamaged = true;
        Invoke("DamagedComplete", 0.5f);
        curHealth -= damage; // damage 매개변수 추가해야함

        NotifyObservers();

        if (curHealth <= 0)
        {
            curHealth = 0;
            Dead();
        }

    }



    //피격 애니메이션 끝(Damaged animation complete)
    void DamagedComplete()
    {
        ChangeAnimation(IDLE);
        isDamaged = false;

    }


}
