using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public bool muted = false;

    [System.Serializable]
    public class Chord {
        public AudioClip[] clips;
    }

    public Chord[] chords;

    [System.Serializable]
    public class ChordTiming {
        public int beats = 1;
        public int chordId = 0;
    }
    public ChordTiming[] chordProgression;

    float bpm {
        get {
            return Mathf.Lerp(minMaxBPM.x, minMaxBPM.y, (Mathf.Sin(Time.time * 2f * Mathf.PI / bpmPeriod) + 1f) / 2f);
        }
    }

    public Vector2Int minMaxBPM = new Vector2Int(23, 27);
    public float bpmPeriod = 120f;

    int chordIt = 0;

    public AudioSource[] musicSources;
    int musicSourceIt = 0;

    float SecondsPerBeat {
        get {
            return 60f / bpm;
        }
    }
    float timeSinceLastBeat = 0;
    int lastChordBeatDuration = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    int chordsPlayed = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            muted = !muted;
            if (muted)
            {
                foreach (AudioSource s in musicSources)
                    if (s.isPlaying) s.Stop();
            }
            else {

            }
        }

        timeSinceLastBeat += Time.deltaTime;

        if (muted)
        {
            return;
        }
        //Debug.Log(bpm);

        if (timeSinceLastBeat >= SecondsPerBeat * lastChordBeatDuration)
        {
            timeSinceLastBeat = 0;// timeSinceLastBeat % SecondsPerBeat;

            //
            AudioSource s = GetNextAudioSource();
            ChordTiming c = GetNextChord();

            int whichClip = Random.Range(0, chords[c.chordId].clips.Length);
            if (chordsPlayed == 0) whichClip = 0;

            s.clip = chords[c.chordId].clips[whichClip];
            lastChordBeatDuration = c.beats;

            // Todo playScheduled if we want better timming
            s.Play();
            chordsPlayed++;
        }
    }

    AudioSource GetNextAudioSource()
    {
        AudioSource s = musicSources[musicSourceIt];

        if (s.isPlaying)
            s.Stop();

        musicSourceIt = (musicSourceIt + 1) % musicSources.Length;

        return s;
    }

    ChordTiming GetNextChord()
    {
        ChordTiming c = chordProgression[chordIt];
        chordIt = (chordIt + 1) % chordProgression.Length;
        return c;
    }
}
