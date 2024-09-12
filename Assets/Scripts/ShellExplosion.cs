using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    private bool hasBounced = false;

    public int damage = 3;
    public GameObject player;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.Find("Crimson");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 주인공 'Crimson'과 충돌했는지 확인합니다.
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Shell hit player");
            animator.SetTrigger("ExplodeDirect"); // 폭발 애니메이션을 재생합니다.

            // player의 OnDamage 함수를 호출합니다.
            player.GetComponent<PlayerMove>().Damaged(damage);

            return;
        }

        // 바닥과 충돌했는지 확인합니다.
        if (collision.gameObject.tag == "Wall" && !hasBounced)
        {
            hasBounced = true; // 첫 충돌에서만 튕김 효과를 적용합니다.

            float delay = Random.Range(0f, 1f); // 1초 이내에 랜덤한 시간 설정
            StartCoroutine(ExplodeAfterDelay(delay));
        }
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.SetTrigger("Explode"); // 폭발 애니메이션을 재생합니다.
        Debug.Log("Explosion Triggered");

    }

    public void DestroyObject()
    {
        Destroy(gameObject); // 폭발 후 객체를 제거합니다.
    }
}