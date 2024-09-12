using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteEnemy : Enemy
{
    [Header("Enemy Attack")]
    public GameObject shellPrefab;
    public Transform shellPos;
    public float plusyValue;
    void Start()
    {
        enemyType = Type.elite;
        plusy = plusyValue;

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Invoke("Think", 0f);
    }

    void FixedUpdate()
    {
        if (!isDying)
        {
            DetectPlayer();
            MoveEnemy();
        }
    }
    protected override void AttackLogic()
    {
        Skill1();
    }

    protected override void OnAttack()
    {
        base.OnAttack();

    }

    protected override void Skill1()
    {
        base.Skill1();
        anim.SetTrigger("Gun");
    }

    protected override void TrackPlayer()
    {

        if (!IsOnGround) return;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), runSpeed * Time.deltaTime);

        anim.SetBool("run", true);
        anim.SetBool("walk", false);

    }

    protected override void BelowShootRange()
    {

        if (curTime <= 0)
            OnAttack();

    }

    public void ShootShell()
    {
        GameObject shell = Instantiate(shellPrefab, shellPos.position, shellPos.rotation);
        shell.GetComponent<Rigidbody2D>().velocity = Vector3.left * transform.localScale.x * 3f;
    }
}
