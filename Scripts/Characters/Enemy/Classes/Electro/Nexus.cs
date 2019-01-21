using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nexus : Enemy
{

    Transform shootingPos;
    LineRenderer supportLine;
    Light supportLight;

    private Vector3 floatY;
    private float floatStrength = .5f;
    private float sizeGrowth = 0;
    private bool poweredLeader = false;

    protected override void Start()
    {
        shootingPos = transform.Find("ShootingPos").transform;
        supportLine = shootingPos.GetComponent<LineRenderer>();
        supportLight = shootingPos.GetComponent<Light>();

        currentHealthPoints = maxHealthPoints;

        level = 22;

        transform.position += Vector3.up / 2f;

        base.Start();
    }

    public override void Patrol()
    {
        base.Patrol();
        if (currentState == patrolState)
        {
            floatY.y = (Mathf.Sin(Time.time * 5f) * floatStrength);
            transform.position += floatY * Time.deltaTime;
        }
    }
    public void Follow()
    {
        if (isInSquad && SubordinateN == 1)
        {
            transform.position = new Vector3(leaderPos.position.x, transform.position.y, leaderPos.position.z) - leaderPos.forward - leaderPos.right;
            floatY.y = (Mathf.Cos(Time.time * 5f) * floatStrength);
            transform.position += floatY * Time.deltaTime;

        }
        else if (isInSquad && SubordinateN == 2)
        {
            transform.position = new Vector3(leaderPos.position.x, transform.position.y, leaderPos.position.z) - leaderPos.forward + leaderPos.right;
            floatY.y = (Mathf.Sin(Time.time * 5f) * floatStrength);
            transform.position -= floatY * Time.deltaTime;
        }
        else if (isInSquad && SubordinateN == 3)
        {
            transform.position = new Vector3(leaderPos.position.x, transform.position.y, leaderPos.position.z) - leaderPos.forward * 2;
            floatY.y = (Mathf.Cos(Time.time * 5f) * floatStrength);
            transform.position -= floatY * Time.deltaTime;
        }
    }
    public override void Approach(float speed)
    {
        transform.position += (transform.forward * speed * Time.deltaTime);
    }
    public override void Chase()
    {
        base.Chase();
    }
    public override void SquadChase()
    {
        Turn(leaderPos.rotation);
        transform.position = SquadFormation.Position(leaderPos, FormationType, SubordinateN);
    }
    public override void OnAttackEnter()
    {
        if (isLeader && !alliesNotified)
            LeaderAttack();

        base.OnAttackEnter();
    }
    public override void WaitToAttack()
    {
        if (isLeader && !alliesNotified)
            LeaderAttack();
        else if (SubordinateN > 0)
            SquadWaitToAttackPosition();
        else
            base.WaitToAttack();
    }
    public override void TargetAcquired()
    {
        float dist = 0;
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
            dist = 1 + hit.point.y;

        transform.position = new Vector3(transform.position.x, dist, transform.position.z);
    }

    public override int GetLevel()
    {
        return level;
    }

    public override void SquadPlacement()
    {
        transform.position = SquadFormation.Position(leaderPos, FormationType, SubordinateN);
        squadPosDir = transform.position - leaderPos.position;
        squadPosOffset = Vector3.Distance(transform.position, leaderPos.position);//Vector3.Magnitude(transform.position - leaderPos.position);
    }
    
    public void SquadWaitToAttackPosition()
    {
        Turn(leaderPos.rotation);

        if (!poweredLeader)
        {
            if (squadAllies[0] is Nexus)
                squadAllies[0].GetComponent<Nexus>().PowerUpLeader();

            supportLight.enabled = true;
            supportLine.enabled = true;

            poweredLeader = true;
        }
        supportLine.SetPosition(0, shootingPos.position);
        supportLine.SetPosition(1, leaderPos.Find("LockPosition").position);


    }

    public override void LeaderAttack()
    {
        foreach (Enemy e in squadAllies)
            e.currentState = e.waitState;

        alliesNotified = true;
    }

    public void PowerUpLeader()
    {
        attackDamage += 3;
        defencePower++;
        sizeGrowth += 0.25f;
        StartCoroutine(Grow());
    }

    public override void SetOrderToAttack(bool toAttack)
    {
        if (toAttack)
        {
            if (SubordinateN == 0)
                base.SetOrderToAttack(toAttack);
            else
                orderToAttack = SubordinateN + 1;
        }
        else
            base.SetOrderToAttack(toAttack);
    }

    private IEnumerator Grow()
    {
        while (transform.localScale.x - sizeGrowth < 1)
        {
            transform.localScale += Vector3.one * 0.2f * Time.deltaTime;
            yield return null;
        }
    }
}
