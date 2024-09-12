using System.Collections;
using Lightbug.Utilities;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public enum Type { normal, elite, boss };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public GameObject player;
    public int nextMove;
    public float runSpeed;
    public float FrontDetectionRange;
    public float BackDetectionRange;
    public float intervalTime;
    private string curState;
    public float curTime = 0;
    public float coolTime;
    public float shootRange;
    protected float plusy;
    protected float count = 0;

    protected bool isPlayerDetected;
    protected bool isMoving;
    protected bool isAttack;
    protected bool isHit;

    public bool isDying;

    protected Rigidbody2D rigid;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;
    // protected Vector2 detectionCenter;
    protected Vector2 direction;
    protected GameObject playerObject;

    protected bool IsOnGround;




    protected virtual void DetectPlayer()
    {
        // 감지 영역 설정

        float detectionRadius = FrontDetectionRange; // 적당한 감지 반경 설정
        Vector2 detectionCenter = new Vector2(transform.position.x, transform.position.y + plusy);

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

        // isPlayerDetected = false; // 플레이어를 감지하지 못한 경우
    }

    // OverlapCircleAll의 기즈모 표시해야함.
    private void OnDrawGizmos()
    {
        Vector2 detectionCenter2 = new Vector2(transform.position.x, transform.position.y + plusy);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionCenter2, FrontDetectionRange);
        // 초록색 으로 총 범위 추가해야함.
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(detectionCenter2, shootRange);

    }

    // 대기 상태일 때 몬스터 이동 선택 로직
    protected virtual void Think()
    {
        nextMove = Random.Range(-1, 2);
        Invoke("Think", intervalTime);

    }


    protected virtual void MoveEnemy()
    {
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        Vector2 downVec = new(rigid.position.x - 0.5f * nextMove, rigid.position.y);
        Debug.DrawRay(downVec, Vector3.down * 0.5f, Color.blue); // Ray 길이를 0.5로 조정

        RaycastHit2D RayHit = Physics2D.Raycast(downVec, Vector3.down, 0.5f, LayerMask.GetMask("Wall", "Floor")); // Ray 길이를 0.5로 조정


        if (RayHit.collider == null)
        {
            IsOnGround = false;
            nextMove = -nextMove;
            Debug.Log("No Floor");
        }
        else
        {
            IsOnGround = true;
        }


        if (isPlayerDetected)
        {
            nextMove = 0;

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            // Debug.Log(distanceToPlayer);

            direction = new Vector2(player.transform.position.x - transform.position.x, 0).normalized;

            // Debug.Log(direction);
            transform.localScale = new Vector3(direction.x > 0 ? -1 : 1, 1, 1);


            if (distanceToPlayer > shootRange && distanceToPlayer < FrontDetectionRange)
            {
                transform.Translate(Vector2.zero);
                if (curTime <= 0 && !isAttack)
                {
                    AttackLogic();
                }
                else
                {
                    anim.SetBool("run", false);
                    anim.SetBool("walk", false);
                }

            }
            else if (distanceToPlayer > shootRange && !isAttack && !isHit)
            {

                RaycastHit2D RayHit2 = Physics2D.Raycast(transform.position, direction, 0.5f, LayerMask.GetMask("Wall", "Floor"));

                if (RayHit2.collider != null)
                {
                    transform.Translate(Vector2.zero);
                    anim.SetBool("run", false);
                    anim.SetBool("walk", false);
                }
                else
                {
                    TrackPlayer();
                }
            }
            else if (distanceToPlayer < shootRange && !isAttack && !isHit)
            {

                BelowShootRange();

            }

            if (curTime > 0)
            {
                curTime -= Time.deltaTime;
            }

            Debug.DrawRay(transform.position, -direction, new Color(0, 1, 0));
        }
        else
        {

            if (nextMove == 0)
            {
                anim.SetBool("run", false);
                anim.SetBool("walk", false);
            }
            else
            {
                anim.SetBool("run", false);
                anim.SetBool("walk", true);
            }

            Vector2 center = new Vector2(rigid.position.x, rigid.position.y + 1f);
            RaycastHit2D RayHit2 = Physics2D.Raycast(center, Vector3.left * nextMove, 1f, LayerMask.GetMask("Wall", "Floor"));
            Debug.DrawRay(center, Vector3.left * nextMove, new Color(0, 1, 0));

            if (RayHit2.collider != null)
            {
                nextMove = -nextMove;
            }
            {
                transform.Translate(Vector2.left * nextMove * runSpeed * 0.4f * Time.deltaTime);
                transform.localScale = new Vector2(nextMove > 0 ? 1 : -1, 1);
            }

        }

    }

    // 몬스터마다 해당 범위 로직 선언
    protected abstract void AttackLogic();
    protected abstract void TrackPlayer();
    protected abstract void BelowShootRange();

    protected virtual void OnAttack()
    {
        anim.SetTrigger("Attack");
        isAttack = true;
        curTime = coolTime;
        AttackComplete();
    }

    protected virtual void Skill1()
    {
        isAttack = true;
        curTime = coolTime;
        AttackComplete();
    }


    protected virtual void AttackComplete()
    {
        isAttack = false;
    }
    public void OnDamage(int attackDamage)
    {
        if (isDying) return;

        Debug.Log("Enemy Attacked " + attackDamage);
        curHealth -= attackDamage;

        if (curHealth <= 0)
        {
            curHealth = 0;
            OnDie();
        }
        else
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        anim.SetTrigger("Hit");
        rigid.velocity = Vector2.zero;
        anim.SetBool("run", false);
        anim.SetBool("walk", false);
        isHit = true;
        Invoke("HitComplete", 0.5f);
    }

    protected virtual void HitComplete()
    {
        isHit = false;
        if (isDying)
        {
            isDying = false;
        }
    }

    protected virtual void OnDie()
    {
        if (isDying) return;

        isDying = true;
        anim.SetBool("Die", true);
        Debug.Log("Enemy Die");
        isHit = false;
        anim.SetTrigger("DDie");
    }


    protected virtual void ChangeAnimation(string newState)
    {
        if (curState == newState)
        {
            return;
        }
        anim.Play(newState);
    }

    protected virtual IEnumerator CurTimeFunc()
    {
        while (curTime > 0)
        {
            curTime -= 1f; // 1초마다 1씩 감소
            yield return new WaitForSeconds(1f); // 1초 기다림
        }
    }


}