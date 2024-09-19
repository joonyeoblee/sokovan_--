using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public enum EnemyType { normal, elite };

    public int maxHealth;
    public float runSpeed;
    public float FrontDetectionRange;
    public float intervalTime;

    public float coolTime;
    public float shootRange;
    public int damage;

    [Header("Enemy Attack")]
    public float plusy;
}
