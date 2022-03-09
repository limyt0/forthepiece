using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Part9_3CloseWeaponController : MonoBehaviour
{
    
    //현재 장작된 Hand형 타입 무기.
    [SerializeField]
    protected Part4_1CloseWeapon CurrentCloseWeapon;

    //상태변수를 만들것이다
    //공격중??
    protected bool isAttack = false;//기본값 false이고 true면 공격하지 못하게 할것이다
    protected bool isSwing = false;//칼을 휘두르는지 아닌지 이걸로 체크

    protected RaycastHit hitinfo;//레이저에 닿은녀석이 hitinfo에 담기되 된다(닿은녀석을 스크립트를 불러와서 체력을 깍거나 어떤것을 하기위해)

    NormalZombie_AI norzom_ai;//11.12
    protected void TryAttack()
    {
        //설명:마우스 왼클릭하면 코루틴이 실행되고 true가 되면서 중복실행이 많어지게된다
        //마우스 누르고 있어도 작동되기위해 GetButton을 쓴다
        //이전 코드
        //if (Input.GetButton("Fire1"))//Fire1은 유니티에서 총알발사(project에 Input Manager Axes=>Fire1에 Positive Button에 left Ctrl(mouse 0과 동일)을 없애준다(움크리기와 중복되서)
        if( OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) ) //11.12 공격키 바꿈.
        {
            if (!isAttack)
            {
                //코루틴 실행.
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;//중복실행 true해서 막음
        CurrentCloseWeapon.anim.SetTrigger("Attack");//animator에 만든 Attack발동, 공격에니메이션 실행

        yield return new WaitForSeconds(CurrentCloseWeapon.attackDelayA);//CurrentHand.attackDelayA만큼 딜레이(약간의 딜레이 후에)
        isSwing = true; //팔을 뻣었으니 이때부터 공격이 들어가게

        //적중여부 판단할 코루틴실행 (isSwing true일때)
        StartCoroutine(HitCoroutine());//아래 코루틴에 while를 써서 계속 반복실행한다

        //또 대기
        yield return new WaitForSeconds(CurrentCloseWeapon.attackDelayB);
        isSwing = false;//이때부터 팔을 접었다


        yield return new WaitForSeconds(CurrentCloseWeapon.attackDelay - CurrentCloseWeapon.attackDelayA - CurrentCloseWeapon.attackDelayB);
        isAttack = false; //잠시후가 지나면 false가되서  isAttack을(마우스 왼클릭) 다시 실행시킬수 있게 준비
    }

    //미완성으로 자식이 완료 = 추상 코루틴.
    protected abstract IEnumerator HitCoroutine(); 
    

    protected bool CheckObject()//위에 if(CheckObject())라는 조건문이라서 bool로 함
    {
        //전방에 뭐가 있다면 true를 반환시킨다
        //(내자신,  내앞으로, hitinfo에 저장, 공격범위)
        if (Physics.Raycast(transform.position, transform.forward, out hitinfo, CurrentCloseWeapon.range))
        {
            //11.12 적 공격 판정
            if (hitinfo.transform.tag == "Zombie")
            {
                norzom_ai = hitinfo.transform.GetComponent<NormalZombie_AI>();
                norzom_ai.attacked(CurrentCloseWeapon.damage); //주인공 공격력만큼 공격값넣기
                
            }
            return true; //충돌 있다
        }

        return false;//충돌 없다
    }
    //팔이 안따라 가는건 MainCamera안에다 넣어준다
    //팔 사라지고 검은색 보이는거는 카메라 복사후 하위 삭제=> Weapon Camera이름 바꾸고, Depth only, Near0.01, 
    //Depth(순위) 바꾸후 Main Camera 아래에 복사 Layer 만들고 Culling Mast활용해서 팔만 보이게 하고 Main Camera는 팔빼고 나머지 보이게 한다
    //오디오 에러 나는것은 Weapon Camera에 Audio Listener 삭제하면 된다(중복되서)
    //충돌처리 => 빈객체 만들고 초기화 Holder이름변경해서 y축 로테이트를 0으로 만들어줘야한다 HandHolder 스크립트를 Holder로
    //Player에 Layer를 Ignore RayCast(해당객체는 RayCast영향 받지않겠다)해준다, Hand는 다시 Weapon으로

    // 완성 함수이지만, 추가 편집한 함수.
    public virtual void CloseWeaponChange(Part4_1CloseWeapon _CloseWeapon)//Part4_1CloseWeapon 함수 복사해와서 _CloseWeapon로 교체
    {
        //!= null :먼가 들고있는경우
        if (Part8_1WeaponManager.currentWeapon != null)
            Part8_1WeaponManager.currentWeapon.gameObject.SetActive(false);

        //인수로 넘어왔다
        CurrentCloseWeapon = _CloseWeapon;
        //모든 객체는 트랜스폼이 있어서 오류가 안뜬다
        Part8_1WeaponManager.currentWeapon = CurrentCloseWeapon.GetComponent<Transform>();
        //에니메이션을 넣어준다
        Part8_1WeaponManager.currenWeaponAnim = CurrentCloseWeapon.anim;


        //정조준 하면 좌표값이 바뀌는데 다른무기로 교체하고 오면 좌표값이 틀려져 있을수 있기때문에 Vector3.zero;로 초기화 해준다
        CurrentCloseWeapon.transform.localPosition = Vector3.zero;
        CurrentCloseWeapon.gameObject.SetActive(true);
       
    }
}
