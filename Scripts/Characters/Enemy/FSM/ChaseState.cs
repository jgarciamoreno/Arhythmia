using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private readonly Enemy enemy;
    private bool pathRequested = false;

    private bool delayAttackTurn = false;
    private int delayNotes = 0;

    public ChaseState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    #region State Changes
    public void ToAttackState()
    {
        pathRequested = false;

        if (enemy.AttackType == Enemy.RhythmAttackType.NOTES)
        {
            int[] aux = enemy.TargetPlayer.PatternSynch.CalculateNotesInTime(enemy, enemy.attackTime, true);

            delayNotes = aux[0];
            enemy.attackNotesQty = aux[1];

            if (delayNotes == 0)
            {
                enemy.Role.NotifySquad(enemy);
                enemy.currentState = enemy.attackState;
            }
            else
            {
                delayAttackTurn = true;
            }
        }
    }
    public void ToChaseState()
    {
        //Can't transition to same state.
    }
    public void ToIdleState()
    {
    }
    public void ToAlertState()
    {
        pathRequested = false;
        enemy.currentState = enemy.alertState;
    }
    public void ToPatrolState()
    {
        pathRequested = false;
        enemy.currentState = enemy.patrolState;
    }
    public void ToProtectState()
    {
        pathRequested = false;
        enemy.currentState = enemy.protectState;
    }
    public void ToSearchLostPlayerState()
    {
        pathRequested = false;
        enemy.currentState = enemy.searchState;
    }
    public void ToStunnedState()
    {
        pathRequested = false;
        enemy.currentState = enemy.stunnedState;
    }
    public void ToWaitToAttackState()
    {
        pathRequested = false;
        enemy.currentState = enemy.waitState;
    }
    #endregion
    public void TargetAcquired()
    {
        if(enemy.AttackType == Enemy.RhythmAttackType.NOTES)
            enemy.SetOrderToAttack(true);
    }

    public void UpdateState()
    {
        if (!delayAttackTurn)
        {
            enemy.auxMesh.material.color = Color.gray;
            if (!pathRequested)
            {
                pathRequested = true;
                enemy.RequestPath(enemy.TargetPlayer.transform.position);
            }
            else
                enemy.Role.Chase(enemy);
        }
    }

    public void StateCountdown()
    {
        if (delayAttackTurn)
        {
            delayAttackTurn = false;
            enemy.Role.NotifySquad(enemy);
            enemy.currentState = enemy.attackState;
        }
    }

    public void TargetUnreachable()
    {
        ToAlertState();
    }

    public void OnSongBeat()
    {
    }
}
