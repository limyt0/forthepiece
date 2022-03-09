using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NormalZombie_AI : MonoBehaviour 
{
    public enum eActionState
    {
        IDLE = 0,
        WALK,          //IDLE, WALK는 좀비 성격에 따라 랜덤으로 실행. 이부분을 앞으로 AI라고 부를게요
        RUN,           
        ATTACK,
        HIT,
        DEAD
    }

    public enum eCharacter //좀비 성격으로 유니티 인스펙터창에서 설정 가능(AI관련)
    {
        Fierce=0, //돌아다닐 확률이 높다
        lazy      //가만히 있을 확률이 높다.
    }


    //좀비가 비공격모드일때 돌아다니는 구역, 시간(AI관련)
    [SerializeField] float _limitX = 5;
    [SerializeField] float _limitZ = 5;
    [SerializeField] float _minTime = 2f;
    [SerializeField] float _maxTime = 7f; //위 min과 max가 timeWait변수에 들어간다.
    [SerializeField] eCharacter _eCh;
    [SerializeField] float _MAXHP; //11.11 좀비 고유 HP 추가.
    float _HP;//11.11 좀비 고유 HP 추가.

    float _timeWait; //좀비가 IDLE이나 WALK 상태일때 _minTime~_maxTime 사이 시간동안 행동하고 다음 행동을 결정한다.


    //좀비의 이동 속도
    [SerializeField] float _runSpeed = 3;       //뛰는속도
    [SerializeField] float _walkSpeed = 0.5f;   //걷는 속도
    float _speed; //runSpeed, walkSpeed등 최종적 이동속도를 받을 변수


    //좀비 공격모션 구간에서만 콜라이더 활성화 제어를 위함 공격 애니메이션의 DamageOn, DamageOff 이벤트함수에서 제어된다.
    [SerializeField] BoxCollider _DamageZone;

    //플레이어의 좀비 공격 판정을 위한 좀비 메쉬를 싸고 있는 박스콜라이더
    BoxCollider _ZombieCollision;

    eActionState _stateAction;
    NavMeshAgent _navAgent; //벽뚫거나 장애물에 막혀 허우적대는걸 막기 위해 네비메쉬를 썼습니다.
    Animator _ani;
    Rigidbody rig;


    //좀비 이동지점
    Vector3 _startPos;    
    Vector3 _posTarget;

    //상태 체크 
    bool _isDead;       //죽으면(True) Update 시작하고 반환하여 아래 코드 실행X
    bool _isSelectAI;   //비선택시(False) ProcessAI 함수가 실행되어 다음 행동 결정
    bool _isAttackEnd;  //공격 애니메이션의 이벤트를 함수 속에서 공격이 끝난 지점 체크

    //캐릭터 성격에 따른 AI 행동 확률 조절
    int _characterStd;
   

    //좀비 체력(마우스 클릭당 1 차감)
    //int ZombieHP=3;

    //플레이어 위치 구하기용
    GameObject player;
    public Scrollbar zomebie_HP_scroll; //11.11 좀비 HP UI

    ZombieParams curZombParams;//11.11 좀비 정보가 담긴 스크립트

    private void Awake()  //각종 컴포넌트와 변수 설정
    {
        _ani = GetComponent<Animator>();
        _navAgent = this.GetComponent<NavMeshAgent>();
        _ZombieCollision = this.GetComponent<BoxCollider>();

        _timeWait = 0;
        _startPos = _posTarget = this.transform.position;   //시작때 위치한 좌표를 얻어옴

        _isDead = false;
        _isSelectAI = false;
        _isAttackEnd = true;
        
        player= GameObject.FindGameObjectWithTag("Player"); //태그로 플레이어를 정함
        curZombParams = this.GetComponent<ZombieParams>();//11.11 좀비 정보 스크립트 내용 초기값가져옴
    }


    void Start()  //캐릭터 성격 Fierce(공격적)/Lazy(게으름)에 따른 IDLE 행동을 취할 확률 책정
    {
        curZombParams.curHP = _HP = _MAXHP;//11.12 HP 초기값 세팅.

        switch (_eCh)
        {
            case eCharacter.Fierce:
                _characterStd = 25;
                _runSpeed = 4;
                curZombParams.curHP = 200;//11.12 HP값세팅
                break;
            case eCharacter.lazy:
                _characterStd = 80;
                _runSpeed = 3;
                curZombParams.curHP = 100;//11.12 HP값세팅
                break;              
        }

    }


    //==========================================================================================
    //상태에 따른 변화 프로세스 이해
    //1. 플레이어 행동이 없을 경우 자동으로 좀비 성격에 따른 행동변화가 일어납니다.
    //   이 행동변화는 void ProcessAI()함수에서 결정됩니다.
    //     *호출 : Update에서 _isSelcectAI이 True인지 False인지 체크하여 False시 호출.
    //             IDLE은 랜덤으로 뽑은 시간이 되었는지, WALK는 목표지점에 도착했는지를 기준으로 
    //             _isSelectAI가 false가 되고, ProcessAI함수 호출하여 다음 행동 결정하면 True가 되어 호출X
    //            
    //2. 플레이어가 공격(마우스클릭)하면 OnMouseDown 이벤트가 호출된다
    //   좀비 체력이 남아있으면 상태가 HIT로 이동-> 맞은 다음 플레이어 거리에 따라 Attack할지, 플레이어쪽으로 Run할지 결정
    //   좀비 체력이 0이 되면 상태가 DEAD가 된다.
    //
    //3. Update함수에서는 AI관련, 움직이는 Player 위치 관련하여 상태변화가 필요한 코드 입력
    //   ChangeAction 함수에서는 애니매이션의 구체적 제어
    //============================================================================================


    void Update() //가급적 상태에 따라 실시간 체크가 필요한 부분만 입력
    {
        if (_isDead)                                 //ChangeAction 함수에서 상태가 DEAD가 되면 _isDead가 True가 되고, 아래실행X
            return;

        switch(_stateAction)                   
        {
            case eActionState.IDLE:
                _timeWait -= Time.deltaTime;         //ProcessAI에서 결정된 _timeWait(목표시간)에서 1초씩 카운트다운한다.
                if(_timeWait<=0)                     //0초가 되면 다음 AI행동 결정을 위해 _isSelectAI를 false로 하여 ProcessAI함수가 실행가능하도록 한다.
                {
                    _isSelectAI = false;
                }
                break;

            case eActionState.WALK: 
                if(Vector3.Distance(_posTarget, transform.position)<=0.1f)    //ProcessAI에서 결정된 _posTarget(목표지점)과 좀비(this)의 거리를 매 프레임 체크
                {                                                             //***좀비가 제자리걸음을 하면 두 거리가 0.1거리 아래로 떨어지지 않는 것이다.
                                                                              //***지형의 문제 등으로 거리가 좁혀지지 않는 것일 수 있으니 0.1f 수치값을 조절하여 확인해본다.
                    _isSelectAI = false;                                      //둘 거리가 0.1f이하가 되면 _isSelectAI를 false로 하여 ProcessAI함수가 실행가능하도록 한다.
                }                                                             
                break;

            case eActionState.RUN: 
                if (_isAttackEnd == true)                                     //좀비의 공격 애니매이션이 끝나거나 공격상태가 아닐때
                {
                    _posTarget = player.transform.position;                   //플레이어 실시간 위치를 구해서
                    _navAgent.destination = _posTarget;                       //플레이어를 향해 달려간다.
                }

                _posTarget = player.transform.position;                       //공격 상태여도 다음 행동을 위해 플레이어위치를 실시간으로 구한다.

                if (Vector3.Distance(_posTarget, transform.position) <= 3f)   //플레이어와 좀비 거리가 3f이하면
                { ChangedAction(eActionState.ATTACK); }                       //공격을 한다.

                if ((Vector3.Distance(_posTarget, transform.position) >= 30f))//플레이어와 좀비 거리가 30f이상이 되면
                { ChangedAction(eActionState.IDLE); }                         //좀비는 다시 IDLE상태가 되고 ProcessAI를 실행한다.
                break;

            case eActionState.ATTACK:
                _posTarget = player.transform.position;                       //공격할지 달려갈지 결정을 위해 플레이어의 실시간 위치를 구한다.

                //아래 두줄은 플레이어쪽으로 자연스럽게 회전하기 위한 부분
                Vector3 dir = player.transform.position - this.transform.position;
                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);
                
                if (Vector3.Distance(_posTarget, transform.position) > 3.5f&&_isAttackEnd==true)  //플레이어와 좀비 거리가 3.5이상이고 공격 끝난상태면
                { ChangedAction(eActionState.RUN); }
                break;

            case eActionState.HIT:
                
                _isAttackEnd = true;  //좀비가 ATTACK상태일때 플레이어 공격으로 HIT를 맞으면 공격종료를 알리는 AttackEnd이벤트가 호출되지 않아
                                      //제대로 움직이지 않을 수 있으므로 일단 공격상태가 끝난 것으로 간주한다.

                if (_ani.GetCurrentAnimatorStateInfo(0).IsName("Zombie Reaction Hit")) //지금 실행되는 애니가 HIT 애니(제목으로찾음)가 맞는지
                {
                    if (_ani.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)      //지금 실행되는 애니가 다 재생되면(0이 시작, 1이 끝)
                    {
                        _posTarget = player.transform.position;                        //플레이어의 위치를 구해서
                        if (Vector3.Distance(_posTarget, transform.position) <= 3f)  //플레이어 위치와 좀비 위치 거리가 3.1f미만이면 
                        {
                            ChangedAction(eActionState.ATTACK);                        //공격한다.
                        }
                        else                                                           //거리가 3.1 이상이면
                        {
                            ChangedAction(eActionState.RUN);                           //다시 플레이어를 향해 달려간다.
                        }
                    }
                }
                break;

            case eActionState.DEAD:
                break;
        }
        ProcessAI();
        HP_scroll();//11.11 player 시야 따라가는 HPbar 
    }

    //****************************************************************************************************
    //*********************************          함수 PART         ***************************************
    //****************************************************************************************************

    Vector3 GetRandomPos(Vector3 center, float limitX, float limitZ)//좀비가 WALK상태일때 시작 구간 내에서 목표 지점을 랜덤으로 결정해주는 함수. 
                                                                    //ProcessAI 함수에서 쓰인다.
    {
        float rX = Random.Range(-limitX, limitX); //limitX가 5면 -5~5 사이의 값을 랜덤하게 얻어온다.
        float rZ = Random.Range(-limitZ, limitZ); //x, z를 따로 받는 이유는 정사각형이 아닌 직사각형 영역도 받기위해
        Vector3 rv = new Vector3(rX, 0, rZ);      //랜덤 위치 값을 좌표형식으로 받아옴
        return center + rv;                       //center값은 좀비의 시작위치로 받아온다. 
                                                  //center값이 없으면 월드좌표상 0,0에서만 구간이 설정된다.
    }

    void ProcessAI() //플레이어와 교전 상태가 아닐 때, 좀비의 IDLE/WALK상태를 결정하는 AI함수
    {
         //이 함수가 호출되면 좀비가 IDLE을 할지 WALK를 할지 확률에 따라 결정을 한다.
         //정해진 목표를 달성할때까지 다음 행동 결정을 보류해야한다. -> 달성여부는 Update에서 체크한다.
         //달성조건이 되면 Bool형 _isSelectAI를 이용해 다시 결정이 가능하다.

        if (!_isSelectAI) //IDLE 목표시간 미달, WALK 목표지점 미도달, 플레이어와 교전 상태일때는 실행X
        {
            print("AI실행");
            int r = Random.Range(0, 100); //Start함수에서 설정한 _characterStd값을 확률로 생각하여, 보기 쉽게 100으로 함

            if (r < _characterStd)  //대기상태
            {
                ChangedAction(eActionState.IDLE);                           //상태는 IDLE
                _timeWait = Random.Range(_minTime, _maxTime);               //_minTime과 _maxTime사이 시간을 추출
                                                                            //_timeWait(대기시간)Update의 IDLE부분에서 카운트다운
            }
            else //걷기
            {
                ChangedAction(eActionState.WALK);                           //상태는 WALK
                _posTarget = GetRandomPos(_startPos, _limitX, _limitZ);     //목표지점을 정하고
                _navAgent.destination = _posTarget;                         //목표지점까지 네비에이전트로 이동한다.
                                                                            //Update의 WALK에서 목표지점과 좀비의 거리를 계속 체크한다.
            }

            _isSelectAI = true;  //무엇이 되었든, 다음 행동을 결정했으니 _isSelect는 true가 되어 
                                 //Update에서 체크하여 Flase값이 넘어오기 전까지 이 함수는 이 함수는 실행되지 않는다.
        }
    }

    void ChangedAction(eActionState state) //일단 상태가 변하면 이 함수가 호출된다.
    {
        switch (state)
        {
            case eActionState.IDLE:
                print("IDEL");
                _stateAction = state; //이 함수에서 값이 셋팅된 다음 _stateAction를 통해 Update로 상태를 넘긴다.
                _ani.SetInteger("AnyState", 0);
                break;

            case eActionState.WALK:
                if (_stateAction != eActionState.ATTACK)  //공격상태가 아닐때만 바뀐다.
                {
                    print("WALK");
                    _stateAction = state;

                    _navAgent.speed = _walkSpeed;         //걷는 상태일때 스피드값
                    _navAgent.stoppingDistance = 0;       //정확하게 목표지점에 도착할때까지 이동한다.
   
                    _ani.SetInteger("AnyState", 1);
                }
                break;

            case eActionState.RUN:                 //공격할때 플레이어와의 거리를 조절하려면 여기서
                print("RUN");
                _stateAction = state;

                _navAgent.speed = _runSpeed;
                _navAgent.stoppingDistance = 2.5f; //공격할때는 정확히 플레이어 위치가 아니라 조금 떨어져있어야함. 

                _ani.SetInteger("AnyState", 2);           
                break;

            case eActionState.ATTACK:
                print("ATTACK");
                _stateAction = state;

                _isAttackEnd = false;

                _ani.SetInteger("AnyState", 3);
                break;

            case eActionState.HIT:                    //마우스로 공격당하면 일단 여기로 넘어온다(죽는건 그냥 바로 죽음)
                print("Hit");
                _stateAction = state;

                _posTarget = this.transform.position; //좀비가 목표지점으로 이동하는 중에 맞으면 그 위치에 멈추게 하기위해
                _navAgent.destination = _posTarget;   //HIT 상태변화가 되는 순간의 위치를 받아 목표지점을 수정한다.

                _isSelectAI = true;                   //일단 맞으면 ProcessAI가 실행되지 않게 한다.
                                                      //HIT를 거친 다음에 RUN이냐 ATTACK이냐가 결정되므로
                                                      //플레이어와 일정 거리 이상 벌어져 다시 IDLE이 되기 전까지 ProcessAI는 실행X
                
                _ani.SetInteger("AnyState", 4);
                break;

            case eActionState.DEAD:
                print("DEAD");
                _stateAction = state;

                _isDead = true;                        //isDead가 True가 되어 더이상 Update 실행이 되지 않는다.
                _ZombieCollision.enabled = false;      //추가적 OnMouseDown이벤트 호출을 막기 위해 BoxCollider를 끈다.

                _posTarget = this.transform.position;  //좀비가 목표지점으로 이동하는 중에 맞으면 그 위치에 멈추게 하기위해
                _navAgent.destination = _posTarget;    //DEAD 상태변화가 되는 순간의 위치를 받아 목표지점을 수정한다.

                _ani.SetInteger("AnyState", 5);                
                break;
        }
    }


    //====================================================================================================
    //좀비가 공격할때만 좀비 앞의 박스콜라이더 _DamageZone이 활성화되어 플레이어에게 데미지를 줄 수 있다.
    //좀비 공격이 끝나면 콜라이더가 꺼져 불필요한 충돌을 막는다.
    //====================================================================================================
    void DamageOn() //Zombie Punching 애니매이션의 DamageOn Event Marker에 닿으면 호출된다.
    { _DamageZone.enabled = true; }
    void DamageOff() //Zombie Punching 애니매이션의 DamageOn Event Marker에 닿으면 호출된다.
    { _DamageZone.enabled = false;}

    void AttackEnd()
    { _isAttackEnd = true; }


    //===========================================================================================
    //플레이어가 좀비를 공격할 때 좀비의 데미지 관리 부분.
    //현재는 마우스 클릭으로 판정하므로, VR로 옮길 경우 아래부분을 OnTriggerEnter 등 다른 함수로 수정해야한다.
    //===========================================================================================
    /*void OnMouseDown()
    {
        print("플레이어가 공격함. 남은HP=" + ZombieHP);
        --ZombieHP;          //좀비가 클릭될때마다 일단 HP가 1씩 깎인다.

        if (ZombieHP >= 1)   //좀비 체력이 1이상이면 맞는 액션(HIT)을 취한다.
        { ChangedAction(eActionState.HIT); }
        else                 //좀비 체력이 1미만이면 죽는다.
        { ChangedAction(eActionState.DEAD); }
    }*/

    //11.11 피격판정
    
    public void attacked(float Attackpower)
    {
        
        curZombParams.SetAttackedDamage(Attackpower);
        zomebie_HP_scroll.size = curZombParams.curHP / 200.0f; // 11.13 공격받은직후 HP 적용.
        //11.12 위코드와 원래코드 합친거
        if (curZombParams.curHP >= 1)   //좀비 체력이 1이상이면 맞는 액션(HIT)을 취한다.
        { ChangedAction(eActionState.HIT); }
        else                 //좀비 체력이 1미만이면 죽는다.
        {

            zomebie_HP_scroll.transform.Find("Sliding Area").gameObject.SetActive(false);

            ChangedAction(eActionState.DEAD);
        
        }
        //값은 zombieparam으로 넘겨주고 계산은 characterparam에서 계산함.
        //print("좀비 현재 HP: " + curZombParams.curHP);

    }


    //11.11 좀비 HP player 시야 따라감.
    void HP_scroll()
    {
        
        zomebie_HP_scroll.transform.rotation =
            Quaternion.Euler(0, GameObject.FindGameObjectWithTag("MainCamera").transform.localEulerAngles.y, 0);



    }
}
    