using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMove : Subject
{
    Rigidbody2D rigid;
    SpriteRenderer[] spriteRenderers;
    Animator anim;
    CapsuleCollider2D capsuleCollider;
    public HUDController _hudController;
    public PlayerAnimationController playerAnimationController;

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


    void Awake()
    {
        //이유 없이 뜨는 dontdestroyonload 없애기
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

        rigid = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        playerAnimationController = GetComponent<PlayerAnimationController>();

        // anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        attackStack = 0;
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
        if (!isIdleState()) return;

        float horiz = Input.GetAxis("Horizontal");
        if (horiz == 0 && !isAttack)
            rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;


        //일반 공격(Attack)
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (isStun) return;
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
            if (isStun) return;
            if (isIdleState())
            {
                isParry = true;
                isAttack = true;
                playerAnimationController.ChangeAnimation(playerAnimationController.PARRY);
                Invoke("AttackComplete", 0.4f);
            }
        }

        //점프(Jump)
        if (Input.GetKey(KeyCode.W) && isIdleState())
        {
            if (isStun) return;
            Jump();
            isJump = true;
        }

        //스프라이트 좌우 바꾸기(Sprite X reverse)
        if (Input.GetButton("Horizontal") && !isAttackState() && !isStun)
        {
            if (isStun) return;

            bool isLeft = Input.GetAxisRaw("Horizontal") == -1;
            transform.localScale = new Vector3(isLeft ? -1 : 1, 1, 1); //Flip X

        }

        //걷기 & 달리기 애니메이션(Walk & Run animatoration)
        if (Mathf.Abs(rigid.velocity.x) > 2)
        {
            playerAnimationController.ChangeAnimation(playerAnimationController.RUN);
        }
        else
        {
            playerAnimationController.ChangeAnimation(playerAnimationController.IDLE);
        }

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


            Vector2 downVec = new Vector2(rigid.position.x, rigid.position.y); ;
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

                        playerAnimationController.ChangeAnimation(playerAnimationController.AIRATK3);
                        Invoke("AttackComplete", 0.25f);
                    }

                }
            }
            else
            {
                if (isAirAttack)
                {
                    playerAnimationController.ChangeAnimation(playerAnimationController.AIRATK2);
                }
                else
                {
                    playerAnimationController.ChangeAnimation(playerAnimationController.JUMP_DOWN);
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
                Debug.Log("It's Door");
                break;
        }
    }

    //점프(Jump)
    private void Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jump_power);
        playerAnimationController.ChangeAnimation(playerAnimationController.JUMP_UP);
    }


    // 웅크리기
    private void CurlUp()
    {
        isCurlUp = true;
        StartCoroutine(AdjustColliderForCurlUpGradually());
        playerAnimationController.ChangeAnimation(playerAnimationController.CURLUP_DOWN);
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
        playerAnimationController.ChangeAnimation(playerAnimationController.IDLE);
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
                playerAnimationController.ChangeAnimation(playerAnimationController.ATTACK1);
                break;
            case 1:
                playerAnimationController.ChangeAnimation(playerAnimationController.ATTACK2);
                break;
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
                if (enemy.GetComponent<Enemy>().isDying == false)
                    enemy.GetComponent<Enemy>().OnDamage(attackDamage);
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
            Invoke("AttackComplete", shortDelay);
        else
            Invoke("AttackComplete", longDelay);
        attackStack++;
    }

    //공중 공격(Air Attack)
    private void AirAttack()
    {
        isAirAttack = true;
        rigid.velocity = new Vector2(0, 13);
        playerAnimationController.ChangeAnimation(playerAnimationController.AIRATK1);
        Invoke("AirFall", 0.3f);
        //Debug.Log("AirAttack");
    }

    //공중 공격을 실행하면 빠르게 낙하(Drop quickly when you run 'AirAttack')
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
        playerAnimationController.ChangeAnimation(playerAnimationController.DeadAni);
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
        return !(isJump || isAttack || isFall || isAirAttack || isDamaged || isCurlUp || isHitOn || isParry || isStun);
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
        playerAnimationController.ChangeAnimation(playerAnimationController.GETDAMAGE);
        isDamaged = true;
        StartCoroutine(DamagedComplete());
        curHealth -= damage; // damage 매개변수 추가해야함

        NotifyObservers();

        if (curHealth <= 0)
        {
            curHealth = 0;
            Dead();
        }
    }

    //피격 애니메이션 끝(Damaged animation complete)
    IEnumerator DamagedComplete()
    {

        playerAnimationController.ChangeAnimation(playerAnimationController.IDLE);
        yield return new WaitForSeconds(0.5f);
        isDamaged = false;

    }

    void SetActiveFalse()
    {
        gameObject.SetActive(false);
    }

}
