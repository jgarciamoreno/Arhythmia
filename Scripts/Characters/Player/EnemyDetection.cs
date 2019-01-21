using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDetection : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public Transform cam;

    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine(FindTargetsWithDelay(.2f));
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    visibleTargets.Add(target);
            }
        }
    }

    public Enemy FindTargetFromSides(Transform currentTarget, bool right)
    {
        Transform toTarget = null;
        Vector3 localPos;
        float nearestToLeft = Mathf.NegativeInfinity;
        float nearestToRight = Mathf.Infinity;

        foreach (Transform t in visibleTargets)
        {
            if (t == currentTarget)
                continue;

            localPos = cam.InverseTransformPoint(t.position);

            if (!right)
            {
                if (localPos.x > nearestToLeft && localPos.x < 0)
                {
                    nearestToLeft = localPos.x;
                    toTarget = t;
                }

                else if ((localPos.x > nearestToRight || nearestToRight == Mathf.Infinity) && nearestToLeft == Mathf.NegativeInfinity)
                {
                    nearestToRight = localPos.x;
                    toTarget = t;
                }
            }
            else
            {
                if (localPos.x < nearestToRight && localPos.x > 0)
                {
                    nearestToRight = localPos.x;
                    toTarget = t;
                }

                else if ((localPos.x < nearestToLeft || nearestToLeft == Mathf.NegativeInfinity) && nearestToRight == Mathf.Infinity)
                {
                    nearestToLeft = localPos.x;
                    toTarget = t;
                }
            }
        }

        return toTarget.GetComponent<Enemy>();
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
