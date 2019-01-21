using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using Arhythmia;

[CustomEditor(typeof(Enemy), true)]
public class EnemyEditor : Editor
{
    #region Properties
    SerializedProperty attackDmg;
    SerializedProperty defencePwr;
    SerializedProperty attackPeriodList;
    SerializedProperty waitToAttackPeriodList;
    SerializedProperty normalSpeed;
    SerializedProperty chaseSpeed;
    SerializedProperty lockedSpeed;
    SerializedProperty turnSpeed;
    SerializedProperty paralysisRes;
    SerializedProperty burnRes;
    SerializedProperty breakRes;
    SerializedProperty recoveryRate;
    #endregion
    Enemy enemy;
    bool HasTargetPriority;
    Enemy.PriorityTarget firstTarget = Enemy.PriorityTarget.NONE;

    enum Aggro { TIER1, TIER2, TIER3, TIER4, BOSS }
    Aggro aggro;
    int aggroLvl = 0;
    Aggro previousTier;

    bool statsDisplay;

    private void OnEnable()
    {
        enemy = (Enemy)target;
        attackPeriodList = serializedObject.FindProperty("attackPeriod");
        waitToAttackPeriodList = serializedObject.FindProperty("waitToAttackPeriod");
        attackDmg = serializedObject.FindProperty("attackDamage");
        defencePwr = serializedObject.FindProperty("defencePower");
        normalSpeed = serializedObject.FindProperty("normalMovementSpeed");
        chaseSpeed = serializedObject.FindProperty("chaseMovementSpeed");
        lockedSpeed = serializedObject.FindProperty("onLockMovementSpeed");
        turnSpeed = serializedObject.FindProperty("turnSpeed");
        paralysisRes = serializedObject.FindProperty("paralysisResistance");
        burnRes = serializedObject.FindProperty("burnResistance");
        breakRes = serializedObject.FindProperty("armorBreakResistance");
        recoveryRate = serializedObject.FindProperty("statusRecoverySpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //base.OnInspectorGUI();

        statsDisplay = EditorGUILayout.Foldout(statsDisplay, "Stats");
        if (statsDisplay)
        {
            EditorGUILayout.BeginVertical("box");
            enemy.maxHealthPoints = EditorGUILayout.FloatField("Max HP", enemy.maxHealthPoints);
            EditorGUILayout.PropertyField(attackDmg);
            EditorGUILayout.PropertyField(defencePwr);
            EditorGUILayout.PropertyField(normalSpeed);
            EditorGUILayout.PropertyField(chaseSpeed);
            EditorGUILayout.PropertyField(lockedSpeed);
            EditorGUILayout.PropertyField(turnSpeed);
            EditorGUILayout.PropertyField(paralysisRes);
            EditorGUILayout.PropertyField(burnRes);
            EditorGUILayout.PropertyField(breakRes);
            EditorGUILayout.PropertyField(recoveryRate);
            EditorGUILayout.EndVertical();
        }

        enemy.AttackType = (Enemy.RhythmAttackType)EditorGUILayout.EnumPopup("Attack Mode", enemy.AttackType);
        enemy.attackDamageType = (Character.TypeOfDamage)EditorGUILayout.EnumPopup("Damage Type", enemy.attackDamageType);
        if (enemy.AttackType == Enemy.RhythmAttackType.NOTES)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(attackPeriodList, true);
            EditorGUILayout.PropertyField(waitToAttackPeriodList, true);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.BeginVertical("box");
            enemy.beatsToAttack = EditorGUILayout.IntField("Beats To Attack", enemy.beatsToAttack);
            EditorGUILayout.EndVertical();
        }

        HasTargetPriority = EditorGUILayout.Toggle("Has Target Priority", HasTargetPriority);
        if (HasTargetPriority)
        {
            EditorGUILayout.BeginVertical("box");
            enemy.priorities[0] = (Enemy.PriorityTarget)EditorGUILayout.EnumPopup("Primary Target", firstTarget);
            enemy.priorities[1] = (Enemy.PriorityTarget)EditorGUILayout.EnumPopup("Secondary Target", enemy.priorities[1]);
            enemy.priorities[2] = (Enemy.PriorityTarget)EditorGUILayout.EnumPopup("Third Target", enemy.priorities[2]);
            enemy.priorities[3] = (Enemy.PriorityTarget)EditorGUILayout.EnumPopup("Fourth Target", enemy.priorities[3]);
            EditorGUILayout.EndVertical();
            firstTarget = enemy.priorities[0];
        }
        else
        {
            enemy.priorities[0] = Enemy.PriorityTarget.NONE;
        }

        aggro = (Aggro)EditorGUILayout.EnumPopup("Aggro", aggro);
        if (aggro != previousTier)
        {
            switch (aggro)
            {
                case Aggro.TIER1:
                    aggroLvl = Random.Range(1, 20);
                    previousTier = aggro;
                    break;
                case Aggro.TIER2:
                    aggroLvl = Random.Range(20, 50);
                    previousTier = aggro;
                    break;
                case Aggro.TIER3:
                    aggroLvl = Random.Range(50, 80);
                    previousTier = aggro;
                    break;
                case Aggro.TIER4:
                    aggroLvl = Random.Range(80, 100);
                    previousTier = aggro;
                    break;
                case Aggro.BOSS:
                    aggroLvl = 100;
                    previousTier = aggro;
                    break;
            }
            enemy.aggro = aggroLvl;
        }
        //EditorGUILayout.HelpBox(enemy.aggro.ToString(), MessageType.None);

        enemy.lockPosition = (Transform)EditorGUILayout.ObjectField("Player Lock Position", enemy.lockPosition, typeof(Transform), true);
        serializedObject.ApplyModifiedProperties();
    }
}
