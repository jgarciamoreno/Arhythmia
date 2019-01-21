using Arhythmia;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Enemy : Character
{

    public MeshRenderer auxMesh;

    PlayerDetection playerDetection;
    protected float staggerValue = 1;
    protected int level;
    #region SquadRoles
    public IRole Role;
    public IRole Leader = new Leader();
    public IRole Subordinate = new Subordinate();
    public IRole Independent = new Independent();
    #endregion
    #region Movement Vars
    [SerializeField] protected float turnSpeed;
    public float turnDst = 5;
    #endregion
    #region Delegates & events
    public delegate void onDamageTaken(Enemy e, float currentHealth, float maxHealth);
    public static event onDamageTaken OnDamageTaken; //Update THIS health bar to all player cameras
    #endregion
    #region Pathfinding Vars
    public PathF path;
    public bool playerLocked = false;
    public bool isTargetPlayerVisible = false;
    protected Player targetPlayer;
    public Player TargetPlayer
    {
        get { return targetPlayer; }
    }
    public Vector3 playerPosition = Vector3.zero;

    private bool updatePathRequested = false;
    private float sqrMoveThreshold;
    public Vector3 targetPosOld;
    #endregion
    #region Pathway vars
    public List<Transform> patrolWaypoints = new List<Transform>(); //Waypoints in the patrol pattern
    protected int pathIndex = 0;
    protected int waypointIndex = 0;
    protected bool followingPath = false;
    const float pathUpdateMoveThreshold = 0.5f;
    const float minPathUpdateTime = 0.2f;
    private float pathUpdateTimer = 0;
    #endregion
    #region Targeting Vars
    public Transform lockPosition;
    protected int currentTargetPriority = -1;
    public enum PriorityTarget { MELODY, ARMONIZER, BASS, PERCUSSION, NONE }
    public PriorityTarget[] priorities = new PriorityTarget[4];
    protected int maxAreasToChase; //0 means no leaving own area
    protected float maxDistToChase; //0 means static character, only it's attack range
    //protected float cryRadius; //Yelling for aid radius
    public int aggro = 0; //1 - 20 (Tier 1) / 21 - 49 (Tier 2) / 50 - 79 (Tier 3) / 80 - 99 (Tier 4) / 100 (Boss)
    protected int orderToAttack = -1; //In case of same aggro order is determined by order
    #endregion
    #region Squad Vars
    public bool isInSquad;
    public bool isLeader;
    private int subordinateN;
    public int SubordinateN
    {
        get
        {
            return subordinateN;
        }
        set
        {
            subordinateN = value;
            if (value == 0 && isInSquad)
                isLeader = true;
            else
                isLeader = false;
        }
    }
    public int FormationType = 0;
    public List<Enemy> squadAllies = new List<Enemy>();
    public Transform leaderPos;
    protected Vector3 squadPosDir;
    protected float squadPosOffset;
    protected bool alliesNotified = false;
    #endregion
    #region Attack Vars & Times
    public bool hasAttacked = false;
    public enum RhythmAttackType { NOTES, BEATS }
    public RhythmAttackType AttackType;
    public TypeOfDamage attackDamageType;
    protected float attackRange; //Range in which the enemy begins its attack
    [SerializeField] private BeatValue[] attackPeriod;
    [SerializeField] private BeatValue[] waitToAttackPeriod;
    public float attackTime = 0; //Attack period in seconds to calculate amount of notes
    public float waitToAttackTime = 0; //WaitToAttack period in seconds to calculate amount of notes
    public int attackNotesQty; //Max amount of attack notes
    public int notesBeforeAttackQty; //Max amount of notes to wait to attack again
    public int beatsToAttack;
    #endregion
    #region FSM vars
    public IEnemyState currentState;
    public IdleState idleState;
    public PatrolSate patrolState;
    public ChaseState chaseState;
    public AttackState attackState;
    public WaitState waitState;
    public SearchState searchState;
    public AlertState alertState;
    public ProtectState protectState;
    public StunnedState stunnedState;
    #endregion

    private void Awake()
    {
        idleState = new IdleState(this);
        patrolState = new PatrolSate(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        waitState = new WaitState(this);
        searchState = new SearchState(this);
        alertState = new AlertState(this);
        protectState = new ProtectState(this);
        stunnedState = new StunnedState(this);

        //For color testing
        auxMesh = GetComponent<MeshRenderer>();
        //
        playerDetection = GetComponent<PlayerDetection>();
        attackRange = playerDetection.attackRadius;

    }
    protected override void Start()
    {
        base.Start();

        if (AttackType == RhythmAttackType.NOTES)
        {
            for (int i = 0; i < attackPeriod.Length; i++)
                attackTime += 60 / (MusicController.bpm * BeatDecimalValues.values[(int)attackPeriod[i]]);
            for (int i = 0; i < waitToAttackPeriod.Length; i++)
                waitToAttackTime += 60 / (MusicController.bpm * BeatDecimalValues.values[(int)waitToAttackPeriod[i]]);
        }

        if (isInSquad)
        {
            if (isLeader)
                Role = Leader;
            else
                Role = Subordinate;

            Role.SquadPlacement(this);
        }
        else
        {
            Role = Independent;
        }

        if (patrolWaypoints != null && patrolWaypoints.Count > 0)
            currentState = patrolState;
        else
            currentState = idleState;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
        orderToAttack = -1;
    }
    protected virtual void FixedUpdate()
    {
        currentState.UpdateState();
    }

    private void MelodySynch_onPatternBeat()
    {

    }

    protected override void BattleManager_enemyKilled(Enemy enemy, bool battleEnded)
    {
        orderToAttack = (--orderToAttack == 0) ? 0 : orderToAttack;
    }

    public virtual void SquadPlacement() { }

    RaycastHit hit;
    private bool isObstacleInFront = false;
    public virtual void Turn(Vector3 objective)
    {
        if (!isObstacleInFront)
        {
            Quaternion rotation = Quaternion.identity;
            Vector3 pos = objective - transform.position;
            if (pos != Vector3.zero)
                rotation = Quaternion.LookRotation(pos);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);
        }

        Transform leftRay = transform;
        Transform rightRay = transform;

        if (Physics.Raycast(leftRay.position + (transform.right * 1) + Vector3.up, transform.forward, out hit, 7) ||
            Physics.Raycast(rightRay.position - (transform.right * 1) + Vector3.up, transform.forward, out hit, 7))
        {
            if (hit.collider.gameObject.CompareTag("Obstacle") || ((!isInSquad || isLeader) && hit.collider.gameObject.CompareTag("Enemy")))
            {
                isObstacleInFront = true;
                transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed);
            }
        }

        if (Physics.Raycast(transform.position - (transform.forward * 1) + Vector3.up, transform.right, out hit, 7) ||
            Physics.Raycast(transform.position - (transform.forward * 1) + Vector3.up, -transform.right, out hit, 7))
        {
            if (hit.collider.gameObject.CompareTag("Obstacle") || ((!isInSquad || isLeader) && hit.collider.gameObject.CompareTag("Enemy")))
            {
                isObstacleInFront = false;
            }
        }
    }
    public virtual void Turn(Quaternion rot)
    {
        transform.rotation = rot;
    }
    public virtual void Approach(float speed) { }
    public virtual void Idle() { }
    public virtual void Patrol()
    {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
        if (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
        {
            if (pathIndex == path.finishLineIndex)
            {
                followingPath = false;
                RequestPath(patrolWaypoints[waypointIndex].position);
                return;
            }
            else
                pathIndex++;
        }

        if (followingPath)
            Turn(path.lookPoints[pathIndex]);

        transform.Translate(Vector3.forward * (normalMovementSpeed * Time.deltaTime) / staggerValue);
    }
    public virtual void SquadPatrol()
    {
        Turn(SquadFormation.Position(leaderPos, FormationType, SubordinateN));
        transform.position = SquadFormation.Position(leaderPos, FormationType, SubordinateN);
    }
    public virtual void Chase()
    {
        if (pathUpdateTimer >= minPathUpdateTime)
            UpdatePath(targetPlayer.transform.position);
        else
            pathUpdateTimer += Time.deltaTime;

        Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
        if (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
        {
            if (pathIndex < path.finishLineIndex)
                pathIndex++;
            else
            {
                playerLocked = false;
                currentState.ToAlertState();
                return;
            }
        }


        float dist = Vector3.Distance(transform.position, TargetPlayer.transform.position);
        if (dist <= attackRange)
        {

            SetOrderToAttack(true);
            if (GetOrderToAttack() == 0)
                currentState.ToAttackState();
            else
                currentState.ToWaitToAttackState();

            updatePathRequested = false;
        }

        Turn(path.lookPoints[pathIndex]);
        transform.Translate(Vector3.forward * chaseMovementSpeed * (Time.deltaTime / staggerValue));
    }
    public virtual void SquadChase()
    {
        Turn(transform.position + Vector3.up * 10);
        transform.position = SquadFormation.Position(leaderPos, FormationType, SubordinateN);
    }
    public virtual void Search()
    {
        //Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
        //if (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
        //    pathIndex++;

        //if (pathIndex >= path.finishLineIndex)
        //{
        //    currentState.ToAlertState();
        //}

        //Turn(path.lookPoints[pathIndex]);
        //transform.Translate(Vector3.forward * chaseMovementSpeed * (Time.deltaTime / staggerValue));
    }
    public virtual void Protect()
    {
    }
    public virtual void Alert()
    {
    }
    public virtual void OnAttackEnter()
    {
        Turn(TargetPlayer.transform.position);

        float dist = Vector3.Distance(transform.position, TargetPlayer.transform.position);

        if (dist >= attackRange / 2 && dist <= attackRange)
            Approach(onLockMovementSpeed);
        else if (dist > attackRange)
            currentState.ToChaseState();
    }
    public override void NormalAttack() { }
    public override void StunAttack() { }
    public override void BreakDefence() { }
    public virtual void Attack(bool successful)
    {
        if (successful)
        {
            damageDone = targetPlayer.TakeDamage(attackDamage + additionalAttack, transform.position, attackDamageType);
            //Add attack animation
        }
        else
        {
            //Add blocked attack animation
        }
    }
    public virtual void SquadAttack() { }
    public virtual void WaitToAttack()
    {
        Turn(TargetPlayer.transform.position);

        float dist = Vector3.Distance(transform.position, TargetPlayer.transform.position);

        if (dist > attackRange / 2 && dist < attackRange)
            Approach(onLockMovementSpeed);
        else if (dist > attackRange)
            currentState.ToChaseState();
    }
    public virtual void SquadWaitToAttack() { }

    public virtual void TargetAcquired() { }
    public virtual void TargetLost()
    {
        isTargetPlayerVisible = false;
        //currentState.ToSearchLostPlayerState();
    }

    public virtual void NotifySquad()
    {
        foreach (Enemy e in squadAllies)
            e.LeaderAttack();
    }
    public virtual void LeaderAttack() { }

    public override float TakeDamage(float dmg, Vector3 hitPosition, TypeOfDamage type)
    {
        float damageTaken = base.TakeDamage(dmg, hitPosition, type);
        HandleStagger(2);
        if (currentHealthPoints <= 0)
            Die();
        else
        {
            OnDamageTaken(this, currentHealthPoints, maxHealthPoints);
            if (targetPlayer == null)
            {
                playerPosition = hitPosition;
                currentState.ToAlertState();
                alertState.AlertLevel = 1;
            }
        }

        return damageTaken;
    }

    public override void Paralyze(int pwr)
    {
        base.Paralyze(pwr);
        if (isParalyzed)
            Stun();
    }

    public void Stun()
    {
        currentState.ToStunnedState();
    }
    public void Stun(int typeOfStun, float stunTime)
    {
        currentState.ToStunnedState();
        StartCoroutine(stunnedState.Countdown(typeOfStun, stunTime));
    }
    public void Unstun(int typeOfUnstun)
    {
        if (targetPlayer != null && orderToAttack != -1)
        {
            if (typeOfUnstun == 0)
                currentState.ToAttackState();
            else if (typeOfUnstun == 1)
                currentState.ToWaitToAttackState();
        }
        else
        {
            currentState.ToAlertState();
            alertState.AlertLevel = typeOfUnstun;
        }
    }

    public void RequestPath(Vector3 objective)
    {
        PathRequestManager.RequestPath(new PathRequest(transform.position, objective, OnPathFound));
    }
    protected void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new PathF(waypoints, transform.position, turnDst);
            waypointIndex = ++waypointIndex == patrolWaypoints.Count ? 0 : waypointIndex;
            pathIndex = 0;
            followingPath = true;
        }
        else
        {
            currentState.TargetUnreachable();
        }
    }
    protected void UpdatePath(Vector3 objective)
    {
        if (!updatePathRequested)
        {
            sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
            if (isTargetPlayerVisible)
                targetPosOld = TargetPlayer.transform.position;
            updatePathRequested = true;
        }

        if(isTargetPlayerVisible)
        {
            if ((targetPlayer.transform.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, TargetPlayer.transform.position, OnPathFound));
                targetPosOld = TargetPlayer.transform.position;
                pathIndex = 0;
            }
        }
    }

    protected IEnumerator HandleStagger(float value)
    {
        staggerValue = value;
        while (staggerValue > 1)
        {
            staggerValue -= Time.deltaTime;
            yield return null;
        }
        staggerValue = -1;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        dead = true;
        if (isLeader)
            DelegateLeadership();
        BattleManager.INSTANCE.CharacterKilled(this, targetPlayer);
        base.Die();
    }

    protected void DelegateLeadership()
    {

    }

    public void ReorderToAttack(int order, int typeOfReorder)
    {
        switch (typeOfReorder)
        {
            case 0://No need to reassign
                orderToAttack = order;
                if (order == 0)
                {
                    if (aggro > 0 && aggro < 20 || hasAttacked)
                        currentState.TargetAcquired();
                    else if (aggro > 20 && aggro < 40)
                        currentState.ToAttackState();
                }
                break;
            case 1://Single unit reassignment, replaced by 40-59 aggro
                break;
            case 2://Multiple unit reassignment, replaced by 60+ aggro, protect said Enemy ally
                break;
            case 3://Enemy killed or enemy left the list
                orderToAttack = order;
                break;
        }
    }
    public virtual void SetOrderToAttack(bool toAttack)
    {
        if (toAttack)
            orderToAttack = BattleManager.INSTANCE.AddAttackingEnemies(targetPlayer, this);
        else
            orderToAttack = -1;
    }
    public int GetOrderToAttack()
    {
        return orderToAttack;
    }

    public abstract int GetLevel();

    public void SetTargetPlayer(Player _target)
    {
        if (priorities[0] == PriorityTarget.NONE)
        {
            targetPlayer = _target;
            currentTargetPriority = 0;
            playerLocked = true;
        }
        else
        {
            int priority = _target.PatternSynchNumber;
            for (int i = 0; i < 4; i++)
            {
                if ((i < currentTargetPriority || currentTargetPriority == -1) && priority == (int)priorities[i])
                {
                    targetPlayer = _target;
                    currentTargetPriority = i;
                    if (i == 0)
                    {
                        playerLocked = true;
                        break;
                    }
                }
            }
        }

        currentState.TargetAcquired();
    }

    public virtual void ChargeAoEProjector() { }
    public virtual void HideAoEProjector() { }
}