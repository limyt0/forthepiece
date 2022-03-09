using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Part4_1CloseWeapon : MonoBehaviour
{
    
    public string CloseWeaponName;//핸드 이름 // 너클이나 맨손을 구분

    //웨폰 유형.
    
    public bool isHand;
    public bool isAxe;
    public bool isPickaxe;

    public float range;//공격 범위
    public int damage;//공격력
    public float workSpeed;//작업 속도
    public float attackDelay; //공격 딜레이
    public float attackDelayA;//공격 활성화 시점.
    public float attackDelayB;//공격 비활성화 시점.
        
    public Animator anim;//만들었던 에니메이터를 넣어줄것이다
    //public BoxCollider boxCollider;//여기서는 1인칭시점이라 휘드를때 다르게 보일수 있으므로 십자가 모양으로 할것이기 때문에 쓰지않을것이다
}
