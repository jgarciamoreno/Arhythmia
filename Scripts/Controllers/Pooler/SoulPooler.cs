using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoulPooler : MonoBehaviour
{
    public Gradient g;

    private JPap jpap;

    public List<GameObject> souls = new List<GameObject>();
    public List<Soul> pooledSouls = new List<Soul>();

    private List<Soul> attackSouls = new List<Soul>();

    public GameObject healingParticle;
    private GameObject heal;

    private void Awake()
    {
        jpap = GetComponent<JPap>();
    }

    public void SetupSouls()
    {
        GameObject soulHolder = new GameObject("SoulHolder");
        DontDestroyOnLoad(soulHolder);

        foreach (GameObject go in souls)
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(go, soulHolder.transform);
                Soul s = g.GetComponent<Soul>();
                s.InitializeSoul(jpap);
                g.SetActive(false);
                pooledSouls.Add(s);
            }
        }

        //heal = Instantiate(healingParticle.gameObject, soulHolder.transform);
        //heal.SetActive(false);

        BattleManager.OnEnemyKilled += SpawnSoul;
    }

    public void SpawnSoul(Enemy e, Player p)
    {
        foreach (Soul soul in pooledSouls)
        {
            if (!soul.isActiveAndEnabled)
            {
                soul.transform.position = e.transform.position;
                soul.ActivateAsHarvestable();
                return;
            }
        }
    }

    public void SpawnAttackSoul(Enemy e, float power)
    {
        foreach (Soul s in pooledSouls)
        {
            if (!s.isActiveAndEnabled)
            {
                attackSouls.Add(s);
                float x = Random.Range(-1f, 1f);
                float y = Random.Range(0.6f, 1.5f);
                float z = Random.Range(-1f, 1f);
                s.transform.position = transform.position + new Vector3(x, y, z);
                s.ActivateAsAttack(e, power);
                return;
            }
        }
    }

    int attackIterator = 0;
    public void ChargeAttackSoul(float charge)
    {
        attackSouls[attackIterator].Overcharge(charge);
        attackIterator = ++attackIterator == attackSouls.Count ? 0 : attackIterator;
    }
    public IEnumerator ShootSouls(bool success)
    {
        for (int i = 0; i < attackSouls.Count; i++)
        {
            attackSouls[i].Shoot(success);
            yield return new WaitForSeconds(0.1f);
        }

        attackIterator = 0;
        attackSouls.Clear();
    }

    public void ShieldAlly(Player target, int power)
    {
        foreach(Soul s in pooledSouls)
        {
            if (!s.isActiveAndEnabled)
            {
                s.ActivateAsShield(target, power);
                return;
            }
        }
    }
}
