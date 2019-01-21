using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectState : IEnemyState
{
    private readonly Enemy enemy;

    public ProtectState(Enemy _enemy)
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
    #endregion
    public void ToStunnedState()
    {
        throw new NotImplementedException();
    }

    public void ToWaitToAttackState()
    {
        throw new NotImplementedException();
    }

    public void TargetAcquired()
    {

    }

    public void UpdateState()
    {
        throw new NotImplementedException();
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
