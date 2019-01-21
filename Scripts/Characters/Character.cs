using UnityEngine;
using System;
using System.Collections;

public abstract class Character : MonoBehaviour
{
    #region Stats
    public float maxHealthPoints;
    public float currentHealthPoints;
    public float maxShieldPoints;
    public float currentShiledPoints;
    protected float additionalHP;
    public float additionalShield;
    protected float additionalAttack;
    protected float additionalDefence;
    [SerializeField] protected float attackDamage;
    public Arhythmia.BeatValue attackSpeed;
    protected float critMultiplier;
    [SerializeField] protected float defencePower;
    public bool isInBattleStance = false;
    #endregion
    #region LockOn
    public Enemy lockedEnemy;
    protected bool selfSpecial = false;
    protected bool beingTargeted = false;
    public bool isLockedOnEnemy = false;
    #endregion
    #region References
    protected Locomotion locomotion;
    public InputHandler inputHandler;
    public int PlayerNumber { get; set; }
    protected CameraMovement cameraMovement;
    #endregion

    /// REFACTOR TIME!!!!
    #region TO REFACTOR
    protected float currentParalysisState;
    protected float currentBurnState;
    protected float currentArmorBreakState;
    
    private float brnDmg;

    public enum TypeOfDamage { NORMAL, TRUE, PIERCE }
    public TypeOfDamage damageType;

    [Range(0, 10)]
    [SerializeField] protected float paralysisResistance;
    [Range(0, 10)]
    [SerializeField] protected float burnResistance;
    [Range(0, 10)]
    [SerializeField] protected float armorBreakResistance;
    protected bool isParalyzed;
    protected bool isBurned;
    protected bool isBroken;
    protected bool shieldDisabled;
    #endregion

    [SerializeField] protected float statusRecoverySpeed;
    protected float recoveryCoolDown = 3f;
    protected bool recoverOnCD;
    protected Coroutine StatusRecoveringCoroutine;

    protected bool dead;

    protected Vector3 incomingHitDirection;

    protected object empoweringObject;
    
    [SerializeField] protected float normalMovementSpeed;
    [SerializeField] protected float chaseMovementSpeed;
    [SerializeField] protected float onLockMovementSpeed;

    protected virtual void Start()
    {
        locomotion = GetComponent<Locomotion>();
    }

    protected virtual void OnEnable()
    {
        brnDmg = Mathf.Max(maxHealthPoints * 0.025f, 10);
        dead = false;
        BeatSynch.OnSongBeat += BeatSynch_OnSongBeat;
    }
    protected virtual void OnDisable()
    {
        BeatSynch.OnSongBeat -= BeatSynch_OnSongBeat;
    }
    protected virtual void BattleManager_enemyKilled(Enemy enemy, bool battleEnded) { }

    protected float damageDone;
    public virtual float TakeDamage(float damage, Vector3 hitPosition, TypeOfDamage type)
    {
        if (shieldDisabled)
            type = TypeOfDamage.PIERCE;

        switch (type)
        {
            case TypeOfDamage.NORMAL:
                damage -= defencePower + additionalDefence;
                break;
            case TypeOfDamage.TRUE:
                break;
            case TypeOfDamage.PIERCE:
                damage -= defencePower + additionalDefence;
                break;
        }
        int dmg = Mathf.RoundToInt(damage);

        if (damage < 0)
            damage = 0;
        else if (type != TypeOfDamage.PIERCE)
            DamageShield(dmg);
        else
            DamageHP(dmg);

        if (!recoverOnCD)
        {
            if(StatusRecoveringCoroutine != null)
            {
                StopCoroutine(StatusRecoveringCoroutine);
                StatusRecoveringCoroutine = null;
            }

            StartCoroutine(StatusCoolDown());
        }
        else
            recoveryCoolDown = 3f;

        incomingHitDirection = hitPosition - transform.position;

        return damage;
    }
    protected virtual void DamageShield(int dmg)
    {
        if (additionalShield > 0)
        {
            additionalShield -= dmg;
            if (additionalShield <= 0)
            {
                additionalShield = 0;
                if (typeof(IReturnable).IsAssignableFrom(typeof(Soul)))
                {
                    IReturnable r = (IReturnable)empoweringObject;
                    r.ReturnObject();
                }
            }
        }
        else if (currentShiledPoints > 0)
        {
            currentShiledPoints -= dmg;
            if (currentShiledPoints < 0)
                currentShiledPoints = 0;
        }
        else
            DamageHP(dmg);
    }
    protected virtual void DamageHP(int dmg)
    {
        if (additionalHP > 0)
        {
            additionalHP -= dmg;
            if (additionalHP < 0)
                additionalHP = 0;
        }
        else
        {
            currentHealthPoints -= dmg;
            if (currentHealthPoints < 0)
                currentHealthPoints = 0;
        }
    }

    public virtual void AddHP(int hp)
    {
        additionalHP += hp;
    }
    public virtual void AddShield(object _empoweringObject, float shield)
    {
        empoweringObject = _empoweringObject;
        additionalShield += shield;
    }
    public virtual void AddAttack(int atk)
    {
        additionalAttack += atk;
    }
    public virtual void AddDefence(int def)
    {
        if (!isBroken)
            additionalDefence += def;
    }

