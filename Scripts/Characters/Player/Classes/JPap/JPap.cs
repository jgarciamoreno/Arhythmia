using Arhythmia;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JPap : Player
{
    private Player[] allies;

    #region Passive
    private SoulPooler soulPooler;
    private Collider[] soulsColl = new Collider[10];
    private float passiveRange = 10;
    #endregion
    #region Skill1
    private float assistRange = 20;

    private bool hasAllies;
    private Player weakestAlly;

    private int healingNotesQty = 0;
    private float healingTime;
    private float healingPower;
    private float baseHealingPower = 4;
    private bool isSuckingLife = false;
    #endregion
    #region Skill2
    private float pressedShieldTimer;
    private float retrieveShieldTimeThreshold = 1;
    private int shieldSoulConsumption = 25;
    private double shieldSelfDuration;
    private float shieldSelfProtectionPercentage = 0.25f;
    private float shieldSelfProtectionDmgBonus;
    private int shieldsDeployed = 0;
    private List<Player> alliesShielded = new List<Player>();
    private bool isShielding;
    public GameObject Shield;
    private GameObject shield;
    #endregion
    #region Skill3
    private bool isChargingAttack;
    public int chargeNotesQty;
    private float chargeTime;
    private int targetsQty = 0;
    private float dischargeRange = 40;
    private int damagePerShieldPoint = 3;
    private float accumulatedDmg = 0;
    private List<Enemy> enemiesInRange = new List<Enemy>();
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        maxHealthPoints = 75;
        currentHealthPoints = maxHealthPoints;
        maxShieldPoints = 200;
        currentShiledPoints = maxShieldPoints;
        attackDamage = 6;
        defencePower = 3;

        statusRecoverySpeed = 13;

        skillOneCooldownTime = 12;
        skillOneTimer = skillOneCooldownTime;
        skillTwoCooldownTime = 9;
        skillTwoTimer = skillOneCooldownTime;
        skillThreeCooldownTime = 20;
        skillThreeTimer = skillThreeCooldownTime;

        soulPooler = GetComponent<SoulPooler>();
        enemyDetect = GetComponent<EnemyDetection>();

        shield = Instantiate(Shield, transform);
        shield.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();

        soulPooler.SetupSouls();
        allies = new Player[GameManager.INSTANCE.players.Count - 1];
        if (allies.Length > 0)
        {
            foreach (Player p in FindObjectsOfType<Player>())
            {
                if (p != this)
                    allies[allies.Length - 1] = p;
            }

            hasAllies = true;
        }
        else
        {
            hasAllies = false;
        }

        weakestAlly = this;
    }

    public override void SetBeatBasedVariables(float beat)
    {
        base.SetBeatBasedVariables(beat);
        healingTime = beat * 8;
        shieldSelfDuration = beat * 16;
        chargeTime = beat * 8;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        int sq = Physics.OverlapSphereNonAlloc(transform.position, passiveRange, soulsColl, 1 << 16);
        if(sq > 0)
        {
            for (int i = 0; i < sq; i++)
            {
                Soul s = soulsColl[i].GetComponent<Soul>();
                if (s.isCollectable)
                    Passive(s);
            }
        }

        if (isSuckingLife && hasAllies)
        {
            for (int i = 0; i < allies.Length - 1; i++)
            {
                if (Vector3.Distance(transform.position, allies[i].transform.position) <= assistRange)
                {
                    if (allies[i].currentHealthPoints / allies[i].maxHealthPoints > weakestAlly.currentHealthPoints / weakestAlly.maxHealthPoints)
                    {
                        weakestAlly = allies[i];
                    }
                }
                else
                {
                    if (allies[i].Equals(weakestAlly))
                        weakestAlly = this;
                }
            }

            if (maxHealthPoints / currentHealthPoints > weakestAlly.currentHealthPoints / weakestAlly.maxHealthPoints)
                weakestAlly = this;
        }
    }

    public override void NormalAttack()
    {
        if (!isChargingAttack)
        {
            base.NormalAttack();

            if (isSuckingLife)
            {
                healingPower = damageDone < 3f ? baseHealingPower : baseHealingPower + (damageDone / baseHealingPower);
                healingPower = healingPower > 20 ? 20 : healingPower;
                Heal();
                if (healingNotesQty == 0)
                    isSuckingLife = false;
            }
        }
        else
        {
            chargeNotesQty--;
            ChargeDaSouls(1);
            if (chargeNotesQty == 0)
                ReleaseDaSouls(true);
        }
    }
    protected override void Defend(int defAccuracy)
    {
        base.Defend(defAccuracy);
        if (isChargingAttack)
        {
            chargeNotesQty--;
            ChargeDaSouls(defAccuracy);
            if (chargeNotesQty == 0)
                ReleaseDaSouls(true);
        }
    }
    public override float TakeDamage(float dmg, Vector3 hitPosition, TypeOfDamage type)
    {


        if (isShielding)
            return base.TakeDamage(dmg * shieldSelfProtectionPercentage, hitPosition, type);
        else
            return base.TakeDamage(dmg, hitPosition, type);        
    }

    private void Passive(Soul s)
    {
        s.Absorb();
    }

    public override void Miss()
    {
        if (isSuckingLife)
        {
            healingNotesQty--;
            if (healingNotesQty == 0)
                isSuckingLife = false;
        }
        else if (isChargingAttack)
        {
            ReleaseDaSouls(false);
        }
    }

    public void AbsorbSoul(float power)
    {
        currentShiledPoints += power;
        if (currentHealthPoints > maxShieldPoints)
            currentHealthPoints = maxShieldPoints;
        cameraMovement.UpdatePlayerBar();
    }
    public void RecoverSoul(float power)
    {
        maxShieldPoints += power;
        cameraMovement.UpdatePlayerBar();
    }

    public override void Skill2()
    {
        if (pressedShieldTimer <= 0)
        {
            pressedShieldTimer = Time.time;
        }
        else
        {
            if (Time.time - pressedShieldTimer < retrieveShieldTimeThreshold)
                Skill(2);
            else
                RetrieveSouls();

            pressedShieldTimer = 0;
        }
    }

    protected override void Skill(int skill)
    {
        switch (skill)
        {
            case 1:
                if(!isSuckingLife && isLockedOnEnemy)
                {
                    healingNotesQty = PatternSynch.CalculateNotesInTime(healingTime);
                    isSuckingLife = true;
                }

                break;
            case 2:
                if (!isShielding && currentShiledPoints > 0)
                {
                    isShielding = true;
                    DeployShields();
                    StartCoroutine(SelfShieldingTimer());
                }
                break;
            case 3:
                if (!isChargingAttack && currentShiledPoints > 0)
                {
                    if (FindEnemiesToAttack())
                    {
                        isChargingAttack = true;
                        selfSpecial = true;
                        chargeNotesQty = PatternSynch.CalculateNotesInTime(chargeTime);
                        cameraMovement.UpdatePlayerBar();
                    }
                }
                break;
        }
    }

    private void Heal()
    {
        lockedEnemy.TakeDamage(healingPower, transform.position, TypeOfDamage.NORMAL);
        weakestAlly.currentHealthPoints += healingPower;
        if (weakestAlly.currentHealthPoints > weakestAlly.maxHealthPoints)
            weakestAlly.currentHealthPoints = weakestAlly.maxHealthPoints;

        //weakestAlly.cameraMovement.OnPlayerHeal();
    }

    private void DeployShields()
    {
        shield.transform.position = transform.position + (Vector3.up * .5f);
        shield.SetActive(true);
        int def = Mathf.RoundToInt(currentShiledPoints >= shieldSoulConsumption ? shieldSoulConsumption : currentShiledPoints);
        currentShiledPoints -= def;
        shieldSelfProtectionDmgBonus = Mathf.RoundToInt((def * 6) / shieldSoulConsumption);
        additionalAttack += shieldSelfProtectionDmgBonus;

        if(currentShiledPoints > 0)
        {
            Collider[] coll = Physics.OverlapSphere(transform.position, assistRange, 1 << 12);
            for (int i = 0; i < coll.Length; i++)
            {
                Player p = coll[i].GetComponent<Player>();
                if (p != this && !alliesShielded.Contains(p))
                {
                    alliesShielded.Add(p);
                    def = Mathf.RoundToInt(currentShiledPoints >= shieldSoulConsumption ? shieldSoulConsumption : currentShiledPoints);
                    soulPooler.ShieldAlly(p, def);
                    currentShiledPoints -= def;
                    maxShieldPoints -= shieldSoulConsumption;
                    shieldsDeployed++;

                    if (currentShiledPoints <= 0)
                        break;
                }
            }
        }

        cameraMovement.UpdatePlayerBar();
    }

    private IEnumerator SelfShieldingTimer()
    {
        float timer = 0;
        while (timer < shieldSelfDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isShielding = false;
        shield.SetActive(false);
        additionalAttack -= shieldSelfProtectionDmgBonus;
    }

    private void RetrieveSouls()
    {

    }
    public void RetrieveSoulFrom(Player p)
    {
        alliesShielded.Remove(p);
    }

    private bool FindEnemiesToAttack()
    {
        Collider[] coll = Physics.OverlapSphere(transform.position, dischargeRange, 1 << 8);
        foreach (Collider c in coll)
        {
            if (!Physics.Raycast(transform.position, (c.transform.position - transform.position).normalized, dischargeRange, enemyDetect.obstacleMask))
            {
                targetsQty++;
                enemiesInRange.Add(c.GetComponent<Enemy>());
            }
        }
        if (targetsQty > 0)
        {
            StartCoroutine(SpawnDaSouls());
            return true;
        }

        return false;
    }
    private IEnumerator SpawnDaSouls()
    {
        float dmg = (currentShiledPoints * damagePerShieldPoint) / targetsQty;
        float shieldConsumptionPerSoul = currentShiledPoints / targetsQty;
        for (int i = 0; i < targetsQty; i++)
        {
            soulPooler.SpawnAttackSoul(enemiesInRange[i], dmg);
            currentShiledPoints -= shieldConsumptionPerSoul;
            cameraMovement.UpdatePlayerBar();
            yield return new WaitForSeconds(0.1f);
        }

        isSkillTwoOnCooldown = true;
    }
    private void ChargeDaSouls(float charge)
    {
        accumulatedDmg += 1 / charge;
        soulPooler.ChargeAttackSoul(accumulatedDmg);
    }
    private void ReleaseDaSouls(bool success)
    {
        if (success)
        {

            StartCoroutine(soulPooler.ShootSouls(true));
        }
        else
            StartCoroutine(soulPooler.ShootSouls(false));

        accumulatedDmg = 0;
        targetsQty = 0;
        enemiesInRange.Clear();
        isChargingAttack = false;
        selfSpecial = false;
        //isSkillThreeOnCooldown = true;
    }

    protected override void OnEnemyKilled(Enemy e, Player p)
    {
        base.OnEnemyKilled(e, p);
        if (enemiesInRange.Contains(e))
            enemiesInRange.Remove(e);
    }
}
