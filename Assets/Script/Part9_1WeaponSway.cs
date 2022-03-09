using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//sway:흔들다
public class Part9_1WeaponSway : MonoBehaviour
{
    //기존 위치
    private Vector3 originPos;

    //현재 위치
    private Vector3 currentPos;

    //sway 한계
    [SerializeField]
    private Vector3 limitPos;

    //정조준 sway 한계
    [SerializeField]
    private Vector3 fineSightLimitPos;

    //부드러운 움직임 정도
    [SerializeField]
    private Vector3 smoothSway;

    //필요한 컴포넌트
    [SerializeField]
    private Part5_2_1GunController theGunController;
    void Start()
    {
        originPos = this.transform.localPosition;
    }

   
    void Update()
    {
       // TrySway();
    }

    private void TrySway()
    {            //움직였을때
        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)        
            Swaying();
        
        else
            BackToOriginPos();       
    }
    
    private void Swaying()
    {
        float _moveX = Input.GetAxisRaw("Mouse X");
        float _moveY = Input.GetAxisRaw("Mouse Y");

        if (!theGunController.isFineSightMode)//test후 smoothSway.x와 y를 다르게 변경해 봤다(부드러움이 이상해서 변경함)
        {
            //마우스 벗어나는걸 가두기  마우스가 좌측으로 천천히 따라와야해서 -를 붙임, 부드러운정도,최소값, 최대값
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.x), -limitPos.x, limitPos.x),
                           Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveX, smoothSway.x), -limitPos.y, limitPos.y),
                           originPos.z); //z값은 0이 되면 조금 위험할수도 있으니
            
        }
        else
        {
            //마우스 벗어나는걸 가두기  마우스가 좌측으로 천천히 따라와야해서 -를 붙임, 부드러운정도,최소값, 최대값
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.y), -fineSightLimitPos.x, fineSightLimitPos.x),
                           Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveX, smoothSway.y), -fineSightLimitPos.y, fineSightLimitPos.y),
                           originPos.z); //z값은 0이 되면 조금 위험할수도 있으니
        }

        //조건문 빠져나오고 실제 적용되고
        transform.localPosition = currentPos;
    }
    private void BackToOriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}
