using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitState : IEnemyState
{
    private readonly Enemy enemy;

    private bool counting = false;

    private bool delayAttackTurn = false;
    private int delayNotes = 0;
    private int beatCount = 0;

    public WaitState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    #region State Changes
    public void ToAttackState()
    {
        if (enemy.AttackType == Enemy.RhythmAttackType.NOTES)
        {
            enemy.attackNotesQty = enemy.TargetPlayer.PatternSynch.CalculateNotesInTime(enemy, enemy.attackTime);
        }

        enemy.currentState = enemy.attackState;
    }
    public void ToChaseState()
    {
        beatCount = 0;
        BattleManager.INSTANCE.RemoveAttackingEnemies(enemy.TargetPlayer, enemy);
        enemy.currentState = enemy.chaseState;
    }
    public void ToIdleState()
    {
        //Transition not possible
    }
    public void ToAlertState()
    {
        throw new NotImplementedException();
    }
    public void ToPatrolState()
    {
        
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
        enemy.StopAllCoroutines();//Maybe stop only countdown coroutine
        enemy.currentState = enemy.stunnedState;
    }
    public void ToWaitToAttackState()
    {
        //Transition not possible
    }
    #endregion
    public void TargetAcquired()
    {
        int aux = enemy.TargetPlayer.PatternSynch.CalculateNotesInTime(enemy, enemy.attackTime);
        counting = true;

        enemy.notesBeforeAttackQty = aux;
    }

    public void UpdateState()
    {
        enemy.auxMesh.material.color = Color.green;

        enemy.WaitToAttack();

        if (counting && enemy.notesBeforeAttackQty <= 0)
        {
            counting = false;
            ToAttackState();
            BattleManager.INSTANCE.BeginAttack(enemy.TargetPlayer.PlayerNumber);
        }
    }

    public void StateCountdown()
    {
        if (counting)
        {
            if (delayAttackTurn)
            {
                delayNotes--;
                delayAttackTurn = false;
            }
            else
            {
                enemy.notesBeforeAttackQty--;
            }
        }
    }

    public void TargetUnreachable()
    {
        ToAlertState();
    }

    public void OnSongBeat()
    {
        beatCount++;
        if (enemy.beatsToAttack - beatCount < 3)
        {
            enemy.ChargeAoEProjector();
        }
        if (beatCount >= enemy.beatsToAttack)
        {
            beatCount = 0;
            ToAttackState();
        }
    }
}
