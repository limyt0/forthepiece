using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerParams : CharacterParams
{
    public string name { get; set; }

    public override void InitParams()
    {
        name = "June";
        maxHP = 100;
        curHP = maxHP;
        gun_attack_power = 20;
        knife_attack_power = 30;

        isDead = false;
    }

    protected override void UpdateAfterReceiveAttack()
    {
        base.UpdateAfterReceiveAttack();
    }
}

