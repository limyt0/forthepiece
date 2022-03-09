using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;



public class Part4_2HandController : Part9_3CloseWeaponController
{
    //활성화 여부.
    public static bool isActivate = false;

    void Update()
    {
        if (isActivate)
            TryAttack();
    }

    protected override IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
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
