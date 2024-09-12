using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteShootShell : MonoBehaviour
{
    private EliteEnemy eliteEnemy;
    void Start()
    {
        eliteEnemy = GetComponentInParent<EliteEnemy>();
    }

    void ShootShell()
    {
        eliteEnemy.ShootShell();
    }
    void DestroyObject()
    {
        //물체의 부모를 찾아서 부모를 없애준다.
        Destroy(transform.parent.gameObject);
    }


}