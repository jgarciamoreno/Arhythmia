using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager INSTANCE { get; set; }

    private List<Enemy> enemiesAttackingPlayer1;
    private List<Enemy> enemiesAttackingPlayer2;
    private List<Enemy> enemiesAttackingPlayer3;
    private List<Enemy> enemiesAttackingPlayer4;

    private List<Enemy> beatEnemiesAttackingPlayer1;
    private List<Enemy> beatEnemiesAttackingPlayer2;
    private List<Enemy> beatEnemiesAttackingPlayer3;
    private List<Enemy> beatEnemiesAttackingPlayer4;
    
    private List<Enemy> enemies = new List<Enemy>();
    private List<Player> players = new List<Player>();

    public delegate void onEnemyKilled(Enemy e, Player p);
    public static event onEnemyKilled OnEnemyKilled;

    private bool inBattle;
    private bool InBattle
    {
        get { return inBattle; }
        set
        {
            inBattle = value;
            if (inBattle)
                GameManager.INSTANCE.ToBattle();
            else
                GameManager.INSTANCE.ToOverworld();
        }
    }

    void Awake()
    {
        if (INSTANCE != null)
            Destroy(this);
        else
            INSTANCE = this;
    }

    private void OnEnable()
    {
        Player.onLock += Player_onLock;
    }
    private void OnDisable()
    {
        Player.onLock -= Player_onLock;
    }
    public void SetNumberOfLists(int players)
    {
        switch (players)
        {
            case 1:
                enemiesAttackingPlayer1 = new List<Enemy>();
                beatEnemiesAttackingPlayer1 = new List<Enemy>();
                break;
            case 2:
                enemiesAttackingPlayer2 = new List<Enemy>();
                beatEnemiesAttackingPlayer2 = new List<Enemy>();
                goto case 1;
            case 3:
                enemiesAttackingPlayer3 = new List<Enemy>();
                beatEnemiesAttackingPlayer3 = new List<Enemy>();
                goto case 2;
            case 4:
                enemiesAttackingPlayer4 = new List<Enemy>();
                beatEnemiesAttackingPlayer4 = new List<Enemy>();
                goto case 3;
        }
    }

    public void PlayerOnSelfLock(bool active)
    {
        if (active)
        {
            if (inBattle)
                InBattle = true;
        }
        else
            InBattle = false;
    }

    private void Player_onLock(Player playerInitiator, Enemy targetEnemy, bool active)
    {
        if (inBattle && !active) //If the game is already in battle and player is locking OFF
        {
            if (GetEnemiesAttackingPlayerList(playerInitiator.PlayerNumber).Count == 0) //Check if no enemies are attacking the player
            {
                players.Remove(playerInitiator); //Remove the player from the battle list
                if (players.Count == 0)
                    InBattle = false;
            }
        }
        else if (active) //If player is locking ON
        {
            if (!players.Contains(playerInitiator)) //Check if player is already in the battle list
                players.Add(playerInitiator);
            if (!enemies.Contains(targetEnemy) && targetEnemy != null) //Check if the enemy is already on the battle list
                enemies.Add(targetEnemy);

            if (!inBattle)
                InBattle = true;
        }
    }

    public void OnPlayerSpotted(Enemy enemyInitiator, Player playerSpotted)
    {
        if (inBattle)
        {
            if (!enemies.Contains(enemyInitiator))
                enemies.Add(enemyInitiator);

            if (!players.Contains(playerSpotted))
            {
                playerSpotted.BeingTargeted = true;
                players.Add(playerSpotted);
            }
        }
        else
        {
            enemies.Add(enemyInitiator);
            players.Add(playerSpotted);
            playerSpotted.BeingTargeted = true;
            InBattle = true;
        }
    }

    public void CharacterKilled(Character characterKilled, Player lockedEnemyAtMomentOfDeath)
    {
        if (characterKilled is Enemy)
        {
            if (OnEnemyKilled != null)
                OnEnemyKilled((Enemy)characterKilled, lockedEnemyAtMomentOfDeath);

            Enemy enemy = characterKilled as Enemy;
            enemies.Remove((Enemy)characterKilled);
            if (enemy.TargetPlayer != null)
                RemoveAttackingEnemies(lockedEnemyAtMomentOfDeath, (Enemy)characterKilled);

            if (enemies.Count == 0)
            {
                for (int i = 0; i < players.Count; i++)
                    players[i].EnemyKilled(true);

                players.Clear();
                InBattle = false;
            }
        }
        else
        {
            players.Remove((Player)characterKilled);
            if (enemies.Count == 0 && !inBattle)
                InBattle = false;
        }
    }

    private List<Enemy> GetEnemiesAttackingPlayerList(int i)
    {
        switch (i)
        {
            case 1:
                return enemiesAttackingPlayer1;
            case 2:
                return enemiesAttackingPlayer2;
            case 3:
                return enemiesAttackingPlayer3;
            case 4:
                return enemiesAttackingPlayer4;
        }

        return null;
    }
    private List<Enemy> GetBeatEnemiesAttackingPlayerList(int i)
    {
        switch (i)
        {
            case 1:
                return beatEnemiesAttackingPlayer1;
            case 2:
                return beatEnemiesAttackingPlayer2;
            case 3:
                return beatEnemiesAttackingPlayer3;
            case 4:
                return beatEnemiesAttackingPlayer4;
        }

        return null;
    }

    public Enemy GetNextEnemyToLock(int list)
    {
        Enemy e = null;
        switch (list)
        {
            case 1:
                if (enemiesAttackingPlayer1.Count > 0)
                    e = enemiesAttackingPlayer1[0];
                else if (beatEnemiesAttackingPlayer1.Count > 0)
                    e = beatEnemiesAttackingPlayer1[0];
                break;
            case 2:
                if (enemiesAttackingPlayer2.Count > 0)
                    e = enemiesAttackingPlayer2[0];
                else if(beatEnemiesAttackingPlayer2.Count > 0)
                    e = beatEnemiesAttackingPlayer2[0];
                break;
            case 3:
                if (enemiesAttackingPlayer3.Count > 0)
                    e = enemiesAttackingPlayer3[0];
                else if (beatEnemiesAttackingPlayer3.Count > 0)
                    e = beatEnemiesAttackingPlayer3[0];
                break;
            case 4:
                if (enemiesAttackingPlayer4.Count > 0)
                    e = enemiesAttackingPlayer4[0];
                else if (beatEnemiesAttackingPlayer4.Count > 0)
                    e = beatEnemiesAttackingPlayer4[0];
                break;
        }
        return e;
    }

    #region Order Management
    public int AddAttackingEnemies(Player _p, Enemy _e)
    {
        if (_e.AttackType == Enemy.RhythmAttackType.NOTES)
        {
            List<Enemy> auxList = GetEnemiesAttackingPlayerList(_p.PlayerNumber);

            return auxList.IndexOf(_e);
        }
        else //We know that if it's not a NOTES type it is a BEAT type
        {
            List<Enemy> auxList = GetBeatEnemiesAttackingPlayerList(_p.PlayerNumber);

            if (!auxList.Contains(_e))
                auxList.Add(_e);

            return -1;
        }
    }
    public void RemoveAttackingEnemies(Player _p)
    {
        if (GetEnemiesAttackingPlayerList(_p.PlayerNumber).Count == 0)
            _p.BeingTargeted = false;
    }
    public void RemoveAttackingEnemies(Player _p, Enemy _e)
    {
        List<Enemy> auxList = GetEnemiesAttackingPlayerList(_p.PlayerNumber);
        if (!auxList.Contains(_e))
            return;

        int i = auxList.IndexOf(_e);
        auxList.Remove(_e);
        enemies.Remove(_e);
        if (i < auxList.Count - 1)
            SortOrderAfterRemoval(auxList, i);
    }
    public void BeginAttack(int list)
    {
        if (GetEnemiesAttackingPlayerList(list).Count > 1)
        {
            Enemy e = GetEnemiesAttackingPlayerList(list)[1];
        }
    }
    public bool FinishedAttacking(int list)
    {
        if (GetEnemiesAttackingPlayerList(list).Count > 1)
        {
            List<Enemy> auxList = GetEnemiesAttackingPlayerList(list);
            Enemy auxE = auxList[0];
            auxList[0] = auxList[1];
            auxList[1] = auxE;
            auxList[0].ReorderToAttack(0, 0);
            auxList[1].ReorderToAttack(1, 0);

            if (auxList[0].hasAttacked)
            {
                auxList[0].hasAttacked = false;
                auxList[1].hasAttacked = false;
            }

            return false;
        }
        else
            return true;
    }
    private void SortOrderAfterRemoval(List<Enemy> auxList, int index)
    {
        for (int i = index; i < auxList.Count; i++)
            auxList[i].ReorderToAttack(auxList.IndexOf(auxList[i]), 0);
    }
    #endregion

    public void ToMainMenu()
    {
        enemies.Clear();
        players.Clear();
        inBattle = false;
    }
}
