using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Arhythmia;

public abstract class Player : Character
{
    #region Delegates & Events
    public delegate void OnLock(Player playerInitiator, Enemy targetEnemy, bool active);
    public static event OnLock onLock;
    #endregion
    #region Interactable vars
    private float overlapRadius = 0.75f;
    private float nearestInteractable = Mathf.Infinity;
    private GameObject interactableObject = null;
    #endregion
    #region LockOn vars
    //Player locking ON enemy vars
    public const float lockDistance = 25f;
    //public float lockDistance;
    private bool inAttackRange;
    public bool BeingTargeted
    {
        get
        {
            return beingTargeted;
        }
        set
        {
            beingTargeted = value;
            OnBeingDetected(beingTargeted);
        }
    }
    #endregion
    #region Skill vars
    protected bool isSkillOneOnCooldown = false;
    protected bool isSkillTwoOnCooldown = false;
    protected bool isSkillThreeOnCooldown = false;

    protected float skillOneCooldownTime;
    protected float skillTwoCooldownTime;
    protected float skillThreeCooldownTime;

    protected float skillOneTimer;
    protected float skillTwoTimer;
    protected float skillThreeTimer;
    #endregion
    #region Properties
    private PatternSynch patternSynch;
    public PatternSynch PatternSynch
    {
        get { return patternSynch; }
        set
        {
            patternSynch = value;
            patternSynch.Player = this;
            patternController = patternSynch.GetComponent<PatternController>();
        }
    }
    public int PatternSynchNumber { get; set; }
    private PatternController patternController;
    protected bool isTargetable = true;
    public bool IsTargetable { get { return isTargetable; } }
    #endregion
    #region Scripts/Objects References
    protected Rigidbody rigidBody;
    protected EnemyDetection enemyDetect;
    #endregion
    #region Attack Colliders
    protected AttackHitBoxHandler normalAttackCollider;
    protected AttackHitBoxHandler stunAttackCollider;
    protected AttackHitBoxHandler defenceBreakCollider;
    #endregion

    protected override void Start()
    {
        base.Start();

        normalAttackCollider = transform.Find("NormalHitBox").GetComponent<AttackHitBoxHandler>();
        stunAttackCollider = transform.Find("StunHitBox").GetComponent<AttackHitBoxHandler>();
        defenceBreakCollider = transform.Find("BreakHitBox").GetComponent<AttackHitBoxHandler>();

        currentArmorBreakState = 0;
        currentBurnState = 0;
        currentParalysisState = 0;
    }
    protected override void OnEnable()
    {
        BattleManager.OnEnemyKilled += OnEnemyKilled;
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        BattleManager.OnEnemyKilled -= OnEnemyKilled;
        base.OnDisable();
    }

    protected void Update()
    {
        inputHandler.GetInput();
    }

    protected virtual void FixedUpdate()
    {
        locomotion.FixedTick(Time.deltaTime);
        DetectInteractableObject();
    }

    protected virtual void HandleSkillCooldowns()
    {
        if (isSkillOneOnCooldown)//Skill 1 CD
        {
            skillOneTimer --;
            cameraMovement.SetSkillOnCooldown(1, skillOneTimer / skillOneCooldownTime);

            if (skillOneTimer <= 0)
            {
                skillOneTimer = skillOneCooldownTime;
                isSkillOneOnCooldown = false;
            }
        }

        if (isSkillTwoOnCooldown)//Skill 2 CD
        {
            skillTwoTimer --;
            cameraMovement.SetSkillOnCooldown(2, skillTwoTimer / skillTwoCooldownTime);

            if (skillTwoTimer <= 0)
            {
                skillTwoTimer = skillTwoCooldownTime;
                isSkillTwoOnCooldown = false;
            }
        }

        if (isSkillThreeOnCooldown)//Skill 3 CD
        {
            skillThreeTimer --;
            cameraMovement.SetSkillOnCooldown(3, skillThreeTimer / skillThreeCooldownTime);

            if (skillThreeTimer <= 0)
            {
                skillThreeTimer = skillThreeCooldownTime;
                isSkillThreeOnCooldown = false;
            }
        }
    }

    private void DetectInteractableObject()
    {
        Collider[] objs = Physics.OverlapSphere(transform.position, overlapRadius, 1 << 11);

        if (objs.Length > 0)
        {
            foreach (Collider coll in objs)
            {
                float dist = Vector3.Distance(transform.position, coll.transform.position);
                if (dist < nearestInteractable)
                {
                    nearestInteractable = dist;
                    interactableObject = coll.gameObject;
                }
            }

            //interactUI.gameObject.SetActive(true);
        }
        else
        {
            nearestInteractable = Mathf.Infinity;
            //interactUI.gameObject.SetActive(false);
            interactableObject = null;
        }
    }