    public virtual void Paralyze(int pwr)
    {
        if (!isParalyzed)
        {
            currentParalysisState += pwr - paralysisResistance;
            if (currentParalysisState >= 100)
            {
                currentParalysisState = 100;
                StartCoroutine(ParalysisRecover());
                isParalyzed = true;
            }
        }
    }
    public virtual void Burn(int pwr)
    {
        if (!isBurned)
        {
            currentBurnState += pwr - burnResistance;
            if (currentBurnState >= 100)
            {
                currentBurnState = 100;
                StartCoroutine(BurnRecover());
                isBurned = true;
            }
        }
    }
    public virtual void BreakDef(int pwr)
    {
        if (!isBroken)
        {
            currentArmorBreakState += pwr - armorBreakResistance;
            if (currentArmorBreakState >= 100)
            {
                currentArmorBreakState = 100;
                additionalDefence = -defencePower;
                StartCoroutine(BreakRecover());
                isBroken = true;
            }
        }
    }

    public void EnableShield()
    {
        shieldDisabled = false;
    }
    public void DisableShield()
    {
        DisableShield(0);
    }
    public void DisableShield(float time)
    {
        shieldDisabled = true;
        if (time > 0)
            StartCoroutine(DisabledShieldCountdown(time));
    }

    protected virtual void Die()
    {
        gameObject.SetActive(false);
    }
    public bool IsDead()
    {
        return dead;
    }

    protected virtual IEnumerator StatusCoolDown()
    {
        recoverOnCD = true;
        while (recoveryCoolDown > 0)
        {
            recoveryCoolDown -= Time.deltaTime;
            yield return null;
        }

        recoverOnCD = false;
        recoveryCoolDown = 3;
        StatusRecoveringCoroutine = StartCoroutine("StatusRecover");
    }
    protected virtual IEnumerator StatusRecover()
    {
        int i = 0;
        bool pC = false;
        bool bC = false;
        bool aC = false;
        do
        {
            if (currentParalysisState > 0)
            {
                currentParalysisState -= statusRecoverySpeed * Time.deltaTime;
                if (!pC)
                {
                    i--;
                    pC = true;
                }
            }
            else if (currentParalysisState < 0)
            {
                currentParalysisState = 0;
                i++;
            }

            if (currentBurnState > 0)
            {
                currentBurnState -= statusRecoverySpeed * Time.deltaTime;
                if (!bC)
                {
                    i--;
                    bC = true;
                }
            }
            else if (currentBurnState < 0)
            {
                currentBurnState = 0;
                i++;
            }

            if (currentArmorBreakState > 0)
            {
                currentArmorBreakState -= statusRecoverySpeed * Time.deltaTime;
                if(!aC)
                {
                    i--;
                    aC = true;
                }
            }
            else if (currentArmorBreakState < 0)
            {
                currentArmorBreakState = 0;
                i++;
            }

            yield return null;
        } while (i < 0);

        StatusRecoveringCoroutine = null;
    }
    protected virtual IEnumerator ParalysisRecover()
    {
        while (currentParalysisState > 0)
        {
            currentParalysisState -= statusRecoverySpeed * Time.deltaTime;
            yield return null;
        }

        currentParalysisState = 0;
        isParalyzed = false;
    }
    protected virtual IEnumerator BurnRecover()
    {
        while (currentBurnState > 0)
        {
            currentBurnState -= statusRecoverySpeed * Time.deltaTime;
            yield return null;
        }

        currentBurnState = 0;
        isBurned = false;
    }
    protected virtual IEnumerator BreakRecover()
    {
        while (currentArmorBreakState > 0)
        {
            currentArmorBreakState -= statusRecoverySpeed * Time.deltaTime;
            yield return null;
        }

        currentArmorBreakState = 0;
        isBroken = false;
    }

    private IEnumerator DisabledShieldCountdown(float time)
    {
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        EnableShield();
    }

    protected virtual void BeatSynch_OnSongBeat()
    {
        if(isBurned)
            TakeDamage(brnDmg, transform.position, TypeOfDamage.TRUE);
    }

    //Commands
    public virtual void Move(Vector3 dir)
    {
        locomotion.horizontal = dir.x;
        locomotion.vertical = dir.z;
        Vector3 v = dir.z * cameraMovement.transform.forward;
        Vector3 h = dir.x * cameraMovement.transform.right;
        locomotion.Move(v, h);
    }
    public virtual void MoveCamera(Vector3 dir)
    {
        cameraMovement.vertical = dir.y;
        cameraMovement.horizontal = dir.x;
    }
    public virtual void ResetCamera() { cameraMovement.ResetCamera(transform.eulerAngles.y); }
    public virtual void Jump() { locomotion.Jump(); }
    public virtual void Run() { locomotion.Run(); }
    public virtual void ChangeStance() { }
    public virtual void Interact() { }
    public virtual void NormalAttack() { }
    public virtual void StunAttack() { }
    public virtual void BreakDefence() { }
    public virtual void AimHook(bool isActive) { }
    public virtual void DodgeHook() { }
    public virtual void Skill1() { }
    public virtual void Skill2() { }
    public virtual void Skill3() { }
    public virtual void Pause() { GameManager.INSTANCE.Pause(); }
    #region Targeting
    public virtual void LockOnEnemy()
    {
    }
    public virtual void SwitchTarget(int dir)
    {
    }
    #endregion

    public virtual void SetupCamera(CameraMovement cam)
    {
        cameraMovement = cam;
        GetComponent<EnemyDetection>().cam = transform;
        locomotion.SetupCamera(cam);
    }

    public virtual void SetBeatBasedVariables(float beat) { }

    public void Rag() { locomotion.Ragdoll(); }
}
