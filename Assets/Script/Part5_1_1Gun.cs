using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
//accuracy:정확성,Rate:속도, current:현재의, carry:들고있다(휴대), retroaction:반동, FineSight:정조준, muzzleFlash:총구 섬광
public class Part5_1_1Gun : MonoBehaviour
{

    public string gunName;//총의 이름
    public float range;//사정 거리(너무 멀리나가면 안되니 적당하게)
    public float accuracy;//정확도(총마다 틀릴수 있으니)
    public float fireRate;//연사속도(높을수록 연사가 느려지게)
    public float reloadTime;//재장전 속도

    public int damage;//총의 데미지

    public int reloadBulletCount;//총알의 재장전 갯수
    public int currentBulletCount;//현재 탄알집에 남아있는 총알의 갯수
    public int maxBulletCount;//최대 소유 가능 총알 개수
    public int carryBulletCount;//현재 소유하고 있는 총알 개수

    public float retroactionForce;//반동세기
    public float retroActionFineSightForce;//정조준시의 반동 세기

    public Vector3 fineSightOriginPos;//정조준할때 정해진 위치

    public float fineSightRotation;

    public Animator anim;//에니메이션도 구현되어 있으니 선언, p.s Animation이랑 혼돈해서 쓰지마라

    public ParticleSystem muzzleFlash; //파티클 시스템 가져오기 총구섬광

    public AudioClip fire_Sound;//오디오

    
}
