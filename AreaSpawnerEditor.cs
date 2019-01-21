using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaSpawner))]
public class AreaSpawnerEditor : Editor {

    SerializedObject serializedObj;
    SerializedProperty maxLvl;
    SerializedProperty areaEnemies;
    //SerializedProperty spawnPoints;
    SerializedProperty waypoints;
    SerializedProperty squadsProperty;

    void OnEnable()
    {
        serializedObj = new SerializedObject(target);
        squadsProperty = serializedObj.FindProperty("Squads");
        areaEnemies = serializedObj.FindProperty("AvailableEnemiesInArea");
        maxLvl = serializedObj.FindProperty("MaxZoneLevel");
        //spawnPoints = serializedObj.FindProperty("SpawnPoints");
        waypoints = serializedObj.FindProperty("Waypoints");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(maxLvl);
        EditorGUILayout.PropertyField(areaEnemies, true);
        //EditorGUILayout.PropertyField(spawnPoints, true);
        EditorGUILayout.PropertyField(waypoints, true);
        EditorGUILayout.PropertyField(squadsProperty, true);

        serializedObj.ApplyModifiedProperties();
    }
}