    private Enemy FindNearestEnemy()
    {
        Enemy enemyToLock = null;
        float distToNearestEnemy = enemyDetect.viewRadius;

        foreach (Transform enemyPos in enemyDetect.visibleTargets)
        {
            float dist = Vector3.Distance(transform.position, enemyPos.position);
            if (dist >= distToNearestEnemy)
                continue;

            distToNearestEnemy = dist;

            if (!enemyPos.GetComponent<Enemy>().IsDead())
                enemyToLock = enemyPos.GetComponent<Enemy>();
        }

        return enemyToLock;
    }

    public override void Interact()
    {
        base.Interact();
    }

    public override void Jump()
    {
        base.Jump();
    }

    protected virtual void Skill(int skill)
    {
    }
    public override void NormalAttack()
    {
        normalAttackCollider.Activate(attackDamage);
    }
    public override void StunAttack()
    {
    }
    public override void BreakDefence()
    {
    }
    protected void Attack()
    {
           damageDone = lockedEnemy.TakeDamage(attackDamage + additionalAttack, transform.position, TypeOfDamage.NORMAL);
    }
    protected virtual void Defend(int defAccuracy)
    {
        lockedEnemy.Attack(false);
        //Add block or evade animation
    }

    public virtual void Miss()
    {
    }
    public override float TakeDamage(float dmg, Vector3 hitPosition, TypeOfDamage type)
    {
        float damageTaken = 0;
        //if (!immune)
        //{
            damageTaken = base.TakeDamage(dmg, hitPosition, type);

            cameraMovement.OnPlayerTakeDamage();
        //}

        //if (currentHealthPoints <= 0)
        //    Die();

        return damageTaken;
        //Add dmg taken animation
    }

    protected virtual void OnBeingDetected(bool isBeingTargeted)
    {

    }

    protected virtual void OnEnemyKilled(Enemy e, Player p)
    {
        if (e == lockedEnemy && p != this)
            SwitchTarget(5);
    }
    public void EnemyKilled(bool battleEnded)
    {
        if (isLockedOnEnemy)
        {
            if (battleEnded)
                LockOnEnemy();
            else
                SwitchTarget(0);
        }
    }

    public void ApplyForce(Vector3 force, bool stun)
    {
        locomotion.ApplyExternalForce(force, stun);
    }

