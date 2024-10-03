using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class Turret : MonoBehaviour, IChaseable
{
    [SerializeField]
    protected EnemyData enemyData;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform shootPoint;

    GameObject player;
    bool isPlayerDetected;
    bool isAttacking; // 공격 중인지 여부를 확인하기 위한 플래그

    Vector2 direction;

    void Init()
    {
        player = GameManager.instance.player;
    }

    void Start()
    {
        Init();
    }

    public void DetectPlayer()
    {
        // 감지 영역 설정
        float detectionRadius = enemyData.shootRange; // 적당한 감지 반경 설정
        Vector2 detectionCenter = new Vector2(transform.position.x, transform.position.y + enemyData.plusy);

        isPlayerDetected = false;

        // 감지 영역 내의 모든 콜라이더 검사
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(detectionCenter, detectionRadius, LayerMask.GetMask("Player"));
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != null && hitCollider.gameObject.tag == "Player")
            {
                isPlayerDetected = true;
                return; // 플레이어를 감지하면 반복 중지
            }
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, enemyData.FrontDetectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enemyData.shootRange);
    }

    void FixedUpdate()
    {
        DetectPlayer();

        if (isPlayerDetected && !isAttacking) // 공격 중이 아닐 때만 공격 시작
        {
            Flip();
            Attack().Forget();
        }
    }

    void Flip()
    {
        direction = new Vector2(player.transform.position.x - transform.position.x, 0).normalized;
        transform.localScale = new Vector3(direction.x > 0 ? -1 : 1, 1, 1);
    }

    private async UniTaskVoid Attack()
    {
        isAttacking = true; // 공격 중 상태로 변경

        // 공격 시작 전에 플레이어가 여전히 감지 범위 안에 있는지 확인
        if (!isPlayerDetected)
        {
            isAttacking = false; // 플레이어가 범위에서 벗어났다면 공격 취소
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);

        if (transform.localScale.x < 0)
        {
            bullet.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }

        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.left * transform.localScale.x * 3f;

        Destroy(bullet, 3f);

        await UniTask.Delay(TimeSpan.FromSeconds(enemyData.coolTime)); // 쿨타임 추가 (추가 공격 대기 시간)
        isAttacking = false; // 공격 끝나면 다시 공격 가능
    }
}
