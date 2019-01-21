using Arhythmia;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePooler : MonoBehaviour
{
    private int currentDown = 0;
    private int currentLeft = 0;
    private int currentRight = 0;
    private int currentUp = 0;

    private int bulletToShoot;

    private List<int> downNotes = new List<int>();
    private List<int> leftNotes = new List<int>();
    private List<int> rightNotes = new List<int>();
    private List<int> upNotes = new List<int>();

    private List<GameObject> downRays = new List<GameObject>();
    private List<GameObject> leftRays = new List<GameObject>();
    private List<GameObject> rightRays = new List<GameObject>();
    private List<GameObject> upRays = new List<GameObject>();

    private List<GameObject> hitParticles = new List<GameObject>();

    private List<TypeOfInput> inputList = new List<TypeOfInput>();
    private List<BeatValue> notesValue = new List<BeatValue>();

    public List<GameObject> BulletsToPool = new List<GameObject>(); //List of tap/release bullets that can be pooled

    public List<GameObject> RaysToPool = new List<GameObject>(); // List of hold rays that can be pooled
    public List<GameObject> ParticlesToPool = new List<GameObject>(); // List of particles that can be pooled

    public Dictionary<int, Projectile> bulletsInDict = new Dictionary<int, Projectile>();
    public Dictionary<int, Projectile> initialDict = new Dictionary<int, Projectile>();

    public void SetBullets(List<Note> nl, PatternSynch ps)
    {
        //Split each color into its own list
        int colorCounter = 0;
        int restCounter = 0;
        foreach (Note n in nl)
        {
            bool toAdd = false;
            switch (n.GetColor())
            {
                case NoteColor.RIGHT:
                    rightNotes.Add(colorCounter);
                    toAdd = true;
                    break;
                case NoteColor.LEFT:
                    leftNotes.Add(colorCounter);
                    toAdd = true;
                    break;
                case NoteColor.DOWN:
                    downNotes.Add(colorCounter);
                    toAdd = true;
                    break;
                case NoteColor.UP:
                    upNotes.Add(colorCounter);
                    toAdd = true;
                    break;
                case NoteColor.NONE:
                    restCounter++;
                    toAdd = false;
                    break;
            }

            if (toAdd)
            {
                inputList.Add(nl[colorCounter + restCounter].GetTypeOfInput());
                notesValue.Add(nl[colorCounter + restCounter].GetBeatValue());
                colorCounter++;
            }
        }

        //Create the pool of reusable objects
        Transform parent = GameObject.Find("NotesPathP" + ps.Player.PlayerNumber).transform; ///Sacar después para que funcione con el menú

        for (int i = 0; i < 16; i++) //Normal notes
        {
            for (int j = 0; j < BulletsToPool.Count; j++)
            {
                GameObject obj = Instantiate(BulletsToPool[j]); //Instantiate notes
                Projectile p = obj.GetComponent<Projectile>(); //Get projectile component
                obj.transform.SetParent(parent.Find("Notes")); //Set as child of note for organization
                p.Source = Projectile.Projectile_Source.PLAYER; //Every note begins as a player source
                p.AssignMaterial(0); //Self explanatory
                obj.SetActive(false);
                ReAssignValues(ps, p);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < RaysToPool.Count; j++)
            {
                GameObject obj = Instantiate(RaysToPool[j]);
                obj.SetActive(false);
                OrderRays(obj);
            }
        }

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < ParticlesToPool.Count; j++)
            {
                GameObject obj = Instantiate(ParticlesToPool[j]);
                obj.transform.SetParent(GameObject.Find("Particles").transform);
                obj.SetActive(false);
                hitParticles.Add(obj);
            }
        }

        initialDict = bulletsInDict; //Save config of initial Dictionary in case of restarting level
    }

    public int BulletToShoot { get { return bulletToShoot; } set { bulletToShoot = value; } }
    public Projectile GetBullet() { return bulletsInDict[bulletToShoot]; }
    public Projectile GetBullet(int bulletIndex) { return bulletsInDict[bulletIndex]; }
    public Projectile GetBulletToDestroy(int i)
    {
        if (bulletsInDict.ContainsKey(i))
            return bulletsInDict[i].GetComponent<Projectile>();

        return null;
    }

    public GameObject GetRays(string tag)
    {
        switch (tag)
        {
            case "DOWN":
                for (int i = 0; i < downRays.Count; i++)
                    if (!downRays[i].activeInHierarchy)
                        return downRays[i];
                break;
            case "Left_Normal":
                for (int i = 0; i < leftRays.Count; i++)
                    if (!leftRays[i].activeInHierarchy)
                        return leftRays[i];
                break;
            case "Right_Normal":
                for (int i = 0; i < rightRays.Count; i++)
                    if (!rightRays[i].activeInHierarchy)
                        return rightRays[i];
                break;
            case "Up_Normal":
                for (int i = 0; i < upRays.Count; i++)
                    if (!upRays[i].activeInHierarchy)
                        return upRays[i];
                break;
        }
        return null;
    }
    public GameObject GetHitParticle()
    {
        GameObject hit;

        for (int i = 0; i < hitParticles.Count; i++)
        {
            if (!hitParticles[i].activeInHierarchy)
            {
                hit = hitParticles[i];
                return hit;
            }
        }
        return null;
    }

    //Reassigns the id of the deactivated bullet so it can be shoot again in the correct time
    public void ReAssignValues(PatternSynch ps, Projectile _proj)
    {
        Projectile p = _proj;
        if (p.isActiveAndEnabled)
            return;

        else
        {
            switch (_proj.tag)
            {
                case "RIGHT":
                    if (rightNotes.Count > 0)
                    {
                        p.Id = rightNotes[currentRight];
                        p.noteColor = NoteColor.RIGHT;
                        currentRight = (++currentRight == rightNotes.Count ? 0 : currentRight);
                    }
                    else
                    {
                        p.Id = -1;
                    }
                    break;
                case "LEFT":
                    if (leftNotes.Count > 0)
                    {
                        p.Id = leftNotes[currentLeft];
                        p.noteColor = NoteColor.LEFT;
                        currentLeft = (++currentLeft == leftNotes.Count ? 0 : currentLeft);
                    }
                    else
                    {
                        p.Id = -1;
                    }
                    break;
                case "DOWN":
                    if (downNotes.Count > 0)
                    {
                        p.Id = downNotes[currentDown];
                        p.noteColor = NoteColor.DOWN;
                        currentDown = (++currentDown == downNotes.Count ? 0 : currentDown);
                    }
                    else
                    {
                        p.Id = -1;
                    }
                    break;
                case "UP":
                    if (upNotes.Count > 0)
                    {
                        p.Id = upNotes[currentUp];
                        p.noteColor = NoteColor.UP;
                        currentUp = (++currentUp == upNotes.Count ? 0 : currentUp);
                    }
                    else
                    {
                        p.Id = -1;
                    }
                    break;
            }

            if (p.Id != -1)
            {
                p.InputType = inputList[p.Id];
                p.NoteValue = notesValue[p.Id];
                p.destroyed = false;

                if (p.InputType == TypeOfInput.HOLD)
                    p.HoldTime = (float)ps.samplePeriods[p.Id];
            }
        }

        if (p.Id != -1)
            bulletsInDict[p.Id] = _proj;
    }

    //Stores rays by colors
    private void OrderRays(GameObject obj)
    {
        switch (obj.tag)
        {
            case "GREEN_RAY":
                downRays.Add(obj);
                break;
            case "BLUE_RAY":
                leftRays.Add(obj);
                break;
            case "RED_RAY":
                rightRays.Add(obj);
                break;
            case "YELLOW_RAY":
                upRays.Add(obj);
                break;
            default:
                break;
        }
    }

    public void Reset()
    {
        currentRight = 0;
        currentDown = 0;
        currentLeft = 0;
        currentUp = 0;

        bulletToShoot = 0;
        //bulletsInDict = initialDict;
    }
}