using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IEnemyState
{
    private readonly Enemy enemy;

    #region State Changes
    public AttackState(Enemy _enemy)
    {
        enemy = _enemy;
    }
    public void ToAttackState()
    {
    }
    public void ToChaseState()
    {
        if (enemy.AttackType == Enemy.RhythmAttackType.BEATS)
            attackFlag = false;

        enemy.HideAoEProjector();
        BattleManager.INSTANCE.RemoveAttackingEnemies(enemy.TargetPlayer, enemy);
        enemy.currentState = enemy.chaseState;
    }
    public void ToIdleState()
    {
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
        enemy.currentState = enemy.stunnedState;
    }
    public void ToWaitToAttackState()
    {
        enemy.currentState = enemy.waitState;

        if (enemy.AttackType == Enemy.RhythmAttackType.NOTES)
        {
            //enemy.currentState = enemy.waitState;

            enemy.hasAttacked = true;
            bool isAttackingAlone = BattleManager.INSTANCE.FinishedAttacking(enemy.TargetPlayer.PlayerNumber);
            if (isAttackingAlone)
                enemy.currentState.TargetAcquired();
        }
        else
        {
            
        }
    }
    #endregion
    public void TargetAcquired()
    {

    }

    public void UpdateState()
    {
        enemy.auxMesh.material.color = Color.red;

        if (enemy.AttackType == Enemy.RhythmAttackType.NOTES && enemy.attackNotesQty <= 0)
            ToWaitToAttackState();

        enemy.OnAttackEnter();
    }
    
    public void StateCountdown()
    {
        enemy.attackNotesQty--;
    }

    public void TargetUnreachable()
    {
        ToAlertState();
    }

    bool attackFlag = false;
    public void OnSongBeat()
    {
        if (!attackFlag)
        {
            attackFlag = true;
            enemy.NormalAttack();
        }
        else
        {
            attackFlag = false;
            enemy.HideAoEProjector();
            ToWaitToAttackState();
        }
    }
}
