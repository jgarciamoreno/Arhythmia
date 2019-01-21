using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (PlayerDetection))]
public class EnemyFOVEditor : Editor {

    void OnSceneGUI()
    {
        PlayerDetection fov = (PlayerDetection)target;
        Handles.color = Color.red;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.attackRadius);
        Handles.color = Color.yellow;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
        Handles.color = new Color(0, 1, 1, .2f);
        Vector3 viewAngleA = fov.DirectionFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirectionFromAngle(fov.viewAngle / 2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);
        Handles.DrawSolidArc(fov.transform.position, fov.transform.up, viewAngleA, fov.viewAngle, fov.viewRadius);

        foreach(Transform visibleTarget in fov.visibleTargets)
        {
            if (Vector3.Distance(fov.transform.position, visibleTarget.position) < fov.attackRadius)
                Handles.color = Color.red;
            else
                Handles.color = Color.yellow;

            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
    }
}
