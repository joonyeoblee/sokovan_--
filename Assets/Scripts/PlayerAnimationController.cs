using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator anim;
    public string IDLE = "NewCrimson_Idle";
    public string WALK = "NewCrimson_Walk";
    public string RUN = "NewCrimson_Walk";
    public string JUMP_UP = "NewCrimson_Jump_Up";
    public string JUMP_DOWN = "NewCrimson_Jump_Down";
    public string CURLUP_UP = "NewCrimson_Jump_Up";
    public string CURLUP_DOWN = "NewCrimson_Jump_Down";
    public string ATTACK1 = "NewCrimson_Attack1";
    public string ATTACK2 = "NewCrimson_Attack2";
    public string ATTACK3 = "NewCrimson_Attack3";
    public string ATTACK4 = "NewCrimson_Attack4";
    public string AIRATK1 = "NewCrimson_AirAttack1";
    public string AIRATK2 = "NewCrimson_AirAttack2";
    public string AIRATK3 = "NewCrimson_AirAttack3";
    public string PARRY = "NewCrimson_Parrying";
    public string GETDAMAGE = "NewCrimson_Damaged";
    public string DeadAni = "NewCrimson_Dead";

    private string curState;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void ChangeAnimation(string newState)
    {
        if (curState == newState)
        {
            return;
        }
        anim.Play(newState);
        curState = newState;
    }
}
