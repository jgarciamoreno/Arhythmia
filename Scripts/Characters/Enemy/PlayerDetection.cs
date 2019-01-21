using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDetection : MonoBehaviour
{
    private Enemy enemy;
    public float viewRadius;
    public float attackRadius;
    [Range(0, 360)]
    public float viewAngle;
    [Range(0, 180)]
    public float AoEAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    private Transform currentTarget;
    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution;


    public int edgeResolveIterations;
    public float edgeDistanceThreshold;

    private void Start()
    {
        enemy = GetComponent<Enemy>();

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

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, attackRadius, obstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        else
            return new ViewCastInfo(false, transform.position + dir * attackRadius, attackRadius, globalAngle);
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private void FindVisibleTargets()
    {
        if(!enemy.playerLocked)
        {
            visibleTargets.Clear();
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform target = targetsInViewRadius[i].transform;
                if (!target.GetComponent<Player>().IsTargetable)
                    continue;

                Vector3 dirToTarget = (target.position - transform.position).normalized;
                float distToTarget = Vector3.Distance(transform.position, target.position);

                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        visibleTargets.Add(target);

                        enemy.SetTargetPlayer(target.GetComponent<Player>());
                    }
                }
            }
        }
        else
        {
            visibleTargets.Clear();
            //if(!enemy.TargetPlayer.IsTargetable)
            //{

            //}
            Vector3 dirToTarget = (enemy.TargetPlayer.transform.position - transform.position).normalized;
            float distToTarget = Vector3.Distance(transform.position, enemy.TargetPlayer.transform.position);

            if (Vector3.Angle(transform.forward, dirToTarget) > viewAngle / 2)
            {
                enemy.TargetLost();
            }
            else
            {
                if (Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    enemy.TargetLost();
                else
                    enemy.isTargetPlayerVisible = true;
            }
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
