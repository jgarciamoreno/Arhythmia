using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (EnemyDetection))]
public class PlayerFOVEditor : Editor {

    void OnSceneGUI()
    {
        EnemyDetection fov = (EnemyDetection)target;
        Handles.color = Color.green;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
        Vector3 viewAngleA = fov.DirectionFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirectionFromAngle(fov.viewAngle / 2, false);

        Handles.color = new Color(0, 1, 1, .2f);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);
        Handles.DrawSolidArc(fov.transform.position, fov.transform.up, viewAngleA, fov.viewAngle, fov.viewRadius);
    }
}
