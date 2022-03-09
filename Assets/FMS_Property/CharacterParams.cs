using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterParams : MonoBehaviour
{
    public float curHP { get; set; }
    public float maxHP { get; set; }
    public float gun_attack_power { get; set; }
    public float knife_attack_power { get; set; }
    public bool isDead { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        InitParams();
    }

    // Update is called once per frame
    void Update()
    {

    }


    public virtual void InitParams()
    {

    }

    

    public void SetAttackedDamage(float AttackPower)
    {
        curHP -= AttackPower;
        UpdateAfterReceiveAttack();
    }

    protected virtual void UpdateAfterReceiveAttack()
    {
        print(name + "'s HP: " + curHP);
    }
}
