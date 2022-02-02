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
    float startVolume = 0;
    float startFadingVolume = 0;
    public UnityEngine.Audio.AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        mixer.GetFloat("MusicVolume", out startVolume);
    }

    void FadeOut() {
        currentFadingTime = 0;
        fadingOut = true;
        mixer.GetFloat("MusicVolume", out startFadingVolume);
    }

    void FadeIn()
    {
        currentFadingTime = 0;
        fadingIn = true;
        mixer.GetFloat("MusicVolume", out startFadingVolume);
        source.Play();
        muted = false;
    }

    ShipController ship;
    public void SetShip(ShipController s)
    {
        ship = s;
    }

    float speedAverage = 0f;

    // Update is called once per frame
    void Update()
    {
        if (ship != null)
        {
            float speed = ship.SpeedKnots();
            float f = 0.1f;

            speedAverage = speedAverage * (1f - f) + speed * f;
            //Debug.Log(speedAverage);
        }

        if (muted)
        {
            if (speedAverage < 2f && !fadingIn)
            {
                FadeIn();
            }
        }
        else {
            if (fadingIn)
            {
                currentFadingTime += Time.deltaTime;

                float fadingFactor = 1f - Mathf.Clamp01(currentFadingTime / fadeOutTime);

                if (fadingFactor <= 0f)
                {
                    fadingIn = false;
                }
                else
                    mixer.SetFloat("MusicVolume", Mathf.Lerp(startVolume, startFadingVolume, Easing.Circular.In(fadingFactor)));
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
                }
                else
                    mixer.SetFloat("MusicVolume", Mathf.Lerp(startFadingVolume, -80f, Easing.Circular.In(fadingFactor)));
            }


            if (speedAverage > 2f && !fadingOut)
            {
                FadeOut();
            }
        }
    }
}
