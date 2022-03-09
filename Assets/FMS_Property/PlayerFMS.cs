using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFMS : MonoBehaviour
{
    public enum State
    { 
        idle, attack, Dead
    }
    State myPlayerstate;

    PlayerParams myParams;
    ZombieParams curzombParams;

    GameObject curZombie;
    // Start is called before the first frame update
    void Start()
    {
        myParams = GetComponent<PlayerParams>();
        myParams.InitParams();
        myPlayerstate = State.idle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
