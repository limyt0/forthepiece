using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//Using 추가해줘야함

public class Part6_1HUD : MonoBehaviour
{
    [SerializeField]
    private Part5_2_1GunController theGunController;
    private Part5_1_1Gun currentGun;

    //필요하면 활성화 , 비활성화 만들것이다(차후 무기해제를 위해서)
    [SerializeField]
    private GameObject go_BulletHUD;

    //총알 텍스트
    [SerializeField]
    private Text[] text_Bullet;//Using 추가해줘야함
     
  
    void Update()
    {
        //총알 몇발남았는지 체크
        CheckBullet();
    }
    //매프레임마다 호출되면서 발사하면서 적용될것이다
    private void CheckBullet()
    {
        currentGun = theGunController.GetGun();
        //text는 숫자를 받지않고 string(문자만 받는다)만~
        text_Bullet[0].text = currentGun.currentBulletCount.ToString();
        text_Bullet[1].text = currentGun.reloadBulletCount.ToString();
        text_Bullet[2].text = currentGun.carryBulletCount.ToString();
    }
}
