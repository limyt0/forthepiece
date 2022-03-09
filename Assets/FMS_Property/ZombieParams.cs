using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieParams : CharacterParams
{
    public string name;
    public int exp { get; set; }
    public int rewardValue { get; set; }
    public override void InitParams() {
        name = "Zombie";
        maxHP = 200;
        curHP = maxHP;
        gun_attack_power = 30;
        knife_attack_power = 30;


    }

    protected override void UpdateAfterReceiveAttack() {
        base.UpdateAfterReceiveAttack();
    }

}
