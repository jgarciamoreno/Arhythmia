using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arhythmia;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    private double[] samplePeriods;

    public SongMap songMap;

    public static float bpm = 60f;
    private float startDelay = 6f;

    private enum RythmLines { MELODY, ARMONIZER, BASS, PERCUSSION }
    private RythmLines[] chosenLines;

    public delegate void AudioStartAction();
    public static event AudioStartAction OnAudioStart;

    private bool paused = false;

    private AudioSource[] audioS;
    public CurrentAudio currentAudioPlaying;
    public enum CurrentAudio { OVERWORLD, BATTLE }

    private void Awake()
    {
        bpm = songMap.BPM;
        audioS = GetComponents<AudioSource>();
        currentAudioPlaying = CurrentAudio.OVERWORLD;
    }

    private void Start()
    {
        ReadSongNotes();

        double startTime = AudioSettings.dspTime;

        SetNoteTimes(startTime);

        StartCoroutine(StartAudio(startTime));
    }

    private void ReadSongNotes()
    {
        foreach (PatternSynch ps in FindObjectsOfType<PatternSynch>())
        {
            List<Note> notesList = new List<Note>();

            switch (ps.LineSync)
            {
                case PatternSynch.LineSynch.MELODY:
                    notesList = songMap.getMelodyNotes();
                    break;
                case PatternSynch.LineSynch.ARMONIZER:
                    notesList = songMap.getArmonizerNotes();
                    break;
                case PatternSynch.LineSynch.BASS:
                    notesList = songMap.getBassNotes();
                    break;
                case PatternSynch.LineSynch.PERCUSSION:
                    notesList = songMap.getPercussionNotes();
                    break;
            }

            if (notesList.Count != 0)
                SetNoteValues(notesList, ps);
        }
    }
    private void SetNoteValues(List<Note> nl, PatternSynch ps)
    {
        bool[] restNotes = new bool[nl.Count];
        BeatValue[] beatValues = new BeatValue[nl.Count];
        BeatValue[] leggatoValues = new BeatValue[nl.Count];
        BeatValue[] secondLeggatoValues = new BeatValue[nl.Count];
        for (int i = 0; i < nl.Count; i++)
        {
            if (nl[i].GetColor() == NoteColor.NONE)
                restNotes[i] = true;
            else
                restNotes[i] = false;

            beatValues[i] = nl[i].GetBeatValue();
            leggatoValues[i] = nl[i].GetLegatto();
            secondLeggatoValues[i] = nl[i].GetSecondLegatto();
        }

        samplePeriods = new double[beatValues.Length];

        // Calculate number of samples between each beat in the sequence.
        for (int i = 0; i < beatValues.Length; ++i)
        {
            if (leggatoValues[i] == BeatValue.None)
                samplePeriods[i] = 60f / (bpm * BeatDecimalValues.values[(int)beatValues[i]]);

            else if (secondLeggatoValues[i] == BeatValue.None)
                samplePeriods[i] = 60f / (bpm * BeatDecimalValues.values[(int)beatValues[i]]) +
                                   60f / (bpm * BeatDecimalValues.values[(int)leggatoValues[i]]);
            else
                samplePeriods[i] = 60f / (bpm * BeatDecimalValues.values[(int)beatValues[i]]) +
                                   60f / (bpm * BeatDecimalValues.values[(int)leggatoValues[i]]) +
                                   60f / (bpm * BeatDecimalValues.values[(int)secondLeggatoValues[i]]);
        }

        ps.SetSamplePeriods(samplePeriods, restNotes);
        //ps.gameObject.GetComponent<NotePooler>().SetBullets(nl, ps);
    }

    private void SetNoteTimes(double startTime)
    {
        foreach (PatternSynch ps in FindObjectsOfType<PatternSynch>())
            ps.SetNoteTimes(startTime + startDelay);
    }

    public void SetCharactersBeatDependencies()
    {
        foreach (Character c in FindObjectsOfType<Character>())
            c.GetComponent<Character>().SetBeatBasedVariables(60f / (bpm * BeatDecimalValues.values[(int)BeatValue.QuarterBeat]));
    }

    public void Dead()
    {
        FindObjectOfType<PatternSynch>().slowMo = true;
        if (Time.timeScale > 0.5f)
        {
            Time.timeScale -= 0.05f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            audioS[(int)currentAudioPlaying].pitch -= 0.05f;
        }
    }

    public void Pause()
    {
        paused = !paused;

        if (paused)
            audioS[(int)currentAudioPlaying].Pause();
        else
            audioS[(int)currentAudioPlaying].UnPause();
    }

    public void ResetComponent()
    {
        audioS[(int)currentAudioPlaying].UnPause();
        audioS = GetComponents<AudioSource>();

        FindObjectOfType<NotePooler>().Reset();

        double startTime = AudioSettings.dspTime;
        SetNoteTimes(startTime);
        StartCoroutine(StartAudio(startTime));
    }

    private IEnumerator StartAudio(double startTime)
    {
        yield return new WaitForSecondsRealtime(startDelay);
        if (currentAudioPlaying == CurrentAudio.BATTLE)
        {
            audioS[0].volume = 0;
            audioS[1].volume = 1;
        }
        audioS[(int)currentAudioPlaying].Play();
        OnAudioStart();
    }
    public IEnumerator PowerCrossFade()
    {
        float i = 0f;

        CurrentAudio audioToSwitch;
        audioToSwitch = currentAudioPlaying == CurrentAudio.OVERWORLD ? CurrentAudio.BATTLE : CurrentAudio.OVERWORLD;

        audioS[(int)audioToSwitch].timeSamples = audioS[(int)currentAudioPlaying].timeSamples;
        audioS[(int)audioToSwitch].Play();

        while (paused)
            yield return null;

        while (i < 1f)
        {
            i += 1.25f * Time.deltaTime;
            audioS[(int)currentAudioPlaying].volume = Mathf.Lerp(1f, 0f, i);
            audioS[(int)audioToSwitch].volume = Mathf.Lerp(0f, 1f, i);
            yield return new WaitForSeconds(0);
        }

        audioS[(int)currentAudioPlaying].Stop();
        currentAudioPlaying = audioToSwitch;

        GameManager.INSTANCE.fadeEnded = true;
    }
}
