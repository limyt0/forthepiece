using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Part9_2AxeController : Part9_3CloseWeaponController
{
    //활성화 여부. //test위해서 false, start함수 이동
    public static bool isActivate = false;


        
    void Update()
    {
        if (isActivate)
            TryAttack();
    }
    protected override IEnumerator HitCoroutine()
    {
        while(isSwing)
        {
            if(CheckObject())
            {
                isSwing = false;
                Debug.Log(hitinfo.transform.name);
            }
            yield return null;
        }
    }

    //완성된 함수지만 불러와서 추가편집 가능
    public override void CloseWeaponChange(Part4_1CloseWeapon _CloseWeapon)
    {
        base.CloseWeaponChange(_CloseWeapon);//부모에 함수 호출
        isActivate = true;
    }
}
