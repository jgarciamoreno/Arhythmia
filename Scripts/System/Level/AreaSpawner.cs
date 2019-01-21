using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class AreaSpawner : MonoBehaviour
{
    public int MinZoneLevel;
    public int MaxZoneLevel;
    public List<GameObject> AvailableEnemiesInArea = new List<GameObject>();
    private List<Transform> SpawnPoints = new List<Transform>();
    public List<Transform> Waypoints = new List<Transform>();
    public List<Squad> Squads = new List<Squad>();

    private HashSet<int> exclude = new HashSet<int>();

    int squadIndex = 0;
    int indexToSpawn = 0;
    int lvlToSpawn = 0;

    private void Awake()
    {
        foreach (Transform t in transform.Find("SpawnPoints").GetComponentInChildren<Transform>())
            SpawnPoints.Add(t);
    }

    public void SpawnEnemies()
    {
        int lvlSum = 0;
        int estimatedLvl;

        foreach (Transform sp in SpawnPoints)
        {
            List<GameObject> go = new List<GameObject>();
            while (true)
            {
                if (squadIndex < Squads.Count)
                    go = TrySpawnSquad();
                if (go.Count == 0)
                    go.Add(SpawnUnit());

                lvlToSpawn = go[0].GetComponent<Enemy>().GetLevel();
                estimatedLvl = lvlSum + lvlToSpawn;

                if (estimatedLvl > MaxZoneLevel)
                    exclude.Add(indexToSpawn);
                else
                    break;
            }

            if (indexToSpawn == -1)
                break;

            lvlSum += lvlToSpawn;

            foreach (GameObject g in go)
            {
                g.transform.position = sp.position;
                g.transform.rotation = sp.rotation;
                if (sp.name.Contains("1") || sp.name.Contains("2"))
                {
                    foreach (Transform wp in Waypoints)
                    {
                        if (wp.name.Contains("Pattern1"))
                            g.GetComponent<Enemy>().patrolWaypoints.Add(wp);
                    }
                }
                else if (sp.name.Contains("3"))
                {
                    foreach (Transform wp in Waypoints)
                    {
                        if (wp.name.Contains("Pattern2"))
                            g.GetComponent<Enemy>().patrolWaypoints.Add(wp);
                    }
                }
            }

            go.Clear();
        }
    }

    private int RandomNumber()
    {
        IEnumerable<int> range = Enumerable.Range(0, AvailableEnemiesInArea.Count).Where(x => !exclude.Contains(x));

        if (range.Count() == 0)
            return -1;

        int randIndex = UnityEngine.Random.Range(0, AvailableEnemiesInArea.Count - exclude.Count);

        return range.ElementAt(randIndex);
    }

    private List<GameObject> TrySpawnSquad()
    {
        List<GameObject> go = new List<GameObject>();

        if (Squads[squadIndex].isForced)
        {
            for (int i = 0; i < Squads[squadIndex].enemies.Length; i++)
            {
                GameObject g = Instantiate(Squads[squadIndex].enemies[i]);
                Enemy e = g.GetComponent<Enemy>();
                e.isInSquad = true;
                e.FormationType = (int)Squads[squadIndex].formation;
                e.SubordinateN = i;
                go.Add(g);
                e.leaderPos = go[0].transform;
            }
        }
        else
        {
            if (UnityEngine.Random.value > Squads[squadIndex].rareness / 100f)
            {
                if (Squads[squadIndex].maxUnits == 0)
                {
                    for (int j = 0; j < Squads[squadIndex].enemies.Length; j++)
                    {
                        GameObject g = Instantiate(Squads[squadIndex].enemies[j]);
                        Enemy e = g.GetComponent<Enemy>();
                        e.isInSquad = true;
                        e.FormationType = (int)Squads[squadIndex].formation;
                        e.SubordinateN = j;

                        go.Add(g);
                        e.leaderPos = go[0].transform;
                    }
                }
                else
                {
                    int i = 0;
                    while (i < Squads[squadIndex].maxUnits)
                    {
                        int rnd = RandomNumber();
                        GameObject g = Instantiate(AvailableEnemiesInArea[rnd]);
                        Enemy e = g.GetComponent<Enemy>();
                        e.isInSquad = true;
                        e.FormationType = (int)Squads[squadIndex].formation;
                        e.SubordinateN = i;
                        go.Add(g);
                        e.leaderPos = go[0].transform;
                        i++;
                    }
                }

            }
        }

        for (int i = 0; i < go.Count; i++)
        {
            Enemy e = go[i].GetComponent<Enemy>();
            for (int j = 0; j < go.Count; j++)
            {
                if (j != i)
                    e.squadAllies.Add(go[j].GetComponent<Enemy>());
            }
        }

        squadIndex++;
        return go;
    }

    private GameObject SpawnUnit()
    {
        indexToSpawn = RandomNumber();
        if (indexToSpawn == -1)
            return null;

        GameObject g = Instantiate(AvailableEnemiesInArea[indexToSpawn]);
        g.GetComponent<Enemy>().isInSquad = false;
        return g;
    }
}

[Serializable]
public struct Squad
{
    public GameObject[] enemies;
    public enum Formation { DIAMOND = 1, SQUARE = 2, TRIANGLE = 3 };
    public Formation formation;
    public bool isForced;
    public int maxUnits;
    public int rareness;
}
