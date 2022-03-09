using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

//무기 교체하고 여러가지 동작을 할것이다----------------
//Dictionary:사전

//넣으면 뺄수없게 하는 기능 : [RequireComponent(typeof(Part5_1_2GunController))]:뺄때마다 오류가 생긴다
public class Part8_1WeaponManager : MonoBehaviour
{
    //Part8_1WeaponManager 접근할수 있는 공유자원, 정적 변수, 많이 쓰면 안되고 1~2개정도
    //공유자원을 달리줄수 없다, 자원낭비, 쉽게접근, 보호수준 떨어짐,
    public static bool isChangeWeapon = false; //무기 중복 교체 실행 방지하기 위해 true되면 무기교체 안되게 막고 일정시간뒤 false로~
    
    //현재 무기와 현재 무기의 에니메이션(기존의 무기를 껏다 켯다 하는것, 기본적인 컴퍼넌트 Tranform을 넣어줬다
    public static Transform currentWeapon;
    public static Animator currenWeaponAnim;   
    
    //현재 무기 타입(도끼, 무기등): 차후 정조준일때 정조준해제해라 등에 이용
    [SerializeField]
    private string currentWeaponType;


    //일정시간을 위한 변수, 무기 교체 딜레이 타임.
    [SerializeField]
    private float changeWeaponDelayTime;
    //무기 교체가 완전히 끝난 시점.
    [SerializeField]
    private float changeWeaponEndDelayTime;

    //무기 종류들 전부 관리
    [SerializeField]
    private Part5_1_1Gun[] guns;
    [SerializeField]
    private Part4_1CloseWeapon[] hands;
    [SerializeField]
    private Part4_1CloseWeapon[] axes;
    [SerializeField]
    private Part4_1CloseWeapon[] Pickaxes;

    //배열만로 찾기 힘들어서 관리하기 쉽게,   선언과 생성 동시에, 관리차원에서 쉽게 무기 접근이 가능하도록 만듦.
    private Dictionary<string, Part5_1_1Gun> gunDictionary = new Dictionary<string, Part5_1_1Gun>();
    private Dictionary<string, Part4_1CloseWeapon> handDictionary = new Dictionary<string, Part4_1CloseWeapon>();
    private Dictionary<string, Part4_1CloseWeapon> axeDictionary = new Dictionary<string, Part4_1CloseWeapon>();
    private Dictionary<string, Part4_1CloseWeapon> pickaxeDictionary = new Dictionary<string, Part4_1CloseWeapon>();

    //필요한 컴포넌트
    //끄고 키고 활성화 하기위해
    [SerializeField]
    private Part5_2_1GunController  theGunController;
    [SerializeField]
    private Part4_2HandController theHandController;
    [SerializeField]
    private Part9_2AxeController theAxecontroller;
    [SerializeField]
    private Part10_4PickaxeController thePickaxeController;


    //11.11 무기상태정의(컨트롤러로 좌우 이동키로만 무기 바뀌게 하기 위함임.
    enum Weaponstate { 
          Hand, gun, Knife  
    }

    Weaponstate weaponstate = Weaponstate.Knife;

    void Start()
    {
         //11.11무기상태 처음 나이프로.
        //무기들을 넣어준다  //대문자 랭스
        for (int i = 0; i < guns.Length; i++)
        {
            //(guns i번째에 건네임이 들어가고, guns에 자기자신이 들어가게 된다)
            gunDictionary.Add(guns[i].gunName, guns[i]);
        }
        for (int i = 0; i < hands.Length; i++)
        {
            handDictionary.Add(hands[i].CloseWeaponName, hands[i]); 
        }
        for (int i = 0; i < axes.Length; i++)
        {
            axeDictionary.Add(axes[i].CloseWeaponName, axes[i]);
        }
        for (int i = 0; i < Pickaxes.Length; i++)
        {
            pickaxeDictionary.Add(Pickaxes[i].CloseWeaponName, Pickaxes[i]);
        }


    }
    private void Update()
    {
        weaponChange();
    }


