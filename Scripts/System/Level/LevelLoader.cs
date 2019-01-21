using UnityEngine;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour {

    public List<Transform> playersSpawn = new List<Transform>();
    public List<AreaSpawner> Zones = new List<AreaSpawner>();
    
    public void PlayerSpawn(List<GameObject> players)
    {
        for (int i = 0; i < players.Count; i++)
        {
            //Get player joystick number
            int p = players[i].GetComponent<Player>().PlayerNumber;

            //Put player in spawn poisition
            players[p - 1].transform.position = playersSpawn[i].position;
            players[p - 1].transform.rotation = playersSpawn[i].rotation;
        }
    }

    public void EnemySpawn(int level)
    {
        //string currentLevel = string.Empty;

        switch (level)
        {
            case 1:
                //currentLevel = electroLevel;
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
        }

        foreach (AreaSpawner asp in Zones)
        {
            asp.SpawnEnemies();
        }
    }
}
