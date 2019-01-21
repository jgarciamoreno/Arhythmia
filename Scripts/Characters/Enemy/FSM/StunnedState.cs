using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunnedState : IEnemyState
{
    private readonly Enemy enemy;

    public StunnedState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    #region
    public void ToAttackState()
    {
        enemy.currentState = enemy.attackState;
    }
    public void ToChaseState()
    {
        //Transition not possible
    }
    public void ToIdleState()
    {
        //Transition not possible
    }
    public void ToAlertState()
    {
        enemy.currentState = enemy.alertState;
    }
    public void ToPatrolState()
    {
        //Transition not possible
    }
    public void ToProtectState()
    {
        throw new NotImplementedException();
    }
    public void ToSearchLostPlayerState()
    {
        //Transition not possible
    }
    public void ToStunnedState()
    {
        //Can't transition to same state
    }
    public void ToWaitToAttackState()
    {
        enemy.currentState = enemy.waitState;
    }
    #endregion
    public void TargetAcquired()
    {
        
    }

    public void UpdateState()
    {
        enemy.auxMesh.material.color = Color.magenta;
    }

    public IEnumerator Countdown(int type, float time)
    {
        float i = 0;
        while (i < time)
        {
            i += Time.deltaTime;
            yield return null;
        }

        if (enemy.TargetPlayer == null)
            ToAlertState();
        else
        {
            if (type == 0)
                ToAttackState();
            else if (type == 1)
                ToWaitToAttackState();
            else
                Debug.LogError("Type of stun not contemplated.");
        }
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
