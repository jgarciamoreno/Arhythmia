using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolSate : IEnemyState
{
    private readonly Enemy enemy;

    public float stoppingDst = 10;
    private bool pathRequested = false;

    #region StateChanges
    public PatrolSate(Enemy _enemy)
    {
        enemy = _enemy;
    }

    public void ToAttackState()
    {
        //Not possible
    }
    public void ToChaseState()
    {
        BattleManager.INSTANCE.OnPlayerSpotted(enemy, enemy.TargetPlayer);
        enemy.currentState = enemy.chaseState;
    }
    public void ToIdleState()
    {
        enemy.currentState = enemy.idleState;
    }
    public void ToAlertState()
    {
        enemy.currentState = enemy.alertState;
    }
    public void ToPatrolState()
    {
        Debug.LogError("Can't transition to same state");
    }
    public void ToProtectState()
    {
        //To discuss
    }
    public void ToSearchLostPlayerState()
    {
        Debug.LogError("Transition not allowed");
    }
    public void ToStunnedState()
    {
        enemy.currentState = enemy.stunnedState;
    }
    public void ToWaitToAttackState()
    {
        Debug.LogError("Transition not allowed");
    }
    #endregion
    public void TargetAcquired()
    {
        enemy.TargetAcquired();
        ToChaseState();
    }

    public void UpdateState()
    {
        if (Time.timeSinceLevelLoad > .3f && !pathRequested && enemy.Role != enemy.Subordinate)
        {
            pathRequested = true;
            enemy.RequestPath(enemy.patrolWaypoints[0].position);
        }
        else if (pathRequested || enemy.Role == enemy.Subordinate)
            enemy.Role.Patrol(enemy);
    }

    public void StateCountdown()
    {

    }

    public void TargetUnreachable()
    {
        ToIdleState();
    }

    public void OnSongBeat()
    {
        throw new NotImplementedException();
    }
}
