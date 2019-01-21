using UnityEngine;
using Arhythmia;

public class PatternController : MonoBehaviour
{
    protected int bulletToShoot = 0;
    protected int bulletToDestroy = 0;
    protected int maxBullets;

    public float offsetTolerance;

    public const BeatValue beatOffsetTolerance = BeatValue.ThirtysecondDotted;
    #region ObjectsReferences
    protected Projectile projectile;
    protected PatternSynch patternSynch;
    protected NotePooler objectPooler;
    #endregion
    protected virtual void Awake()
    {
        patternSynch = GetComponent<PatternSynch>();
        objectPooler = GetComponent<NotePooler>();
    }

    public void SetOffsetTolerance()
    {
        offsetTolerance = 60f / (MusicController.bpm * BeatDecimalValues.values[(int)beatOffsetTolerance]);
    }

    public int CheckBulletToDestroy(NoteColor c, TypeOfInput t, float time)
    {
        double pressedTime = patternSynch.noteTimes[bulletToDestroy] - AudioSettings.dspTime;
        pressedTime = pressedTime < 0 ? -pressedTime : pressedTime;

        if (pressedTime <= offsetTolerance)
        {
            projectile = objectPooler.GetBulletToDestroy(bulletToDestroy);
            if (projectile == null)
                return -1;

            else if ((projectile.isActiveAndEnabled && c.ToString().Equals(projectile.tag) && t == projectile.InputType))
            {
                if (projectile.InputType != TypeOfInput.HOLD)
                {
                    projectile.DestroyOnHit();
                    UpdateBulletToDestroy();
                    patternSynch.DestroyNote();
                    //if (projectile.Source != Projectile.Projectile_Source.ENEMY)
                    //{
                        if (pressedTime < offsetTolerance / 4.5f)
                        {
                            //Debug.Log("PERFECT");
                            return 1;
                        }
                        else if (pressedTime >= offsetTolerance / 4 && pressedTime < offsetTolerance / 3)
                        {
                            //Debug.Log("GOOD");
                            return 2;
                        }
                        else if (pressedTime >= offsetTolerance / 3 && pressedTime < offsetTolerance / 2)
                        {
                            //Debug.Log("MEH");
                            return 3;
                        }
                        else if (pressedTime >= offsetTolerance / 2)// && pressedTime < offsetTolerance / 1.5f)
                        {
                            //Debug.Log("BAD");
                            return 4;
                        }
                    //}
                    //else
                    //{
                        if (pressedTime < offsetTolerance / 4)
                        {
                            //Debug.Log("PERFECT");
                            return -1;
                        }
                        else if (pressedTime >= offsetTolerance / 4 && pressedTime < offsetTolerance / 3)
                        {
                            //Debug.Log("GOOD");
                            return -2;
                        }
                        else if (pressedTime >= offsetTolerance / 3 && pressedTime < offsetTolerance / 2)
                        {
                            //Debug.Log("MEH");
                            return -3;
                        }
                        else if (pressedTime >= offsetTolerance / 2)// && pressedTime < offsetTolerance / 1.5f)
                        {
                            //Debug.Log("BAD");
                            return -4;
                        }
                    //}
                }
            }
        }

        return 0;
    }

    public int CheckStickToDestroy(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return -1;

        double pressedTime = patternSynch.noteTimes[bulletToDestroy] - AudioSettings.dspTime;
        pressedTime = pressedTime < 0 ? -pressedTime : pressedTime;

        if (pressedTime <= offsetTolerance)
        {
            projectile = objectPooler.GetBulletToDestroy(bulletToDestroy);
            if (projectile == null)
                return -1;

            if(projectile.isActiveAndEnabled && projectile.isStick)
            {
                projectile.DestroyOnHit();
                UpdateBulletToDestroy();
                patternSynch.DestroyNote();
                return 1;
            }
        }

        return 0;
    }

    public void UpdateBulletToDestroy()
    {
        bulletToDestroy = (++bulletToDestroy == maxBullets ? --bulletToDestroy : bulletToDestroy);
        //bulletToDestroy = (++bulletToDestroy == maxBullets ? 0 : bulletToDestroy);
    }
    public int GetBulletToDestroy()
    {
        return bulletToDestroy;
    }

    public void UpdateBulletToShoot()
    {
        objectPooler.BulletToShoot = bulletToShoot;
        //bulletToShoot = (++bulletToShoot == maxBullets ? 0 : bulletToShoot);
        bulletToShoot = (++bulletToShoot == maxBullets ? --bulletToShoot : bulletToShoot);
    }

    public void Reset()
    {
        bulletToDestroy = 0;
        bulletToShoot = 0;
        patternSynch.Reset();
    }
}
