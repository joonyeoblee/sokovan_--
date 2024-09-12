using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    NormalEnemy normalEnemy;

    void Start()
    {
        normalEnemy = GetComponentInParent<NormalEnemy>();
    }

    void ShootBulletAnimation()
    {
        normalEnemy.ShootBullet();
    }

    public void ShootStunBulletAnimation()
    {
        normalEnemy.ShootStunBullet();
    }

    public void ShootAerialBulletAnimation()
    {
        normalEnemy.ShootAerialBullet();
    }

    public void ShootAerialStunBulletAnimation()
    {
        normalEnemy.ShootAerialStunBullet();
    }

    void DestroyObject()
    {
        //물체의 부모를 찾아서 부모를 없애준다.
        Destroy(transform.parent.gameObject);
    }
}
