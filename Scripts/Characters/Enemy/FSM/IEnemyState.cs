using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyState
{
    void UpdateState();

    void ToIdleState();
    void ToPatrolState();
    void ToChaseState();
    void ToSearchLostPlayerState();
    void ToAttackState();
    void ToWaitToAttackState();
    void ToAlertState();
    void ToProtectState();
    void ToStunnedState();

    void TargetAcquired();
    void TargetUnreachable();

    void StateCountdown();

    void OnSongBeat();
}
