using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arhythmia;

public class PatternSynch : MonoBehaviour
{
    public enum LineSynch { MELODY, ARMONIZER, BASS, PERCUSSION }
    public LineSynch LineSync;
    private bool isInLineSynch = false;
    public bool playerLocked = false;
    private bool musicPlaying = true;
    public bool slowMo = false;
    private BeatValue playerSpeed;

    #region StickVars
    private Enemy currentAttackingEnemy;
    [HideInInspector] public Projectile stickNote;
    private double enemyAttackStartThreshold;
    #endregion
    #region NotesVars
    [HideInInspector]
    public double[] samplePeriods;
    [HideInInspector]
    public List<double> noteTimes;
    private bool[] isRest;
    public const BeatValue beatOffset = BeatValue.WholeDottedBeat;
    [HideInInspector]
    public double offset;

    private double nextBeat;

    private int nextNote = 0;
    private int maxNotes;
    #endregion
    #region ObjectReferences
    private NotePooler notePooler;
    private PatternController patternController;
    public Player Player { get; set; }
    private Enemy[] EnemiesInSync;
    #endregion
    #region NoteSyncLists    
    public List<Projectile> activeColorNotes = new List<Projectile>();
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        patternController = GetComponent<PatternController>();
        notePooler = GetComponent<NotePooler>();
    }

    public void SetObjectsReferences()
    {
        offset = 60f / (MusicController.bpm * BeatDecimalValues.values[(int)beatOffset]);
        patternController.SetOffsetTolerance();
        Player.PatternSynchNumber = (int)LineSync;
        playerSpeed = Player.attackSpeed;
    }

    public void SetSamplePeriods(double[] sp, bool[] ir)
    {
        samplePeriods = sp;
        isRest = ir;
    }
    public void SetNoteTimes(double nextBeatSample)
    {
        noteTimes = new List<double>();
        for (int i = 0; i < samplePeriods.Length; i++)
        {
            if (!isRest[i])
                noteTimes.Add(nextBeatSample);

            nextBeatSample += samplePeriods[i];
        }

        maxNotes = noteTimes.Count;

        enemyAttackStartThreshold = 60f / (MusicController.bpm * BeatDecimalValues.values[(int)BeatValue.EighthDottedBeat]);
    }

    private void OnEnable()
    {
        MusicController.OnAudioStart += StartAudioSynch;
    }
    private void OnDisable()
    {
        MusicController.OnAudioStart -= StartAudioSynch;
    }
    private void StartAudioSynch()
    {
        musicPlaying = true;
    }

    private void Update()
    {
        if (musicPlaying)
        {
            nextBeat = (noteTimes[nextNote] - offset);

            if (AudioSettings.dspTime >= nextBeat)
            {
                //patternController.UpdateBulletToShoot();
                //GameManager.INSTANCE.currentGameState.OnPatternBeat(this);

                //nextNote = (++nextNote == maxNotes ? 0 : nextNote);
            }
        }
    }

    public void OnBattlePatternBeat()
    {
        Projectile proj = notePooler.GetBullet();

        activeColorNotes.Add(proj);

        StartCoroutine(WaitToReassign(proj, offset + patternController.offsetTolerance * 1.5f));
    }

    public IEnumerator WaitToReassign(Projectile p, double remainingTime)
    {
        double t = 0;
        while (t < remainingTime)
        {
            t += Time.deltaTime; //Wait until it's time to destroy the note
            yield return null;
        }

        if (!p.destroyed) //If note hasn't been destroyed by player reassign value
        {
            activeColorNotes.RemoveAt(0); //Remove note from active list
            
            patternController.UpdateBulletToDestroy(); //If note hasn't been destroyed by player update ToDestroy index
            p.gameObject.SetActive(false); //And deactivate object

            Player.Miss();
        }

        notePooler.ReAssignValues(this, p);
    }

    public void SwitchSyncronizerSource()
    {
        isInLineSynch = !isInLineSynch;
    }

    public void DestroyNote()
    {
        activeColorNotes.RemoveAt(0);

        if (currentAttackingEnemy != null)
            currentAttackingEnemy.currentState.StateCountdown();
    }

    public int CalculateNotesInTime(double maxTime)
    {
        return CalculateNotesInTime(null, maxTime, false)[1];
    }
    public int CalculateNotesInTime(Enemy e, double maxTime)
    {
        return CalculateNotesInTime(e, maxTime, false)[1];
    }
    public int[] CalculateNotesInTime(Enemy e, double maxTime, bool firstAttack)
    {
        if (e != null)
            currentAttackingEnemy = e;

        int notesQty = 0;
        int delay = 0;
        int counter = patternController.GetBulletToDestroy();
        double time = AudioSettings.dspTime;
        maxTime += time;
        while (noteTimes[counter] < maxTime)
        {
            if (firstAttack)
            {
                if (noteTimes[counter] <= time + enemyAttackStartThreshold)
                {
                    delay = 1;
                    notesQty--;
                }

                firstAttack = false;
            }

            counter++;
            notesQty++;
        }
        
        return new int[] { delay, notesQty };
    }

    public void Reset()
    {
        StopAllCoroutines();
        activeColorNotes.Clear();
        nextNote = 0;
        Player.Reset();
        notePooler.Reset();
    }
}
