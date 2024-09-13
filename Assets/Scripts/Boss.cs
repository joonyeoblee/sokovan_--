using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Boss Status")]
    [SerializeField]
    private int maxHealth; // 3이 될 예정
    private int curHealth;
    [SerializeField]
    public int damage;
    private int phase;

    [Header("Boss Attack")]
    private GameObject player;
    private GameObject[] Attackers;
    [SerializeField]
    private GameObject meleeAttacker;
    [SerializeField]
    private GameObject fingerAttacker;
    public GameObject fingerBullet;
    [SerializeField]
    private Transform fingerBulletSpawnPoint;

    private Animator anim;

    public float curTime = 0;
    public float coolTime;
    public float updateInterval = 2f; // 업데이트 간격 (초)
    public int countForAreaAttack = 0;

    private bool isAttack;
    private bool isDead = false;
    private bool isInvincible = true; // 보스가 공격 중이 아닌 경우 무적 상태
    private bool isRepeating = true;

    void Awake()
    {
        player = GameObject.Find("Crimson");
        anim = GetComponent<Animator>();

        Init();

        StartRepeatingTask(AttackMelee, 0f, updateInterval).Forget();
    }

    void Init()
    {
        curHealth = maxHealth;

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

    public async UniTaskVoid StartRepeatingTask(System.Action method, float initialDelay, float repeatInterval)
    {
        isRepeating = true;

        // 처음 한 번의 지연
        await UniTask.Delay(TimeSpan.FromSeconds(initialDelay));

        // 반복적으로 호출
        while (isRepeating)
        {
            method?.Invoke();  // 작업 실행
            await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval));  // 반복 지연
        }
    }

    // 반복 작업을 중지하는 함수
    public void StopRepeatingTask()
    {
        isRepeating = false;
    }

    void AttackMelee()
    {
        anim.SetTrigger("MeleeAttack");
        Debug.Log("Attack");
        isAttack = true;
        isInvincible = false; // 공격 시작 시 무적 상태 해제
        // Invoke("EndAttack", 2f); // 공격이 끝난 후 상태 초기화
        EndAttackVoid().Forget();
    }

    void FingerAttack()
    {
        fingerAttacker.SetActive(true);
        curTime = coolTime;
        countForAreaAttack++;
        isAttack = true;
        isInvincible = false; // 공격 시작 시 무적 상태 해제
        FingerAttackEndVoid().Forget();
    }

    private async UniTaskVoid FingerAttackEndVoid()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        fingerAttacker.SetActive(false);
        EndAttack();
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
        // Invoke("AreaAttackEnd", 2f);
        AreaAttackEndVoid().Forget();
    }

    private async UniTaskVoid AreaAttackEndVoid()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
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
        StopRepeatingTask();
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

    private async UniTaskVoid EndAttackVoid()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        isAttack = false;
        isInvincible = true; // 공격 종료 시 무적 상태 설정
    }
}
