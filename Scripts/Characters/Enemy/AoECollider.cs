using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoECollider : MonoBehaviour {

    public List<Player> playersInRange = new List<Player>();
    //private Enemy enemy;

    //private void Awake()
    //{
    //    enemy = GetComponentInParent<Enemy>();
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playersInRange.Add(other.GetComponent<Player>());
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            playersInRange.Remove(p);
        }
    }
}
