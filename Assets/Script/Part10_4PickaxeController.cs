using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Part10_4PickaxeController : Part9_3CloseWeaponController
{
    //활성화 여부.
    public static bool isActivate = true;//test위해 스크립트 이동하면서 시작하는 무기에 추가

    private void Start()//test위해 스크립트 이동하면서 시작하는 무기에 추가
    {
        Part8_1WeaponManager.currentWeapon = CurrentCloseWeapon.GetComponent<Transform>();
        Part8_1WeaponManager.currenWeaponAnim = CurrentCloseWeapon.anim;
    }
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
