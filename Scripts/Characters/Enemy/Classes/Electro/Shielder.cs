using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shielder : Enemy {

    private GameObject shield;

    protected override void Start()
    {
        shield = transform.Find("SphereShield").gameObject;
        shield.SetActive(false);

        currentHealthPoints = maxHealthPoints;

        level = 16;

        transform.position += Vector3.up / 2f;

        base.Start();
    }

    public override void NormalAttack()
    {
        base.NormalAttack();
    }

    public override void OnAttackEnter()
    {
        Turn(TargetPlayer.transform.position);

        //if (!shield.activeSelf)
        //    ShieldUp();
    }
    public override void WaitToAttack()
    {
        Turn(TargetPlayer.transform.position);

        if (!shield.activeSelf)
            ShieldUp();
    }

    protected override void Die()
    {
        ShieldDown();
        base.Die();
    }

    private void ShieldUp()
    {
        shield.transform.parent = null;
        shield.SetActive(true);
    }
    private void ShieldDown()
    {
        shield.transform.parent = transform;
        shield.SetActive(false);
    }

    public override int GetLevel()
    {
        return level;
    }
}
