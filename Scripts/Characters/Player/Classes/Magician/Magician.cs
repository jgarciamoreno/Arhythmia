using Arhythmia;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magician : Player {

    private enum YoYo { YELLOW, RED, VIOLET }
    private YoYo currentYoYo;

    private bool hasYellow = true;
    private bool hasRed = true;
    private bool hasViolet = true;

    private Projector[] projectors = new Projector[2];

    public ParticleSystem[] particles;
    private GameObject trapPulse;
    private GameObject[] trapLightning = new GameObject[10];
    private GameObject EMPparticle;

    #region Passive
    private int statusInducingPower = 10;
    #endregion
    #region Yellow
    private Vector3 trapPosition;
    private bool trapDeployed = false;
    private bool trapOnCooldown = false;
    private float trapRadius = 7;
    private int trapPwr = 25;
    private float trapTimer;
    private int trapCooldown;
    #endregion
    #region Red
    private bool explosionAim = false;
    private bool explosionOnCooldown = false;
    private float explosionDistance = 5f;
    private float explosionRadius = 2f;
    private int linePwr = 10;
    private int explosionPwr = 38;
    private int explosionBrnPwr = 22;
    private float explosionTimer;
    private int explosionCooldown;
    #endregion
    #region Violet
    private bool phantomOnCooldown = false;
    private float phantomDuration;
    private float phantomTimer;
    private int phantomCooldown;
    #endregion
    #region Skill3
    private int ultPassivePwr;
    private int ultParalysisPwr = 60;
    private int ultBurnPwr = 45;
    private int ultBreakPwr = 75;

    private float ultParalysisRadius = 9;
    private int ultParalysisDmg = 15;
    private float ultExplosionRadius = 6;
    private int ultExplosionDmg = 90;
    private float ultRicochetRadiusSearch = 10;
    private int ultRicochetBaseDmg = 10;
    private int ultRicochetDmgPerJump = 4;
    private List<Enemy> enemiesJumped = new List<Enemy>();

    private int ultNotesFirst, ultNotesSecond, ultNotesLast;
    private int ultNotesPerYoYo;
    private int ultNotesQty;
    private double ultTime;

    private bool isJuggling = false;
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        maxHealthPoints = 200;
        currentHealthPoints = maxHealthPoints;
        attackDamage = 9;
        critMultiplier = 1.2f;
        defencePower = 5;

        statusRecoverySpeed = 18;

        trapCooldown = 11;
        trapTimer = trapCooldown;
        explosionCooldown = 14;
        explosionTimer = explosionCooldown;
        phantomCooldown = 18;
        phantomTimer = phantomCooldown;

        skillThreeCooldownTime = 60;
        skillThreeTimer = skillThreeCooldownTime;

        enemyDetect = GetComponent<EnemyDetection>();
        rigidBody = GetComponent<Rigidbody>();

        ultPassivePwr = ultParalysisPwr;

        projectors = GetComponentsInChildren<Projector>();


    }

    protected override void Start()
    {
        MusicController.OnAudioStart += OnMusicStart;
        base.Start();
    }

    public override void SetBeatBasedVariables(float beat)
    {
        base.SetBeatBasedVariables(beat);
        phantomDuration = beat * 16;
        ultTime = beat * 12;

        trapPulse = Instantiate(particles[0].gameObject);
        for (int i = 0; i < 10; i++)
            trapLightning[i] = Instantiate(particles[1].gameObject);
        EMPparticle = Instantiate(particles[2].gameObject);
    }

    private void OnMusicStart()
    {

    }

    protected override void OnDisable()
    {
        MusicController.OnAudioStart -= OnMusicStart;
        base.OnDisable();
    }

    protected override void HandleSkillCooldowns()
    {
        if (trapOnCooldown)
        {
            trapTimer -= Time.deltaTime;
            //Camera skill cd
            if (trapTimer <= 0)
            {
                trapTimer = trapCooldown;
                trapOnCooldown = false;
            }
        }
        if (explosionOnCooldown)
        {
            explosionTimer -= Time.deltaTime;
            //Camera skill cd
            if (explosionTimer <= 0)
            {
                explosionTimer = explosionCooldown;
                explosionOnCooldown = false;
            }
        }
        if(phantomOnCooldown)
        {
            phantomTimer -= Time.deltaTime;
            //Camera skill cd
            if (phantomTimer <= 0)
            {
                phantomTimer = phantomCooldown;
                phantomOnCooldown = false;
            }
        }

        if (isSkillThreeOnCooldown)
        {
            skillThreeTimer -= Time.deltaTime;
            cameraMovement.SetSkillOnCooldown(3, skillThreeTimer / skillThreeCooldownTime);

            if (skillThreeTimer <= 0)
            {
                skillThreeTimer = skillThreeCooldownTime;
                isSkillThreeOnCooldown = false;
            }
        }
    }

    public override void NormalAttack()
    {
        if (!isJuggling)
        {
            Passive(statusInducingPower);
            base.NormalAttack();
        }
        else
        {
            if (ultNotesFirst > 0)
            {
                ultNotesFirst--;
                if (ultNotesFirst == 0)
                {
                    Passive(ultPassivePwr);
                    base.NormalAttack();
                    HandleYoYo();
                }
            }
            else if (ultNotesSecond > 0)
            {
                ultNotesSecond--;
                if (ultNotesSecond == 0)
                {
                    Passive(ultPassivePwr);
                    base.NormalAttack();
                    HandleYoYo();
                }
            }
            else if (ultNotesLast > 0)
            {
                ultNotesLast--;
                if (ultNotesLast == 0)
                    WomboYoyo();
            }
        }
    }

    public override void Miss()
    {
        if (isJuggling)
        {
            isJuggling = false;
            isSkillThreeOnCooldown = true;
        }
    }

    public override void Skill2()
    {
        if (currentYoYo == YoYo.RED)
        {
            if (!explosionOnCooldown)
            {
                explosionAim = !explosionAim;
            }
        }

        Skill(2);
    }

    protected override void Skill(int skill)
    {
        switch (skill)
        {
            case 1:
                HandleYoYo();
                break;
            case 2:
                switch (currentYoYo)
                {
                    case YoYo.YELLOW:
                        if (hasYellow && !trapOnCooldown)
                        {
                            trapPosition = transform.position;
                            trapDeployed = true;
                            hasYellow = false;
                            HandleYoYo();
                            //Drop Trap
                            trapPulse.transform.position = transform.position;
                            trapPulse.SetActive(true);
                        }
                        else
                        {
                            trapDeployed = false;
                            hasYellow = true;
                            trapOnCooldown = true;
                            //Pick up trap
                        }
                        break;
                    case YoYo.RED:
                        if (explosionAim)
                            DrawBoomArea();
                        else
                            Boom();
                        break;
                    case YoYo.VIOLET:
                        if(lockedEnemy != null)
                        {
                            if (!phantomOnCooldown)
                                PhantomizeBaddies();
                        }
                        break;
                }
                break;
            case 3:
                if (!isSkillThreeOnCooldown && !isJuggling)
                {
                    isJuggling = true;
                    ultNotesQty = PatternSynch.CalculateNotesInTime(ultTime);
                    ultNotesPerYoYo = Mathf.RoundToInt(ultNotesQty / 3);
                    int remainder = ultNotesQty - (ultNotesPerYoYo * 3);

                    ultNotesFirst = ultNotesSecond = ultNotesLast = ultNotesPerYoYo;

                    if (remainder > 0)
                        ultNotesLast = ultNotesPerYoYo + remainder;
                }
                break;
        }
    }

    private void Passive(int pwr)
    {
        switch (currentYoYo)
        {
            case YoYo.YELLOW:
                lockedEnemy.Paralyze(pwr);
                break;
            case YoYo.RED:
                if (!explosionOnCooldown)
                    lockedEnemy.Burn(pwr);
                break;
            case YoYo.VIOLET:
                lockedEnemy.BreakDef(pwr);
                break;
        }
    }

    private void HandleYoYo()
    {
        float cd = 0;
        switch (currentYoYo)
        {
            case YoYo.YELLOW:
                if (hasRed)
                {
                    currentYoYo = YoYo.RED;
                    ultPassivePwr = ultBurnPwr;
                    cd = 45;
                }
                else if (hasViolet)
                {
                    currentYoYo = YoYo.VIOLET;
                    ultPassivePwr = ultBreakPwr;
                    cd = 75;
                }
                break;
            case YoYo.RED:
                if (hasViolet)
                {
                    currentYoYo = YoYo.VIOLET;
                    ultPassivePwr = ultBreakPwr;
                    cd = 75;
                }
                else if (hasYellow)
                {
                    currentYoYo = YoYo.YELLOW;
                    ultPassivePwr = ultParalysisPwr;
                    cd = 60;
                }
                break;
            case YoYo.VIOLET:
                if (hasYellow)
                {
                    currentYoYo = YoYo.YELLOW;
                    ultPassivePwr = ultParalysisPwr;
                    cd = 60;
                }
                else if (hasRed)
                {
                    currentYoYo = YoYo.RED;
                    ultPassivePwr = ultBurnPwr;
                    cd = 45;
                }
                break;
        }

        skillThreeCooldownTime = cd;
        skillThreeTimer = skillThreeCooldownTime;
    }

    private void BzztEnemy(Enemy e)
    {
        e.Paralyze(trapPwr);
        for (int i = 0; i < trapLightning.Length; i++)
        {
            if (!trapLightning[i].activeInHierarchy)
            {
                trapLightning[i].transform.position = e.transform.position;
                trapLightning[i].SetActive(true);
                break;
            }
        }
    }

    private void DrawBoomArea()
    {
        projectors[0].enabled = true;
        projectors[1].enabled = true;
    }
    private void Boom()
    {
        Collider[] lineColl = Physics.OverlapBox(transform.position + Vector3.up + transform.forward * (explosionDistance / 2), new Vector3(0.35f, 2, 5), transform.rotation, 1 << 8);
        Collider[] circleColl = Physics.OverlapSphere(transform.position + transform.forward * explosionDistance, explosionRadius, 1 << 8);
        
        for (int i = 0; i < lineColl.Length; i++)
            lineColl[i].GetComponent<Enemy>().TakeDamage(linePwr, transform.position, TypeOfDamage.NORMAL);

        for (int i = 0; i < circleColl.Length; i++)
        {
            Enemy e = circleColl[i].GetComponent<Enemy>();
            e.TakeDamage(explosionPwr, transform.position, TypeOfDamage.NORMAL);
            e.Burn(explosionBrnPwr);
        }

        projectors[0].enabled = false;
        projectors[1].enabled = false;

        explosionOnCooldown = true;
    }

    private void PhantomizeBaddies()
    {
        if (hasViolet)
        {
            lockedEnemy.DisableShield();
            hasViolet = false;
        }
        else
        {
            lockedEnemy.EnableShield();
            hasViolet = true;
            phantomOnCooldown = true;
        }

    }

    private void WomboYoyo()
    {
        switch (currentYoYo)
        {
            case YoYo.YELLOW:
                Zzzap();
                break;
            case YoYo.RED:
                KaBoom();
                break;
            case YoYo.VIOLET:
                RicoRico();
                break;
        }

        isJuggling = false;
        isSkillThreeOnCooldown = true;
    }

    private void Zzzap()
    {
        foreach (Enemy e in FindEnemies(lockedEnemy.transform.position, ultParalysisRadius, 1 << 8))
        {
            e.Paralyze(ultParalysisPwr + 70);
            e.TakeDamage(ultParalysisDmg, transform.position, TypeOfDamage.NORMAL);
        }

        EMPparticle.transform.position = lockedEnemy.transform.position;
        EMPparticle.SetActive(true);
    }
    private void KaBoom()
    {
        foreach (Enemy e in FindEnemies(lockedEnemy.transform.position, ultExplosionRadius, 1 << 8))
        {
            e.Burn(999);
            e.TakeDamage(ultExplosionDmg, transform.position, TypeOfDamage.NORMAL);
        }
    }
    private void RicoRico()
    {
        int jumps = 1;
        bool hit;

        enemiesJumped.Add(lockedEnemy);
        lockedEnemy.TakeDamage(ultRicochetBaseDmg, transform.position, TypeOfDamage.TRUE);

        while (true)
        {
            hit = false;

            foreach (Enemy e in FindEnemies(enemiesJumped[enemiesJumped.Count - 1].transform.position, ultRicochetRadiusSearch, 1 << 8))
            {
                if (!enemiesJumped.Contains(e))
                {
                    e.TakeDamage(ultRicochetBaseDmg + (ultRicochetDmgPerJump * jumps), transform.position, TypeOfDamage.TRUE);
                    enemiesJumped.Add(e);
                    jumps++;
                    hit = true;
                    break;
                }
            }

            if (!hit)
                break;
        }

        enemiesJumped.Clear();
    }

    private Enemy[] FindEnemies(Vector3 pos, float radius, LayerMask layer)
    {
        Collider[] coll = Physics.OverlapSphere(pos, radius, layer);
        Enemy[] e = new Enemy[coll.Length];
        for (int i = 0; i < coll.Length; i++)
            e[i] = coll[i].GetComponent<Enemy>();

        return e;
    }

    bool toBzzt = false;
    protected override void BeatSynch_OnSongBeat()
    {
        if(trapDeployed)
        {
            if (toBzzt)
            {
                foreach (Enemy e in FindEnemies(trapPosition, trapRadius, 1 << 8))
                    BzztEnemy(e);
            }
            toBzzt = !toBzzt;
        }

        base.BeatSynch_OnSongBeat();
    }
}