using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Part2_PlayerController : MonoBehaviour
{
    //스피드 조정 변수 ---------------------------
    //플레이어 움직이기 위해서 기본적인 스피드
    [SerializeField]//보호수준은 유지,인스펙터에는 나오게(SerializeField쓴다고 모든것이 다 나오는것은 아니다)
    private float walkSpeed;//Start에 넣을것
    [SerializeField]
    private float HeadturnSpeed;//11.10 머리 회전 속도
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;//걷기 스피드보다 느려져야 한다
    
    private float applySpeed; // walkSpeed와 runSpeed를 합칠것이다

    [SerializeField]
    private float jumpForce;

    //상태 변수 ---------
    private bool isWalk = false;//걷고있다, Crosshair스크립트에 쓰일꺼
    private bool isRun = false; //뛰는것인지 안뛰는것인지 구분하기 위해..안뛰어도 기본값은 false;이기 때문에 보기좋게 작성했을뿐
    private bool isCrouch = false;//앉았는지 안앉았는지
    private bool isGround = true; //jump에서 땅에 있는지 없는지... 기본값은 땅에 있으니 true로 둔다(true일때만 점프하게 만들어줄것이다)


    //움직임 체크 변수
    private Vector3 lastPos;


    //앉았을때 얼마나 앚을지 결정하는 변수.
    [SerializeField]
    private float crouchposY;
    private float originposY;//원래 돌아갈 위치 //Start에 넣을것
    private float applyCrouchPosY;//위에 두개의 변수를 각각 여기다가 넣을것이다 //Start에 넣을것

    //땅 착지여부
    private CapsuleCollider capsuleCollider; //Start에 넣을것

    //민감도---------
    [SerializeField]//카메라의 민감도(조절가능해야 편하다)
    private float lookSensitivity;

    //카메라 한계 -------------
    [SerializeField]//카메라 로테이트 제한두기
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0; //정면, 45f입력하면  대각선 위

    //필요한 컴포넌트-------------
    [SerializeField]
    private Transform theCamera;
    private Rigidbody myRigid;//육체적 몸 //Start에 넣을것    
    private Part5_2_1GunController theGunConroller;
    private Part7_1Crosshair theCrosshair;


    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();        
        //시작하자마자 불러온다s
        myRigid = GetComponent<Rigidbody>();     
        theGunConroller = FindObjectOfType<Part5_2_1GunController>();
        theCrosshair = FindObjectOfType<Part7_1Crosshair>();

        theCamera.transform.position = this.transform.position+ new Vector3(0, 0.5f, 0.5f);
        //11.10 시작시 강제로 카메라 위치 맞추기(안그러면 할때마다 계속 틀어짐)
        

        //초기화
        //일단 달리기전까지 걷는상태
        applySpeed = walkSpeed;
        originposY = theCamera.transform.localPosition.y; //플레이어 안에 있어서 상대적인것이기 때문에 localPosition을 썻다
        applyCrouchPosY = originposY; //기본상태

       
    }

    //1초에 대략 60회 호출이라 생각
    void Update()
    {
        IsGround(); //이함수의 I는 대문자이다=>땅에 있는지 없는지 체크하는 함수
        TryJump();
        TryRun();//뛰거나 걷는걸 구분하는 함수를 만들어준다.Ps.꼭 Move();위에 있어야 뛰는지 걷는지 판단할수있다
        //키입력이 되면 움직임이 실시간으로 이루어질수있게 코드작성
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();

     
        
    }
    private void FixedUpdate()//업데이트 간격이 여유로워지는 함수(댓글에서 찾은 함수:Walk와 Idle가 번갈아가며 실행되는 문제)
    {
        MoveCheck();
    }

    private void TryCrouch()//앉기 시도
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();//크런치 실행 및 함수작성
        }
    }
    private void Crouch()//앉기 동작
    {
        if(isWalk)//댓글에서 찾은 함수(Walk 에니메이션이 실행되는 문제)
        {
            isWalk = false;
            theCrosshair.WalkingAnimation(isWalk);
        }
        //Crouch가 눌릴때마다 바뀌어야한다
        isCrouch = !isCrouch;//스위치 역활(true면 false로, false이면 true로), 아래의 내용을 축소한것        
        theCrosshair.CrouchingAnimation(isCrouch);
        
        if (isCrouch)//true일 경우
        {
            applySpeed = crouchSpeed; //앉았으니 앉는 스피드로
            applyCrouchPosY = crouchposY;//앉았을때는 crouchposY를 넣어주고
        }
        else//false일 경우
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originposY;//서있을때는 originposY를 넣어준다
        }
        
        StartCoroutine(CrouchCoroutine()); //이게 코루틴 시작이다
    }
    
    //부드러운 앉기동작 실행
    IEnumerator CrouchCoroutine()
    {
        //코루틴 장점: 일반함수들은 위에있는 명령어들이 실행되야 하지만 코루틴을 만나면 위에 명령어들과 같이 실행되게 된다 즉 병렬처리(빠르게 왔다갔다 병렬처리 하는것)
        float _posY = theCamera.transform.localPosition.y;
        //임시 변수 만들어서 15번 실행후 빠져나오게! (0.9999999이렇게 미세한 수치를 없애고 무한 실행되는것을 막기위해)
        int count = 0;

                   //applyCrouchPosY 아닐때(서있을때, 앉았을때가 아니고 그사이일때라는 나의 해석)
        while (_posY != applyCrouchPosY)//_posY가 목적지 applyCrouchPosY에 가게되면 빠져나온다
        {
            count++;//빠져나오게!
            //예를들어 보강은 (1, 2 ,0.5f):1에서 2사이에 2분에1씩 계산되서 증가!!! 1.5가되고, 1.75가 되고 서서히 이게 보강이다         
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.1f);//0.3f 낮을수록 천천히 증가
            //실제 카메라에 적용
            //x,y,z를 각각 수정할수 없으므로 localPosition은 벡터3이므로 vector3 자체를 넣어주어야 한다
            theCamera.transform.localPosition = new Vector3(0, _posY, 0); //카메라 포지션을 보면 0,1,0으로 되어있으니 y값만 _posY를 넣어주면 된다(x,z는 변하지 않는다)
            if (count > 15)//15번 실행하고 빠져나오게!
                break;
            //대기가 while문 안에 없으면 순식간에 일어나기 때문에 앉는과정을 보여주기 위해
            yield return null;//null은 1프레임 대기
        }
        //좋은기능: 1초동안 대기한다. 도중에 흐름을 끊을수가 있다
        //yield return new WaitForSeconds(1f);
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f); //목적지 도달하면 빠져나오게!
    }

    //지면 체크
    private void IsGround()
    {
        //print("IsGround"+ isGround);
        //공중에 있으면 false가 반환된다. 또한 착지한걸 확인할수 있다
        //position대신 Vector3를 쓰는 이유: position은 회전하면 회전한채로 캡슐의 바닥으로 레이저를 쏘니 고정된좌표 Vector3를 쓰는 것이다
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);//0.1f는 경사같은것이 있을수있으니 여유로 주는것(0.3f정도면 경사면 낙하판정은 안받음)
        theCrosshair.JumpingAnimation(!isGround);//점프 크로스헤어

    }

    //점프시도
    private void TryJump()
    {
        //print("땅 체크" + isGround);
        //추가:땅에있을때만
        if (
            (
            Input.GetKeyDown(KeyCode.Space)  ||
            (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger , OVRInput.Controller.LTouch)) //11.12
            )
            && isGround == true) //isGround == true => isGround 와 같다, !isGround:false일 경우
        {
            
            Jump();//함수 구현
        }
    }
    
    //점프
    private void Jump()
    {
        //앉아상태에서 점프시 앉은상태 해제(선택사항)
        if (isCrouch)
            Crouch(); 
                           //공중 * 세기
        myRigid.velocity = transform.up * jumpForce;//velocity를 인위적으로 변경시키는 방법
    }

    //달리기 시도
    private void TryRun()
    {
        //Shift를 누르면 달리도록  //
        if ( Input.GetKey(KeyCode.LeftShift) ||
           (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))  //11.12

          )
        {
            Running();//또 함수를 만들어줄것이다
        }

        //달리기 취소
        if( Input.GetKeyUp(KeyCode.LeftShift) ||
           (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) //11.13
          )
        {
            RunningCancel();
        }
    }

    //달리기 실행
    private void Running()
    {
        //달리기시 움크리기 해제
        if (isCrouch)
            Crouch();

        theGunConroller.CancelFineSight();//뛸때 정조준 모드 해제: <Part5_1_2GunController> CancelFineSight을 public으로 바꾼다


        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;//워크스피드였는데 달리기 속도로 바뀌면서 무브함수에서 달리기로 바뀌게 된다
    }

    //달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;//무브함수에는 다시 걷는걸로 바뀐다
    }
    
    //움직임 실행
    private void Move()
    {
        
        //이렇게 함수 안에서 선언 생성된 변수는 함수 호출이 끝나면 파괴되어 사라진다
        float mov_rotate_y = Input.GetAxisRaw("Horizontal");//1과-1이 안누르면 0리턴되면서 _moveDirX에 들어가게된다. Horizontal는 유니티에서 명시
        //11.10 _moveDirX -> mov_rotate_y로 교체
        float _moveDirZ = Input.GetAxisRaw("Vertical");                
        transform.Rotate(0, Time.deltaTime* mov_rotate_y* HeadturnSpeed, 0 ); //11.10 원래 Vector3 _moveHorizontal 변수 
        Vector3 _moveVertical = transform.forward * _moveDirZ;//(0,0,1) * 1
        //이제 하나로 합칠것이다
        //(1,0,0) (0,0,1) = (1,0,1) = 2 =>normalized; 하면 (0.5, 0, 0.5) = 1 (삼각함수 대각선방향으로 나감),(1이 나오도록 정규화):계산이 편리하고 유니티 권장
        Vector3 _velocity = (_moveVertical).normalized * applySpeed;//방향이 나온것에 속도까지 곱해줌 (applySpeed에 walkSpeed가 있어서 걸을것이다
        //11.10 _moveHorizontal 더해준거 없앰(회전으로 교체)
        //강체에다가 움직이게 만들것               //순간이동되는것을*대략 0.016
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    //움직임 체크
    private void MoveCheck()
    {
        
        if (!isRun && !isCrouch && isGround) //달리지 않고, 움크리지 않을때만 체크한다
        {
            //경사로가 있을수 있으니 내용을 수정한다
            //if (lastPos != transform.position)
            if(Vector3.Distance(lastPos, transform.position) >= 0.01f)
                isWalk = true;
            //0.01보다 작으면 안걷는걸로 간주한다(경사로에 살짝 미끄러진걸로는 걷지않는걸로 간주):이렇게 사소한 미스도 체크
            else
                isWalk = false;

            theCrosshair.WalkingAnimation(isWalk);
            //한프레임 돌고 lastPos에 값이 들어가면 그걸로 현재위치와 비교한다
            lastPos = transform.position;
            
        }
        
    }

    //좌우 케릭터 회전
    private void CharacterRotation()
    {
        //좌우 케릭터 회전
        //float _yRotation = camera_center.transform.rotation.y; //11.10 Input.GetAxisRaw("Mouse X");
        //벡터3를 얻어온다
        Vector3 _characterRotationY = new Vector3(0f, 0.001f, 0f)* lookSensitivity;
        //실제로 적용
        //MoveRotation은 MovePosion(벡터3)과 달리 쿼터니움이다. myRigid도 쿼터니움이다._characterRotationY값을 쿼터니움으로 변환시킨다
       myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));//여러방법이 있지만 리지드를 쓴만큼 맞춰주는것
              

    }
    //상하 카메라 회전
    private void CameraRotation()
    {
        //상하 카메라 회전
                                          //마우스는 X, Y밖에 없기 때문에
        //float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = lookSensitivity;//45도가 팍팍 움직이면 안되니 lookSensitivity로 민감도를 주기위해
        ///나온값을 실제로 적용//여기서 +는 FPS게임류 옵션에 있는 "마우스Y반전"과 관련있다
        currentCameraRotationX -= _cameraRotationX;  //카메라가 거꾸로 내려가면 반대로 -해주면 된다
        //리미트 적용을 위해       /가두기(어떤걸 가두냐?, -45 , +45) //많은 수가 들어와도 +-45로 고정
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);  
        
        //실제 카메라에 적용
        //카메라의 위치정보 혹은 로테이트정보 안에 로컬오일러앵글(x,y,z)
        //theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);//마우스는 위아래로 움직이는데 좌우로 움직이면 안되서


    }


    
}