    void weaponChange() {
        //1,2,3,4 누르면 무기교체
        if (!isChangeWeapon)//false일때만 교체되게 , 코루틴으로 만들거다
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))//무기 교체 실행("타입", "무기")
                StartCoroutine(ChangeWeaponCoroutine("HAND", "맨손"));

            else if (Input.GetKeyDown(KeyCode.Alpha2))
                StartCoroutine(ChangeWeaponCoroutine("GUN", "SubMachineGun1"));

            else if (Input.GetKeyDown(KeyCode.Alpha3))
                StartCoroutine(ChangeWeaponCoroutine("AXE", "Axe"));

            else if (Input.GetKeyDown(KeyCode.Alpha4))
                StartCoroutine(ChangeWeaponCoroutine("PICKAXE", "Pickaxe"));

        }

        //11.11 컨트롤러로 무기교체
        if (!isChangeWeapon)//false일때만 교체되게 , 코루틴으로 만들거다
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch)
                && weaponstate != Weaponstate.Knife)
            {
                weaponstate += 1;
                weapon_changed();
            }
            else if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch)
                && weaponstate != Weaponstate.Hand)
            {
                weaponstate -= 1;
                weapon_changed();
            } else if (
                (
                OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch) ||
                OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch) 
                ) &&
                (
                    weaponstate == Weaponstate.Hand  || weaponstate == Weaponstate.Knife
                )
              ) { 
                weaponstate = (Weaponstate)(Weaponstate.Knife - weaponstate);
                weapon_changed();
                //11.11최대값이 knife이므로 최대값에서 현재값을 빼면 최대->최소, 최소->최대값으로 이동 가능.
            }


            


        }



    }


    void weapon_changed() {
        if (weaponstate == Weaponstate.Hand)//무기 교체 실행("타입", "무기")
            StartCoroutine(ChangeWeaponCoroutine("HAND", "맨손"));
        else if (weaponstate == Weaponstate.gun)
            StartCoroutine(ChangeWeaponCoroutine("GUN", "SubMachineGun1"));
        else if (weaponstate == Weaponstate.Knife)
            StartCoroutine(ChangeWeaponCoroutine("AXE", "Axe"));
    }


                                           //(어떤타입:맨손이냐총이냐, 어떤이름:어떤총으로 바꿀지)
    public IEnumerator ChangeWeaponCoroutine(string _type, string _name)//퀵슬롯 1,2,3,4누르게 하려고 public로 함
    {
        isChangeWeapon = true;
        currenWeaponAnim.SetTrigger("Weapon_out");
        //집어 넣을때까지 대기
        yield return new WaitForSeconds(changeWeaponDelayTime);

        //정조준 상태 해제
        CancelPreWeaponAction();
        //실제 무기를 꺼낸다
        WeaponChange(_type, _name);//위에서 받은걸 그대로 넘겨줄것이다(_type, _name)

        yield return new WaitForSeconds(changeWeaponEndDelayTime);

        currentWeaponType = _type;
        //다시 false로 해서 무기 체인지가 가능하게
        isChangeWeapon = false;

    }
    //무기 취소 함수
    private void CancelPreWeaponAction()
    {
        switch(currentWeaponType)
        {
            case "GUN":
                theGunController.CancelFineSight();
                theGunController.CancelReload();
                Part5_2_1GunController.isActivate = false;
                break;
            case "HAND":
                Part4_2HandController.isActivate = false;
                break;
            case "AXE":
                Part9_2AxeController.isActivate = false;
                break;
            case "PICKAXE":
                Part10_4PickaxeController.isActivate = false;
                break;
        }
    }
    //무기 교체 함수.
    private void WeaponChange(string _type, string _name)
    {
        if(_type == "GUN")        
            theGunController.GunChange(gunDictionary[_name]);
        
        else if(_type == "HAND")
            theHandController.CloseWeaponChange(handDictionary[_name]);

        else if (_type == "AXE")
            theAxecontroller.CloseWeaponChange(axeDictionary[_name]);

        else if (_type == "PICKAXE")
            thePickaxeController.CloseWeaponChange(pickaxeDictionary[_name]);
    }
}
