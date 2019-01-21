using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertState : IEnemyState
{
    private readonly Enemy enemy;

    public int AlertLevel;

    public AlertState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    #region StateChanges
    public void ToAttackState()
    {
        enemy.currentState = enemy.attackState;
    }
    public void ToChaseState()
    {
        enemy.currentState = enemy.chaseState;
    }
    public void ToIdleState()
    {
        throw new NotImplementedException();
    }
    public void ToAlertState()
    {
        //Can't transition to same state
    }
    public void ToPatrolState()
    {
        throw new NotImplementedException();
    }
    public void ToProtectState()
    {
        throw new NotImplementedException();
    }
    public void ToSearchLostPlayerState()
    {
        throw new NotImplementedException();
    }
    public void ToStunnedState()
    {
        enemy.currentState = enemy.stunnedState;
    }
    public void ToWaitToAttackState()
    {
        enemy.currentState = enemy.waitState;
    }
    #endregion
    public void TargetAcquired()
    {
        enemy.SetOrderToAttack(true);
        BattleManager.INSTANCE.OnPlayerSpotted(enemy, enemy.TargetPlayer);
        if (AlertLevel == 0)
            ToAttackState();
        else if (AlertLevel == 1)
            ToWaitToAttackState();
        else
            ToChaseState();
    }

    public void UpdateState()
    {
        enemy.auxMesh.material.color = Color.yellow;
        enemy.Turn(enemy.playerPosition);

        //Add timer to transition to other state
    }

    public void StateCountdown()
    {

    }

    public void TargetUnreachable()
    {
        throw new NotImplementedException();
    }

    public void OnSongBeat()
    {
        throw new NotImplementedException();
    }
}
