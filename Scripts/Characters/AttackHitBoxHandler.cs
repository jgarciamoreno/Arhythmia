using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitBoxHandler : MonoBehaviour {

    public DamageType[] damageTypes;

    private Character character;
    private BoxCollider coll;
    private int validAttackLayer = 8;

    private float playerBaseDamage;
    public float damage;

    public float effectiveTime;
    private float timer;

	void Start ()
    {
        character = transform.parent.GetComponent<Character>();
        coll = GetComponent<BoxCollider>();
        coll.enabled = false;
	}

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
                coll.enabled = false;
        }
    }

    public void Activate(float playerDamage)
    {
        playerBaseDamage = playerDamage;
        coll.enabled = false;
        timer = effectiveTime;
        coll.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == validAttackLayer)
            other.GetComponent<Character>().TakeDamage(damage, transform.position, Character.TypeOfDamage.NORMAL);
    }
}
