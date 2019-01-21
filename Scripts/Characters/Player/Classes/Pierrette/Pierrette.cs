using UnityEngine;
using System.Collections;
using Arhythmia;

public class Pierrette : Player
{
    public Material[] mats;
    #region Skill1
    protected float invisibleTime = 6.0f;
    private bool stillInvisible = false;
    private Coroutine invisibility;
    #endregion
    #region Skill2

    #endregion
    #region Skill3
    private float backstabDuration;
    private int backstabMaxThreshold = 32; //8* GameManager.difficultyLevel;
    private int backstabThreshold = -1;
    private bool backstabInitiated = false;
    #endregion

    public int enemiesOnPlayer = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        maxHealthPoints = 200;
        currentHealthPoints = maxHealthPoints;
        attackDamage = 9;
        critMultiplier = 1.2f;
        defencePower = 5;

        statusRecoverySpeed = 26;

        skillOneCooldownTime = 12;
        skillOneTimer = skillOneCooldownTime;
        skillTwoCooldownTime = 9;
        skillTwoTimer = skillOneCooldownTime;
        skillThreeCooldownTime = 20;
        skillThreeTimer = skillThreeCooldownTime;

        enemyDetect = GetComponent<EnemyDetection>();
        rigidBody = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        MusicController.OnAudioStart += OnMusicStart;
        base.Start();
    }

    protected override void OnDisable()
    {
        MusicController.OnAudioStart -= OnMusicStart;
        base.OnDisable();
    }

    public override void SetBeatBasedVariables(float beat)
    {
        base.SetBeatBasedVariables(beat);
        backstabDuration = beat * 4;
    }

    protected override void OnBeingDetected(bool isBeingTargeted)
    {
    }

    public override void NormalAttack()
    {
        if (stillInvisible)
            Uncloak(true);

        if (backstabInitiated)
        {
            backstabThreshold--;

            if (backstabThreshold == 0)
                lockedEnemy.TakeDamage(Backstab(true), transform.position, TypeOfDamage.TRUE);
        }
        else
            base.NormalAttack();
    }

    public override void Miss()
    {
        if (backstabInitiated)
            Backstab(false);
    }

    public override float TakeDamage(float dmg, Vector3 hitPosition, TypeOfDamage type)
    {
        float damageTaken = base.TakeDamage(dmg, hitPosition, type);

        return damageTaken;
    }

    protected override void Skill(int skill)
    {
        switch (skill)
        {
            case 1:
                if (stillInvisible)
                    break;

                isTargetable = false;
                stillInvisible = true;
                critMultiplier = 1.6f;
                invisibility = StartCoroutine(Invisibility());
                break;
            case 2:
                break;
            case 3:
                if (!backstabInitiated)
                {
                    backstabInitiated = true;
                    backstabThreshold = PatternSynch.CalculateNotesInTime(backstabDuration);
                    if (backstabThreshold > backstabMaxThreshold)
                        backstabThreshold = backstabMaxThreshold;
                    lockedEnemy.Stun();
                }
                break;
        }
    }

    private IEnumerator Invisibility()
    {
        foreach (Material m in mats)
        {
            m.SetFloat("_Mode", 3);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.DisableKeyword("_ALPHABLEND_ON");
            m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
            Color col = m.color;
            col.a = 0;
            m.color = col;
        }

        while (invisibleTime >= 0)
        {
            invisibleTime -= Time.deltaTime;
            yield return null;
        }

        if (stillInvisible)
            Uncloak(false);
    }
    private void Uncloak(bool isAttack)
    {
        if (isAttack)
            StopCoroutine(invisibility);

        isTargetable = true;
        isSkillOneOnCooldown = true;
        stillInvisible = false;
        invisibleTime = 6.0f;
        critMultiplier = 1.2f;

        foreach (Material m in mats)
        {
            m.SetFloat("_Mode", 0);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            m.SetInt("_ZWrite", 1);
            m.DisableKeyword("_ALPHATEST_ON");
            m.DisableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = -1;
            m.SetFloat("_DETAIL_MULX2", 1);
            Color col = m.color;
            col.a = 1;
            m.color = col;
        }
    }

    private int Backstab(bool success)
    {
        int dmg;
        if (success)
        {
            transform.position = lockedEnemy.transform.position - lockedEnemy.transform.forward;
            transform.rotation = Quaternion.LookRotation(lockedEnemy.transform.position - transform.position);
            cameraMovement.MoveCamera(1f);
            dmg = 130;
        }
        else
        {
            dmg = 0;
        }

        lockedEnemy.Unstun(0);

        backstabThreshold = -1;
        backstabInitiated = false;
        isSkillThreeOnCooldown = true;

        return dmg;
    }

    private void OnMusicStart()
    {
    }

    int idleCounter = 0;
    public void IdleLoop()
    {
        idleCounter++;
        if (idleCounter > 5)
            idleCounter = 0;
    }
    public void OnRhythm()
    {
    }
}
