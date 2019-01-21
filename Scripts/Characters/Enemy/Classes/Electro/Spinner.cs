using UnityEngine;
using System.Collections;
using System;

public class Spinner : Enemy
{

    protected override void Start()
    {
        maxHealthPoints = 150;
        currentHealthPoints = maxHealthPoints;
        attackDamage = 3.5f;
        defencePower = 3.5f;

        normalMovementSpeed = 3;
        chaseMovementSpeed = 8.5f;
        onLockMovementSpeed = 3.5f;
        turnSpeed = 15;

        attackRange = 5f;

        level = 35;
        aggro = 21;

        transform.position += Vector3.up / 2f;

        base.Start();
    }

    public override int GetLevel()
    {
        return level;
    }
}
