using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public static Music instance;

    public bool muted = false;

    public AudioSource source;

    public float fadeOutTime = 13f;
    float currentFadingTime = 0;
    bool fadingOut = false;
    bool fadingIn = false;
    float maxVolume = 0;
    float startFadingVolume = 0;
    public UnityEngine.Audio.AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        mixer.GetFloat("MusicVolume", out maxVolume);
    }

    void FadeOut() {
        currentFadingTime = 0;
        fadingOut = true;
        fadingIn = false;
        mixer.GetFloat("MusicVolume", out startFadingVolume);
    }

    void FadeIn()
    {
        currentFadingTime = 0;
        fadingIn = true;
        fadingOut = false;
        mixer.GetFloat("MusicVolume", out startFadingVolume);
        if (!source.isPlaying) source.Play();
        muted = false;
    }

    ShipController ship;
    public void SetShip(ShipController s)
    {
        ship = s;
    }

    float speedAverage = 0f;
    float timeSlow = 0;
    float timeFast = 0;

    // Update is called once per frame
    void Update()
    {
        if (ship != null)
        {
            float speed = ship.SpeedKnots();
            float f = 0.01f;

            speedAverage = speedAverage * (1f - f) + speed * f;
        }

        if (speedAverage < 2f) timeSlow += Time.deltaTime;
        else timeSlow = 0;
        if (speedAverage > 2f) timeFast += Time.deltaTime;
        else timeFast = 0;

        if (!muted) { // music playing
            if (fadingIn)
            {
                currentFadingTime += Time.deltaTime;

                float fadingFactor = 1f - Mathf.Clamp01(currentFadingTime / fadeOutTime);

                if (fadingFactor <= 0f)
                {
                    fadingIn = false;
                    mixer.SetFloat("MusicVolume", maxVolume);
                }
                else
                    mixer.SetFloat("MusicVolume", Mathf.Lerp(maxVolume, startFadingVolume, Easing.Circular.In(fadingFactor)));
            }
            else if (fadingOut)
            {
                currentFadingTime += Time.deltaTime;

                float fadingFactor = Mathf.Clamp01(currentFadingTime / fadeOutTime);

                if (fadingFactor >= 1f)
                {
                    source.Stop();
                    muted = true;
                    fadingOut = false;
                    mixer.SetFloat("MusicVolume", -80f);
                }
                else
                    mixer.SetFloat("MusicVolume", Mathf.Lerp(startFadingVolume, -80f, Easing.Circular.In(fadingFactor)));
            }


            if (timeFast > 5f && !fadingOut)
            {
                FadeOut();
            }
        }

        if (timeSlow > 5f && !fadingIn)
        {
            FadeIn();
        }
    }
}
