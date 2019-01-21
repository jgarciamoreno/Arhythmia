using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pusher : Enemy {

    private Projector[] projectors = new Projector[3];
    private AoECollider aoeColl;

    protected override void Start()
    {
        currentHealthPoints = maxHealthPoints;
        level = 40;

        projectors = GetComponentsInChildren<Projector>();
        aoeColl = GetComponentInChildren<AoECollider>();
        base.Start();
    }

    protected override void OnEnable()
    {
        BeatSynch.OnSongBeat += BeatSynch_OnSongBeat;
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        BeatSynch.OnSongBeat -= BeatSynch_OnSongBeat;
        base.OnDisable();
    }
    protected override void BeatSynch_OnSongBeat()
    {
        currentState.OnSongBeat();
    }

    public override void Patrol()
    {
        base.Patrol();
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
        base.OnAttackEnter();
    }
    public override void WaitToAttack()
    {
        base.WaitToAttack();
    }
    public override void TargetAcquired()
    {
    }
    public override void NormalAttack()
    {
        foreach (Player p in aoeColl.playersInRange)
        {
            p.ApplyForce((p.transform.position - transform.position).normalized * 20 + Vector3.up * 7.5f, true);
            p.TakeDamage(attackDamage, transform.position, TypeOfDamage.NORMAL);
        }
    }
    public override void Attack(bool successful)
    {

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

    public override void LeaderAttack()
    {
        foreach (Enemy e in squadAllies)
            e.currentState = e.waitState;

        alliesNotified = true;
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

    int projectorN = 0;
    public override void ChargeAoEProjector()
    {
        projectors[projectorN].enabled = true;
        projectorN++;
    }

    public override void HideAoEProjector()
    {
        projectorN = 0;
        foreach (Projector p in projectors)
            p.enabled = false;
    }
}
