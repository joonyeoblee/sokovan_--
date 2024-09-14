using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : Enemy
{

    [Header("Enemy Attack")]
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject StunbulletPrefab;
    [SerializeField]
    private Transform bulletPos;
    private bool isStunGun = false;

    void Start()
    {
        // enemyType = Type.normal;
        Init();

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Invoke("Think", 0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isDying)
        {
            DetectPlayer();
            Move();
        }
    }

    protected override void AttackLogic()
    {
        if (count < 1)
        {
            if (player.GetComponent<PlayerMove>().isJump)
            {
                OnAirAttack();
            }
            else
            {
                OnAttack();
            }
        }
        else
        {
            if (player.GetComponent<PlayerMove>().isJump)
            {
                AirSkill1();
            }
            else
            {
                Skill1();
            }

        }
    }

    protected override void OnAttack()
    {
        base.OnAttack();
        count++;
    }

    void OnAirAttack()
    {
        anim.SetTrigger("AttackAerial");
        isAttack = true;
        curTime = coolTime;
        base.AttackComplete();
        count++;
    }

    protected override void Skill1()
    {
        base.Skill1();
        anim.SetTrigger("Stun");
        count = 0;
    }

    void AirSkill1()
    {
        anim.SetTrigger("StunAerial");
        isAttack = true;
        curTime = coolTime;
        base.AttackComplete();
        count = 0;
    }


    protected override void TrackPlayer()
    {
        if (!IsOnGround) return;

        transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), enemyData.runSpeed * Time.deltaTime);

        anim.SetBool("run", true);
        anim.SetBool("walk", false);
    }

    protected override void BelowShootRange()
    {
        RaycastHit2D RayHit2 = Physics2D.Raycast(transform.position, -direction, 0.5f, LayerMask.GetMask("Wall", "Floor"));

        if (RayHit2.collider != null)
        {
            transform.Translate(Vector2.zero);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), -enemyData.runSpeed * 0.5f * Time.deltaTime);
            anim.SetBool("run", true);
            anim.SetBool("walk", false);
        }

    }

    public void ShootStunBullet()
    {
        // bulletPos 에서 bulletPrefab을 생성 0.2f의 속도로 앞으로 발사 해야함.
        GameObject Stunbullet = Instantiate(StunbulletPrefab, bulletPos.position, bulletPos.rotation);
        // 만약 지금 객체의 Scale이 -1이면 bullet의 sprite fipx을 true로 해줘야함.
        if (transform.localScale.x < 0)
        {
            Stunbullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
        Stunbullet.GetComponent<Rigidbody2D>().velocity = Vector3.left * transform.localScale.x * 3f;
        Destroy(Stunbullet, 3f);
    }

    public void ShootBullet()
    {
        // bulletPos 에서 bulletPrefab을 생성 0.2f의 속도로 앞으로 발사 해야함.
        GameObject bullet = Instantiate(bulletPrefab, bulletPos.position, bulletPos.rotation);
        // 만약 지금 객체의 Scale이 -1이면 bullet의 sprite fipx을 true로 해줘야함.
        if (transform.localScale.x < 0)
        {
            bullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.left * transform.localScale.x * 3f;
        Destroy(bullet, 3f);
    }

    public void ShootAerialBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletPos.position, bulletPos.rotation);

        // 몬스터의 방향에 따라 총알을 flip
        if (transform.localScale.x < 0)
        {
            bullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
        else
        {
            bullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }

        Vector2 velocity = new Vector2(-transform.localScale.x * 3f, 3f);
        bullet.GetComponent<Rigidbody2D>().velocity = velocity;

        // 총알의 이동 방향에 따라 회전 각도를 계산하고 설정
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Destroy(bullet, 3f);
    }

    public void ShootAerialStunBullet()
    {
        GameObject Stunbullet = Instantiate(StunbulletPrefab, bulletPos.position, bulletPos.rotation);

        if (transform.localScale.x < 0)
        {
            Stunbullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
        else
        {
            Stunbullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }

        Vector2 velocity = new Vector2(-transform.localScale.x * 3f, 3f);
        Stunbullet.GetComponent<Rigidbody2D>().velocity = velocity;

        // 총알의 이동 방향에 따라 회전 각도를 계산하고 설정
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        Stunbullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Destroy(Stunbullet, 3f);
    }

}
