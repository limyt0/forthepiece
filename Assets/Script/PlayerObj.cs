using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObj : MonoBehaviour
{
    //충돌감지를 하려면 둘 중 하나는  RigidBody가 있어야 한다.

    [SerializeField] GameObject _prefabEffHitNor;

    int _maxHP;
    int _nowHP;

    bool _isDead;
    

    public int Speed=3;




    public bool _ISDEATH //씬매니저에서 플레이어가 죽었는지 확인해야한다. 프로퍼티로 확인한다.
    {
        get 
        {
            return _isDead;
        }
    }

    void Start()
    {
        _maxHP = _nowHP = 10;
        _isDead = false;
    }


    void Update()
    {
        if (_isDead)
            return;
        if (Input.GetMouseButtonDown(0)) //마우스 클릭
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit))
            {
                if (hit.transform.tag == "Map")
                {
                    GameObject go = Instantiate(_prefabEffHitNor);
                    go.transform.position = hit.point;
                    Destroy(go, 2);
                }
            }
        }
        moveObjectFunc();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Zombie"))
        {
            _nowHP--;
            if(_nowHP<=0)
            {
                GetComponent<BoxCollider>().enabled = false; //죽으면 공격안당하기
                _isDead = true;
            }
        }
    }

    private float speed_move = 3.0f;
    private float speed_rota = 2.0f;

    void moveObjectFunc()
    {
        float keyH = Input.GetAxis("Horizontal");
        float keyV = Input.GetAxis("Vertical");
        keyH = keyH * speed_move * Time.deltaTime;
        keyV = keyV * speed_move * Time.deltaTime;
        transform.Translate(Vector3.right * keyH);
        transform.Translate(Vector3.forward * keyV);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.up * speed_rota * mouseX);
        transform.Rotate(Vector3.left * speed_rota * mouseY);
    }
}
