using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//flag:국기를 달다, Accuracy:정확도
public class Part7_1Crosshair : MonoBehaviour
{
    //걷는상태 움크리는상태 모두 플레어어 스크립트에 있다
    [SerializeField]
    private Animator animator;

    //크로스헤어 상태에 따른 총의 정확도.
    private float gunAccuracy;

    //크로스헤어 비활성화를 위한 부모 객체
    [SerializeField]
    private GameObject go_CrosshairHUD;
    [SerializeField]
    private Part5_2_1GunController theGuncuntroller;
  

    public void WalkingAnimation(bool _flag)
    {
        Part8_1WeaponManager.currenWeaponAnim.SetBool("Walk", _flag);
        animator.SetBool("Walking", _flag);
    }

    public void RunningAnimation(bool _flag)
    {
        Part8_1WeaponManager.currenWeaponAnim.SetBool("Run", _flag);
        animator.SetBool("Running", _flag);
    }

    public void JumpingAnimation(bool _flag)
    {
        //Part8_1WeaponManager.currenWeaponAnim.SetBool("Run", _flag); 점프라 이 코드를 뺌
        animator.SetBool("Running", _flag);
    }

    public void CrouchingAnimation(bool _flag)
    {
        animator.SetBool("Crouching", _flag);
    }
    public void FineSightAnimation(bool _flag)
    {
        animator.SetBool("FineSight", _flag);
    }

    public void FireAnimation()
    {
        if (animator.GetBool("Walking"))//걷고 있는상태가 트루면 워크파이어 실행
            animator.SetTrigger("Walk_Fire");
        else if (animator.GetBool("Crouching"))
            animator.SetTrigger("Crouch_Fire");
        else
            animator.SetTrigger("Idle_Fire");
    }
    
            //정조준 피격시 정확도
    public float GetAccuracy()
    {
        if (animator.GetBool("Walking"))
            gunAccuracy = 0.06f;
        else if (animator.GetBool("Crouching"))
            gunAccuracy = 0.015f;
        //else if (theGuncuntroller.GetFineSightMode())
         //   gunAccuracy = 0.001f; //error 자꾸 나서 임시로 주석처리
        else
            gunAccuracy = 0.035f;

        //그리고 반환값으로
        return gunAccuracy;
        
            
    }
}
