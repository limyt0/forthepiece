using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerhandOculus_Controll : MonoBehaviour
{
    public Transform armTransform;   //받아올 위치
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = armTransform.transform.position;
    }
}
