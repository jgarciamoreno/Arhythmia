using UnityEngine;
using System.Collections;
using System;

public class Warrior : Player
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        maxHealthPoints = 500;
        currentHealthPoints = 500;
        attackDamage = 65;
        defencePower = 15;
        
        enemyDetect = GetComponent<EnemyDetection>();
    }

    public override void Miss()
    {

    }
}
