using UnityEngine;

public class SquadFormation : MonoBehaviour
{
    public static Vector3 Position(Transform leader, int formation, int subordinate)
    {
        Vector3 pos = Vector3.zero;

        switch (formation)
        {
            case 1:
                pos = Diamond(leader, subordinate);
                break;
            case 2:
                pos = Square(leader, subordinate);
                break;
            case 3:
                pos = Triangle(leader, subordinate);
                break;
        }

        return pos;
    }

    private static Vector3 Diamond(Transform l, int n)
    {
        switch (n)
        {
            case 1:
                return l.position - l.forward - l.right;
            case 2:
                return /*new Vector3(l.position.x, 0, l.position.z)*/l.position - l.forward + l.right;
            case 3:
                return l.position - l.forward * 2;
            default:
                return Vector3.zero;
        }
    }

    private static Vector3 Square(Transform l, int n)
    {
        return Vector3.zero;
    }

    private static Vector3 Triangle(Transform l, int n)
    {
        return Vector3.zero;
    }
}
