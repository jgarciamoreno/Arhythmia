using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Arhythmia;

public class BeatSynch : MonoBehaviour {

    private double beat;
    private double timer;

    private bool isMusicPlaying = false;

    public delegate void SongBeat();
    public static event SongBeat OnSongBeat;

    public void SetupBeatTime()
    {
        beat = 60f / (MusicController.bpm * BeatDecimalValues.values[(int)BeatValue.QuarterBeat]);
        timer = beat;
    }

    private void OnEnable()
    {
        MusicController.OnAudioStart += MusicController_OnAudioStart;
    }
    private void OnDisable()
    {
        MusicController.OnAudioStart -= MusicController_OnAudioStart;
    }

    private void MusicController_OnAudioStart()
    {
        isMusicPlaying = true;
    }

    void Update ()
    {
        if (isMusicPlaying)
        {
            timer += Time.deltaTime;
            if (timer >= beat)
            {
                if (OnSongBeat != null)
                    OnSongBeat();

                timer = 0;
            }
        }
	}
}