    public override void Skill1()
    {
        if (!isSkillOneOnCooldown)
            Skill(1);
    }
    public override void Skill2()
    {
        if (!isSkillTwoOnCooldown)
            Skill(2);
    }
    public override void Skill3()
    {
        if (isLockedOnEnemy && !isSkillThreeOnCooldown)
            Skill(3);
    }
    /* Battle Input */
    #region Button Press
    protected void BattleDownButton(bool pressed)
    {
        if (pressed)
        {
            //downPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.DOWN, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange || selfSpecial)
                        NormalAttack();
                    break;
                case 0:
                    break;
            }

        }
        else
        {
            //downPressTimer = (float)AudioSettings.dspTime - downPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.DOWN, TypeOfInput.RELEASE, downPressTimer);
            //downPressTimer = 0;
        }
    }
    protected void BattleRightButton(bool pressed)
    {
        if (pressed)
        {

            //rightPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.RIGHT, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:

                    break;
            }

        }
        else
        {
            //rightPressTimer = (float)AudioSettings.dspTime - rightPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.RIGHT, TypeOfInput.RELEASE, rightPressTimer);
            //rightPressTimer = 0;
        }
    }
    protected void BattleLeftButton(bool pressed)
    {
        if (pressed)
        {

            //leftPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.LEFT, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:
                    break;
            }
        }
        else
        {
            //leftPressTimer = (float)AudioSettings.dspTime - leftPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.LEFT, TypeOfInput.RELEASE, leftPressTimer);
            //leftPressTimer = 0;
        }
    }
    protected void BattleUpButton(bool pressed)
    {
        if (pressed)
        {
            //upPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.UP, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:

                    break;
            }
        }
        else
        {
            //upPressTimer = (float)AudioSettings.dspTime - upPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.UP, TypeOfInput.RELEASE, upPressTimer);
            //upPressTimer = 0;
        }
    }
    #endregion
    #region DPad Press
    protected void BattleDPadDown(bool pressed)
    {
        if (pressed)
        {
            //DPadDownPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.DOWN, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:

                    break;
            }
        }
        else if (!pressed)
        {
            //DPadDownPressTimer = (float)AudioSettings.dspTime - DPadDownPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.DOWN, TypeOfInput.RELEASE, DPadDownPressTimer);
            //DPadDownPressTimer = 0;
        }
    }
    protected void BattleDPadRight(bool pressed)
    {
        if (pressed)
        {
            //DPadRightPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.RIGHT, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:
                    break;
            }
        }
        else if (!pressed)
        {
            //DPadRightPressTimer = (float)AudioSettings.dspTime - DPadRightPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.RIGHT, TypeOfInput.RELEASE, DPadRightPressTimer);
            //DPadRightPressTimer = 0;
        }
    }
    protected void BattleDPadLeft(bool pressed)
    {
        if (pressed)
        {
            //DPadLeftPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.LEFT, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:

                    break;
            }
        }
        else if (!pressed)
        {
            //DPadLeftPressTimer = (float)AudioSettings.dspTime - DPadLeftPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.LEFT, TypeOfInput.RELEASE, DPadLeftPressTimer);
            //DPadLeftPressTimer = 0;
        }
    }
    protected void BattleDPadUp(bool pressed)
    {
        if (pressed)
        {
            //DPadUpPressTimer = (float)AudioSettings.dspTime;
            int critChance = patternController.CheckBulletToDestroy(NoteColor.UP, TypeOfInput.TAP, 0);
            switch (critChance)
            {
                case -1:
                case -2:
                case -3:
                case -4:
                    Defend(critChance);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    if (inAttackRange)
                        NormalAttack();
                    break;
                case 0:

                    break;
            }
        }
        else if (!pressed)
        {
            //DPadUpPressTimer = (float)AudioSettings.dspTime - DPadUpPressTimer;
            //patternController.CheckBulletToDestroy(NoteColor.UP, TypeOfInput.RELEASE, DPadUpPressTimer);
            //DPadUpPressTimer = 0;
        }
    }
    #endregion
    #region Targeting
    public override void LockOnEnemy()
    {
        if (!isLockedOnEnemy)
        {
            lockedEnemy = FindNearestEnemy();
            if (lockedEnemy != null)
            {
                isLockedOnEnemy = !isLockedOnEnemy;
                if (onLock != null)
                    onLock(this, lockedEnemy, true);

                cameraMovement.OnPlayerLock(lockedEnemy, true);
                patternSynch.playerLocked = true;
            }
        }
        else
        {
            if (onLock != null)
                onLock(this, null, false);

            cameraMovement.OnPlayerLock(null, false);
            patternSynch.playerLocked = false;
            lockedEnemy = null;
            isLockedOnEnemy = false;
        }
    }
    public override void SwitchTarget(int dir)
    {
        switch (dir)
        {
            case -1:
                lockedEnemy = enemyDetect.FindTargetFromSides(lockedEnemy.transform, false);
                break;
            case 0:
                lockedEnemy = BattleManager.INSTANCE.GetNextEnemyToLock(PlayerNumber);
                break;
            case 1:
                lockedEnemy = enemyDetect.FindTargetFromSides(lockedEnemy.transform, true);
                break;
            case 5:
                lockedEnemy = FindNearestEnemy();
                break;
            case 10:
                break;
        }

        if (lockedEnemy == null)
        {
            LockOnEnemy();
            return;
        }

        cameraMovement.UpdateEnemyBar(lockedEnemy.currentHealthPoints, lockedEnemy.maxHealthPoints);
    }
    #endregion

    protected override void BeatSynch_OnSongBeat()
    {
        base.BeatSynch_OnSongBeat();
        HandleSkillCooldowns();
    }

    public override void Pause()
    {
        int p = GameManager.INSTANCE.currentGameState == GameManager.INSTANCE.PAUSESTATE ? 0 : 1;
        inputHandler.SwitchStates(p);
        base.Pause();
    }

    public override void SetupCamera(CameraMovement cam)
    {
        base.SetupCamera(cam);
    }

    public void SetupController(int n)
    {
        PlayerNumber = n;
        inputHandler = new InputHandler(this, GetComponent<Locomotion>(), n);
    }

    public override void SetBeatBasedVariables(float beat)
    {
        locomotion.SetHookSpeed(beat);

        float bpm = MusicController.bpm;
    }

    public void Reset()
    {
        isLockedOnEnemy = false;
        lockedEnemy = null;
        inAttackRange = false;
        beingTargeted = false;
    }
}