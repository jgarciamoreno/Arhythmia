using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : IEnemyState
{
    private readonly Enemy enemy;
    private bool pathRequested = false;

    public SearchState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    #region
    public void ToAttackState()
    {
        throw new NotImplementedException();
    }
    public void ToChaseState()
    {
        throw new NotImplementedException();
    }
    public void ToIdleState()
    {
        throw new NotImplementedException();
    }
    public void ToAlertState()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
    #endregion
    public void ToWaitToAttackState()
    {
        throw new NotImplementedException();
    }

    public void TargetAcquired()
    {

    }

    public void UpdateState()
    {
        if(!pathRequested)
        {
            pathRequested = true;
            enemy.RequestPath(enemy.targetPosOld);
        }
        else
        {
            enemy.Search();
        }
    }

    public void StateCountdown()
    {

    }

    public void TargetUnreachable()
    {
        ToAlertState();
    }

    public void OnSongBeat()
    {
    }
}
