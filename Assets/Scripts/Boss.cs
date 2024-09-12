using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Boss Status")]
    public int maxHealth; // 3이 될 예정
    public int curHealth;
    public int damage;
    public int phase;

    [Header("Boss Attack")]
    public GameObject player;
    public GameObject meleeAttacker;
    public GameObject fingerAttacker;
    public GameObject fingerBullet;
    public Transform fingerBulletSpawnPoint;
    private Animator anim;

    public float curTime = 0;
    public float coolTime;
    public float updateInterval = 2f; // 업데이트 간격 (초)
    public int countForAreaAttack = 0;

    private bool isAttack;
    private bool isDead = false;
    private bool isInvincible = true; // 보스가 공격 중이 아닌 경우 무적 상태

    void Start()
    {
        player = GameObject.Find("Crimson");
        curHealth = maxHealth;
        anim = GetComponent<Animator>();
        InvokeRepeating(nameof(AttackMelee), 0f, updateInterval);
    }

    // Update is called once per frame
    void Update()
    {
        if (curTime <= 0 && isAttack && !isDead)
        {
            FingerAttack();
        }

        if (curTime > 0)
        {
            curTime -= Time.deltaTime;
        }

        if (countForAreaAttack == 3)
        {
            AreaAttack();
        }
    }

    void SetMeleeAttackerPosition()
    {
        Vector3 newPosition = meleeAttacker.transform.position;
        newPosition.x = player.transform.position.x;
        meleeAttacker.transform.position = newPosition;
    }

    void AttackMelee()
    {
        anim.SetTrigger("MeleeAttack");
        Debug.Log("Attack");
        isAttack = true;
        isInvincible = false; // 공격 시작 시 무적 상태 해제
        Invoke("EndAttack", 2f); // 공격이 끝난 후 상태 초기화
    }

    void FingerAttack()
    {
        fingerAttacker.SetActive(true);
        curTime = coolTime;
        countForAreaAttack++;
        isAttack = true;
        isInvincible = false; // 공격 시작 시 무적 상태 해제
        Invoke("FingerAttackEnd", 2f);
    }

    void FingerAttackEnd()
    {
        fingerAttacker.SetActive(false);
        EndAttack(); // 공격 종료 처리
    }

    public void CreateFingerBullet()
    {
        GameObject fingerBulletInstance = Instantiate(fingerBullet, fingerBulletSpawnPoint.position, fingerBulletSpawnPoint.rotation);
        fingerBulletInstance.GetComponent<Rigidbody2D>().velocity = Vector3.left * transform.localScale.x * 10f;

        Destroy(fingerBulletInstance, 3f);
    }

    void AreaAttack()
    {
        isInvincible = false; // 공격 시작 시 무적 상태 해제
        isAttack = true;
        anim.SetTrigger("AreaAttack");
        countForAreaAttack = 0;
        Invoke("AreaAttackEnd", 2f);
    }

    void AreaAttackEnd()
    {
        phase++;
        EndAttack(); // 공격 종료 처리
    }

    public void OnDamage()
    {
        Debug.Log(isInvincible && !isDead);
        if (isInvincible && !isDead) // 무적 상태가 아니고 죽지 않았을 때만 데미지 입음
        {
            curHealth--;
            if (curHealth == 0)
            {
                OnDead();
            }
            else
            {
                anim.SetTrigger("Damaged");
                isInvincible = true; // 데미지를 입었을 때 무적 상태 설정
                phase++;
            }
        }
    }

    void OnDead()
    {
        anim.SetBool("IsDead", true);
        isDead = true;
        // 'IsDead' 애니메이션이 종료된 후 오브젝트를 파괴하기 위해 Invoke 사용
        Invoke("DestroyBoss", anim.GetCurrentAnimatorStateInfo(0).length); // 현재 애니메이션의 길이를 얻어와 그 후에 파괴
    }

    void DestroyBoss()
    {
        gameObject.SetActive(false);
    }

    void EndAttack()
    {
        isAttack = false;
        isInvincible = true; // 공격 종료 시 무적 상태 설정
    }
}
