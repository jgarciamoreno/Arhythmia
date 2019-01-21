using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IEnemyState
{
    private readonly Enemy enemy;

    public IdleState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    #region
    public void ToAttackState()
    {
        //Transition NOT possible
    }
    public void ToChaseState()
    {
        BattleManager.INSTANCE.OnPlayerSpotted(enemy, enemy.TargetPlayer);
        enemy.currentState = enemy.chaseState;
    }
    public void ToIdleState()
    {
        //Transition NOT possible
    }
    public void ToAlertState()
    {
        //Transition NOT possible
    }
    public void ToPatrolState()
    {
        // ONLY IF ALLOWED //
    }
    public void ToProtectState()
    {
    }
    public void ToSearchLostPlayerState()
    {
        //Transition NOT possible
    }
    public void ToStunnedState()
    {
        enemy.currentState = enemy.stunnedState;
    }
    public void ToWaitToAttackState()
    {
        //Transition NOT possible
    }
    #endregion
    public void TargetAcquired()
    {
        ToChaseState();
    }

    public void UpdateState()
    {
        
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
    }
}
