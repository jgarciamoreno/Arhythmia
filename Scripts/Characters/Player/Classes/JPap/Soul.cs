using UnityEngine;

public class Soul : MonoBehaviour ,IReturnable {

    private JPap jpap;
    private SoulPooler sp;
    private Player playerShielding;
    private Enemy enemyTarget;
    public ParticleSystem body;
    public ParticleSystem trail;
    public ParticleSystem flame;
    private enum Type { COLLECTABLE, DEFENSIVE, OFFENSIVE, RETURNING }
    private Type type;

    private Vector3 errorDisplacement = Vector3.zero;

    private float power = 0;
    private float overcharge = 0;
    private float maxLifeTimeAsProjectile = 5;
    private float maxLifeTimeAsCollectable = 15f;
    private float lifeTimer = 0;

    public bool isCollectable = false;
    private bool isProjectile = false;

    private void Update()
    {
        switch (type)
        {
            case Type.COLLECTABLE:
                lifeTimer += Time.deltaTime;
                if (lifeTimer >= maxLifeTimeAsCollectable)
                {
                    gameObject.SetActive(false);
                }
                break;
            case Type.DEFENSIVE:
                //transform.position += (transform.position - playerShielding.transform.position) * 15f * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, playerShielding.transform.position, 15 * Time.deltaTime);
                if (Vector3.Distance(transform.position, playerShielding.transform.position) < 0.3f)
                {
                    //Effect of shielding
                    playerShielding.AddShield(this, power);
                    playerShielding.AddDefence(0);
                    gameObject.SetActive(false);
                }
                break;
            case Type.OFFENSIVE:
                if (isProjectile)
                {
                    transform.position += transform.forward * 15 * Time.deltaTime;
                    if (Vector3.Distance(transform.position, enemyTarget.transform.position) < 0.15f)
                    {
                        enemyTarget.TakeDamage(power + overcharge, jpap.transform.position, Character.TypeOfDamage.NORMAL);
                        //Add Explosion or something
                        gameObject.SetActive(false);
                    }

                    lifeTimer += Time.deltaTime;
                    if (lifeTimer >= maxLifeTimeAsProjectile)
                        gameObject.SetActive(false);
                }
                break;
            case Type.RETURNING:
                transform.position = Vector3.MoveTowards(transform.position, jpap.transform.position, 15 * Time.deltaTime);
                if (Vector3.Distance(transform.position, jpap.transform.position) < 0.3f)
                {
                    if (isCollectable)
                        jpap.AbsorbSoul(power);
                    else
                        jpap.RecoverSoul(power);

                    gameObject.SetActive(false);
                }
                break;
        }
    }

    private void OnDisable()
    {
        power = 0;
        overcharge = 0;
        lifeTimer = 0;
        isCollectable = false;
        isProjectile = false;
        enemyTarget = null;
        var main = body.main;
        main.startColor = sp.g.Evaluate(0);
    }

    public void InitializeSoul(JPap j)
    {
        jpap = j;
        sp = jpap.GetComponent<SoulPooler>();
    }
    public Player GetPlayer()
    {
        return playerShielding;
    }

    public void ActivateAsHarvestable()
    {
        type = Type.COLLECTABLE;
        isCollectable = true;
        gameObject.SetActive(true);
        power = 10;
    }
    public void ActivateAsShield(Player toShield, int _power)
    {
        type = Type.DEFENSIVE;
        playerShielding = toShield;
        power = _power;
        transform.position = jpap.transform.position;
        gameObject.SetActive(true);
    }
    public void ActivateAsAttack(Enemy e, float _power)
    {
        power = Mathf.RoundToInt(_power);
        type = Type.OFFENSIVE;
        enemyTarget = e;
        gameObject.SetActive(true);
    }

    public void Overcharge(float power)
    {
        if (overcharge < 10)
        {
            overcharge += power;
            if (overcharge > 10)
                overcharge = 10;

            var main = body.main;
            main.startColor = sp.g.Evaluate(overcharge);
        }
    }

    public void Shoot(bool perfectAcurracy)
    {
        isProjectile = true;
        if (!perfectAcurracy)
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            errorDisplacement = new Vector3(x, y, z);
        }

        transform.rotation = Quaternion.LookRotation((enemyTarget.transform.position + errorDisplacement) - transform.position);
    }

    public void Absorb()
    {
        type = Type.RETURNING;
    }

    public void ReturnObject()
    {
        jpap.RetrieveSoulFrom(playerShielding);
        transform.position = playerShielding.transform.position;
        playerShielding.AddDefence(-3);
        type = Type.RETURNING;
        gameObject.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (type == Type.OFFENSIVE)
        {
            Enemy e = collision.gameObject.GetComponent<Enemy>();
            if (e != null)
                e.TakeDamage(power + overcharge, jpap.transform.position, Character.TypeOfDamage.NORMAL);

            gameObject.SetActive(false);
        }
    }
}
