using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFinger : MonoBehaviour
{
    private Boss boss;

    private bool isAreaAttack;

    void Start()
    {
        boss = GetComponentInParent<Boss>();
    }

    void Shoot()
    {
        boss.CreateFingerBullet();
    }

    public void Dead()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("Dead");
        Debug.Log("Finger dead");
        Invoke(nameof(DeadEnd), 1f);
    }

    void DeadEnd()
    {
        gameObject.SetActive(false);
    }

}
