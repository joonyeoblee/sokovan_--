using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour, IChaseable
{
    [SerializeField]
    protected EnemyData enemyData;
    [SerializeField]
    private GameObject StunbulletPrefab;
    bool isPlayerDetected;

    public void DetectPlayer()
    {
        // 감지 영역 설정
        float detectionRadius = enemyData.FrontDetectionRange; // 적당한 감지 반경 설정
        Vector2 detectionCenter = new Vector2(transform.position.x, transform.position.y + enemyData.plusy);

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

    void FixedUpdate()
    {
        if (isPlayerDetected)
        {
            Attack();
        }
    }

    void Attack()
    {

    }


}
