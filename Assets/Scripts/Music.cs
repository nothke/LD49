using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public static Music instance;

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
    public AudioSource noiseSource;
    int musicSourceIt = 0;

    public float fadeOutTime = 13f;
    float currentFadingTime = 0;
    bool fadingOut = false;
    float startVolume = 0;
    public UnityEngine.Audio.AudioMixer mixer;


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
        instance = this;
        mixer.GetFloat("MusicVolume", out startVolume);
    }

    public void FadeOut() {
        currentFadingTime = 0;
        fadingOut = true;
    }

    int chordsPlayed = 0;
    // Update is called once per frame
    void Update()
    {
        if (false && Input.GetKeyDown(KeyCode.M))
        {
            muted = !muted;
            if (muted)
            {
                foreach (AudioSource s in musicSources)
                    if (s.isPlaying) s.Stop();
                noiseSource.Stop();
            }
            else {
                noiseSource.Play();
            }
        }

        timeSinceLastBeat += Time.deltaTime;

        if (muted)
        {
            return;
        }

        if (fadingOut) {
            currentFadingTime += Time.deltaTime;

            float fadingFactor = Mathf.Clamp01(currentFadingTime / fadeOutTime);

            if (fadingFactor >= 1f)
            {
                foreach (AudioSource s in musicSources)
                    if (s.isPlaying) s.Stop();
                noiseSource.Stop();
                muted = true;
                fadingOut = false;
            }
            else
                mixer.SetFloat("MusicVolume", Mathf.Lerp(startVolume, -80f, Easing.Circular.In(fadingFactor)));
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
