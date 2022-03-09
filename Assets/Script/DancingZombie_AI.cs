using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class DancingZombie_AI : MonoBehaviour
{
    public enum eActionState
    {
        IDLE = 0,
        HIPHOP,
        GANGNAM,
        SURPRISE,
        RUNAWAY,
    }



    [SerializeField] float _runSpeed = 5; //뛰는속도
    [SerializeField] float _minTime = 1f;
    [SerializeField] float _maxTime = 3f; //위 min과 max가 timeWait변수에 들어간다.

    eActionState _stateAction;
    Animator _ani;
    NavMeshAgent _navAgent;

    Vector3 _startPos;
    Vector3 _posTarget;

    float _speed; //runSpeed, walkSpeed등 최종적 이동속도를 받을 변수
    bool _isDead;
    
    
    float _timeWait;
    bool _isSelectAI;
    int _characterStd;
    bool _isDancing;
    bool _isSurprised;

    private void Awake()  //남의 것을 가져올 때는 Start, 내 것을 가져올땐 Awake가 더 편하다.
    {
        _isDead = false;
    
        _ani = GetComponent<Animator>();
        _navAgent = this.GetComponent<NavMeshAgent>();

        _startPos = this.transform.position;//시작때 위치한 좌표를 얻어옴
        _timeWait=0;
        _isSelectAI = false;
        _characterStd = 0;
        _isDancing = false;
        _isSurprised = false;

    
    }




    void Update()
    {
        if (_isDead)
            return;

        switch(_stateAction)                   
        {
            case eActionState.IDLE:
                _timeWait -= Time.deltaTime;
                if(_timeWait<=0)
                {
                    //기다릴 시간이 0이 되면 다시 갈지 말지 선택
                    _isSelectAI = false;
                }
                break;
            case eActionState.GANGNAM:
                break;
            case eActionState.HIPHOP:
                if(_isDancing==false)
                {
                    _isSelectAI = false;
                }
                break;

        }
        

        ProcessAI();
    }
    void ProcessAI()
    {
        //AI에서 첫번째가 주어진 명령을 끝까지 수행하는 것이라면
        //두번째는 각각의 상황에서 선택의 순서를 잘 만들어 주는 것이다.
        //움직일지 말지를 결정이 먼저.
        if (!_isSelectAI)
        {
            int r = Random.Range(0, 100);
            if (r <= 30)
            {
                ChangedAction(eActionState.IDLE);
                _timeWait = Random.Range(_minTime, _maxTime);

                _isSelectAI = true;  //선택을 했으니 true가 된다.
                                     //Debug.Log(r);
                                     //Debug.Log("Time=" + _timeWait);
            }
            else if (r <= 70)
            {
                ChangedAction(eActionState.GANGNAM);
                _isSelectAI = true;
            }
            else
            {
                ChangedAction(eActionState.HIPHOP);
                _isSelectAI = true;
            }
        }
    }


    void ChangedAction(eActionState state)
    {
        switch (state)
        {
            case eActionState.IDLE:
                _stateAction = state;
                _ani.SetInteger("AnyState", 0);
                break;

            case eActionState.GANGNAM:
                {
                _stateAction = state;
                _ani.SetInteger("AnyState", 1);
                }
                break;

            case eActionState.HIPHOP:
                {
                    _stateAction = state;
                    _ani.SetInteger("AnyState", 2);
                }
                break;

            case eActionState.SURPRISE:
                {
                    _isDancing = false;
                    _stateAction = state;
                    _ani.SetInteger("AnyState", 3);
                 
                }
                break;
            case eActionState.RUNAWAY:
                {
                    _ani.SetInteger("AnyState", 4);
                    GameObject RP = GameObject.FindGameObjectWithTag("RunPoint");
                    _posTarget = RP.transform.position;
                    _navAgent.destination = _posTarget; //클릭당했을때 갈 목표위치
                }
                break;

        }
    }
 
    void HiphopEnd()
    {
        _isDancing = false;
    }
     
    void SurpriseEnd()
    {
        _isSurprised = true;
        ChangedAction(eActionState.RUNAWAY);

    }
    void OnMouseDown()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); //플레이어 위치를 가져옴
       

        this.transform.LookAt(player.transform);
        _isDancing = false;
        ChangedAction(eActionState.SURPRISE);

       
    }
}
    