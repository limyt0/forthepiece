using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Part5_2_1GunController : MonoBehaviour
{
    //활성화 여부.
    public static bool isActivate = false;
    //현재 장착된 총
    [SerializeField]
    private Part5_1_1Gun currentGun; //Part5_1Gun에 있는것을 참조

    //연사 속도 계산
    private float currentFireRate;//연사속도를 깍을것이다. 0이되면 발사할수 있게

    //상태변수
    private bool isReload = false;//재장전이 false일때만 발사가 이뤄져야한다  
    
    [HideInInspector]//public일 경우 Inspector에 노출되서 불필요하게 띄우지 않게
    public bool isFineSightMode = false;//true면 정조준, false면 기본상태

    //정조준하고 돌아오는 본래 포지션값 .   
    private Vector3 originPos;

    //효과음 재생
    private AudioSource audioSource;//선언

    //레이저 충돌 정보 받아옴
    private RaycastHit hitinfo;

    //필요한 컴포넌트
    [SerializeField]
    private Transform theCam;//총알이 화면가운데(크로스에어)에 나올꺼기 때문에 카메라를 가져온다
    private Part7_1Crosshair theCrosshair;

    //피격 이펙트
    [SerializeField]
    private GameObject hit_effect_prefab;

    NormalZombie_AI norzom_ai;//11.12
    PlayerParams playerparams; //11.12
    public Transform gun_ray; //11.12 총 나가는 판정용 오브젝트
    void Start()
    {
        originPos = Vector3.zero;
        //선언 했으면 inspector창에 Audio Source 컴퍼넌트를 넣어줘야한다
        audioSource = GetComponent<AudioSource>();
        //originPos = transform.localPosition;//SerializeField로 해서 인스펙터창에서 보고 할것이라 필요없다
        theCrosshair = FindObjectOfType<Part7_1Crosshair>();

        //11.12 초기값으로 자기자신 object 정보 넣기
        playerparams = this.transform.GetComponent<PlayerParams>();
    }
    void Update()
    {
        if (isActivate)
        {
            GunFireRateCalc();//연사속도 깍을 함수
            TryFire();//발사할수 있게 시도
            TryReload();//재장전 시도
            TryFineSight();//정조준 시도
        }
    }

    
    private void GunFireRateCalc()//연사속도 재계산
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;//1초에 1씩 감소
    }

    private void TryFire()//발사 시도
    {
        //원래 코드 --------------
        //if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)//0보다 작으면 발사할수있게
        //11.12 공격키 변경했음.
        if (
           OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)
           && currentFireRate <= 0 && !isReload
          )
        {
            Fire();//발사 이루어짐, 함수 추가
        }
    }
    private void Fire()//쏘기위한 과정 //발사전
    {
        if (!isReload)
        {
            //총알이 하나라도 있으면 
            if (currentGun.currentBulletCount > 0)
                Shoot();
            else
            {
                CancelFineSight();//재장전전에 정조준 취소
                StartCoroutine(ReloadCoroutine());
            }
        }

    }
    private void Shoot()//발사 후
    {
        theCrosshair.FireAnimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;//연사 속도 재계산(발사후에 재계산)
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();//파티클 플레이
        Hit();
        StopAllCoroutines();//ps. 동시에 while문이 영원히 경쟁하므로 그걸막기위해 Stop
        //총기 반동 코루틴 실행(총알발사와 총기반동은 병렬처리(스위치)로 이루어져야 하기 때문에 코루틴),(대기시간도 줄수있기때문에 코루틴이 편하다)
        StartCoroutine(RetroActionCoroution());
        //Debug.Log("총알 발사함"); //닿는걸 만들거기 때문에 필요없음

    }

    //적중된 위치 알아보기
    private void Hit()//오브젝트플링기법으로 총알이 직접나가는것도 있는데 여기서는 쏘면 바로 맞추게(발사속도가 빨러서 그게 그거다)
    {
                //(Local처럼 상대좌표가 아닌 절대좌표인 월드로 가야한다, 앞으로발사, 반환 저장, 사정거리
        if(Physics.Raycast(gun_ray.transform.position, gun_ray.transform.forward +
        /*정확도 추가*/ new Vector3(Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                            Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                            0)
                , out hitinfo, currentGun.range))
        {
            //맞는 오브젝트 이름 반환
            Debug.Log(hitinfo.transform.name); //이거 대신에 effect prefab
            //(프리팹,point:충돌객체반환, 바라보고 있는상태로 회전시킨다(normal:표면반환)
            GameObject clone = Instantiate(hit_effect_prefab, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
            Destroy(clone, 2f);//하이어라키 메모리에 쌓이는거 방지(반환타입을 모를때 var, 우리는 GameObject를 알고있어서 써준다)
            //11.12 적 공격 판정
            if (hitinfo.transform.tag == "Zombie") {
                norzom_ai = hitinfo.transform.GetComponent<NormalZombie_AI>();
                norzom_ai.attacked(currentGun.damage); //주인공 공격력만큼 공격값넣기
            }
        }
    }

    private void TryReload()//R버튼 누르면 재장전시도
    {                                                 //재장전 갯수보다 작을 경우만 실행:가득한 상태에서 재장전 안되게
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();//여기다 넣으면 정조준을 해제하면서 Reload가 된다
            StartCoroutine(ReloadCoroutine());
        }

    }

    public void CancelReload()
    {
        if(isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }

    //재장전
    IEnumerator ReloadCoroutine() //재장전할동안 발사 못하도록 코루틴으로 바꿈
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;
            currentGun.anim.SetTrigger("Reload");
            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;
            yield return new WaitForSeconds(currentGun.reloadTime);

            //현재 소유한 총알갯수            //재장전 갯수(10발)
            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;//전부 재장전
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;//현재 소유한 총알갯수에서 재장전갯수만큼 뺀다
            }
            else //재장전 갯수가 10발 미만이면
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("소유한 총알이 없습니다.");
        }


    }
    private void TryFineSight()//정조준시도
    { //ps.&& !isReload: 재장전이 아닐때만 이루어지게(이걸 안할시 재장전시 마우스 우클릭하면 총알 발사 재장전 오류)
      
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();//또 함수 만듬
        }
        //마우스 오른쪽!Fire2, 마우스 왼쪽! Fire1
    }

    //정조준에서 재장전시 정조준 취소되게
    public void CancelFineSight()//정조준 취소
    {//is는 bool이라서 is명시
        if (isFineSightMode)//isFineSightMode true면
            FineSight();//아래 FineSight함수상태가 될때고 false로 바뀌고 정조준상태가 취소가 될것이다
    }

               
    private void FineSight()//정조준 로직가동    
    {
        //true, false가 이루어 지는것은 Gun0_FineSight는 Bool에니메이션으로 해줘야한다, 그리고 계속 상태전의가 일어나는 Any State로 
        //실행하는것보다 다른 에니메이션들과 사이에 이루어지는게 낫다. Animator확인
                           
        isFineSightMode = !isFineSightMode;//FineSight 실행될때 마다 알아서 true, false 바뀌게
        //에니메이션 가동 //바로 윗줄에 !isFineSightMode(스위치역할)을 했기때문에 여기서 true, false을 알수가 없어서 isFineSightMode해줘야한다  
        currentGun.anim.SetBool("FineSightMode", /*true*/isFineSightMode);
        theCrosshair.FineSightAnimation(isFineSightMode);//정조준 크로스헤어 추가
        if(isFineSightMode)
        {
            StopAllCoroutines();//Lerp같은 경우같은경우 정확히 숫자가 떨어지지 않아서 While문을 빠져나가지 않아서 코루틴을 중지 해줘야한다
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCoroutine());
        }

    }


    //Lerp같은 경우같은경우 정확히 숫자가 떨어지지 않아서 While문을 빠져나가지 않아서 코루틴을 중지 해줘야한다
    //정조준 활성화
    IEnumerator FineSightActivateCoroutine()//화면가운데에 오게하는 Lerp실행
    {
        //(이총의 현재위치 != 정조준위치가 될때까지 반복(!= 같지 않을때 까지 반복)
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)//자식 객체일때 localPosition을 사용한다
        {
            //Mathf.Lerp(float), Vector3.Lerp(vector3) 둘이 같지만 인수가 틀림                             //0.2f세기로~
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;//1프레임 대기
        }
    }

    //정조준 비활성화
    IEnumerator FineSightDeActivateCoroutine()//다시 원래의 값으로 Lerp실행
    {
        //(이총의 현재위치 != 기본포지션값이 될때까지 반복(!= 같지 않을때 까지 반복)
        while (currentGun.transform.localPosition != originPos)//자식 객체일때 localPosition을 사용한다
        {
            //Mathf.Lerp(float), Vector3.Lerp(vector3) 둘이 같지만 인수가 틀림                    기본포지션값 //0.2f세기로~
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;//1프레임 대기
        }
    }

    IEnumerator RetroActionCoroution()//반동코루틴
    {
        //정조준안했을때 최대반동     90도 꺽었기 때문에 x축 반동을 넣어준것이다,나머진 그대로
        Vector3 recorlBack = new Vector3(currentGun.retroactionForce, originPos.y, originPos.z);
        //정조준 했을때의 최대반동
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        //정조준상태가 아닌경우 
        if(!isFineSightMode)
        {
            
            currentGun.transform.localPosition = originPos;//처음 위치로 돌리는것
            //반동시작                                                       //- 0.02f여우(대충 일치하면 끝내라), count로 하는방법과 이방법이 있음) 
            while(currentGun.transform.localPosition.x <= currentGun.retroactionForce - 0.02f)
            {                                                                         //일반목적지//반동이 빨리 이루어지게 0.4f
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recorlBack, 0.4f);
                OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch); //11.12 진동구현 -배터리 방지 위해 일단 꺼둠
                
                yield return null;//대기. 매프레임마다 이반복이 이루어질수 있도록
            }

            //원위치                       //!= 될때까지 반복
            while(currentGun.transform.localPosition != originPos)
            {                                                   //(현재위치, 원래위치까지 반복, 느린속도로)        
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch); //11.12 진동멈춤 -배터리 방지 위해 일단 꺼둠
                yield return null;
            }
        }
        //정조준 상태가 맞는경우
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;//정조준상태의 위치로 돌리는것
            //반동시작                                                       //- 0.02f여우(대충 일치하면 끝내라), count로 하는방법과 이방법이 있음) 
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {                                                                         //정조준목적지//반동이 빨리 이루어지게 0.4f
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;//대기. 매프레임마다 이반복이 이루어질수 있도록
            }

            //원위치                       //!= 될때까지 반복
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {                                                   //(현재위치, 원래위치까지 반복, 느린속도로)        
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }

        }
    }

    //사운드 재생
    private void PlaySE(AudioClip _clip)//효과음 재생
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public Part5_1_1Gun GetGun()
    {
        return currentGun;
    }
    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }

    public void GunChange(Part5_1_1Gun _gun)
    {
        //!= null :먼가 들고있는경우
        if (Part8_1WeaponManager.currentWeapon != null)
            Part8_1WeaponManager.currentWeapon.gameObject.SetActive(false);

        //인수로 넘어왔다
        currentGun = _gun;//바꾸고자 할 총을 현재총에 넣어줌
        //모든 객체는 트랜스폼이 있다
        Part8_1WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        //에니메이션을 넣어준다
        Part8_1WeaponManager.currenWeaponAnim = currentGun.anim;
        //정조준 하면 좌표값이 바뀌는데 다른무기로 교체하고 오면 좌표값이 틀려져 있을수 있기때문에 Vector3.zero;로 초기화 해준다
        currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }



    
}
