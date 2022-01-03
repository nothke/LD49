using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSounds : MonoBehaviour
{

    ShipController ship;
    Rigidbody rb;

    [Header("Ship sound pool")]
    public AudioSource soundPrefab;
    public int poolSize = 64;
    AudioSource[] soundPool;
    int nextInPool = 0;
    public float minTimeBetweenCollisionSounds = 0.21f;

    [Header("Waves against ship")]
    public AudioClip[] wavesAgainstShipClips;
    public UnityEngine.Audio.AudioMixerGroup wavesAgainstShipMixer;
    BuoyancyPoint[] buoyancyPoints;
    float[] lastPointAltitudes;
    public AudioClip[] wavesCrashAgainstShipClips;
    public UnityEngine.Audio.AudioMixerGroup wavesCrashAgainstShipMixer;
    public bool doWavesAgainstShip = true;

    [Header("Ship clash sounds")]
    public AudioClip[] shipCrashClips;
    public UnityEngine.Audio.AudioMixerGroup shipCrashMixer;
    float lastWaveClashTime = 0;
    float lastWaveCrashTime = -5f;


    [Header("Ship moving sounds")]
    public AudioSource stationary, frontMoving, backMoving;
    [Range(0, 1)]
    public float frontBackDirectionalFactor = 0.7f;

    public float maxExpectedKnots = 20f;
    public float maxMovingVolume = 2f;

    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponent<ShipController>();
        rb = GetComponent<Rigidbody>();

        frontMoving.volume = 0f;
        backMoving.volume = 0f;
        stationary.volume = 1f;

        // pool
        soundPool = new AudioSource[poolSize];
        Transform poolParent = new GameObject("Sound Pool").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < poolSize; ++i)
        {
            soundPool[i] = Instantiate(soundPrefab.gameObject, poolParent).GetComponent<AudioSource>();
            soundPool[i].gameObject.SetActive(false);
        }

        //
        buoyancyPoints = GetComponentsInChildren<BuoyancyPoint>();
        lastPointAltitudes = new float[buoyancyPoints.Length];

        for (int i = 0; i < buoyancyPoints.Length; ++i)
        {
            Vector3 p = buoyancyPoints[i].transform.position;
            lastPointAltitudes[i] = p.y - Water.GetHeight(p);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float shipSpeed = ship.SpeedKnots();

        float movingFactor = Mathf.Clamp01(shipSpeed / maxExpectedKnots);

        float frontDot = -Vector3.Dot(Camera.main.transform.forward, frontMoving.transform.forward);

        //Debug.Log(frontDot);

        stationary.volume = (1f - movingFactor);
        frontMoving.volume = movingFactor * (1f - frontBackDirectionalFactor + frontBackDirectionalFactor * Mathf.Max(0, frontDot));
        backMoving.volume = movingFactor * (1f - frontBackDirectionalFactor + frontBackDirectionalFactor * Mathf.Max(0, -frontDot));

        // buoyancy points wave clashes
        for (int i = 0; i < buoyancyPoints.Length; ++i)
        {
            Vector3 p = buoyancyPoints[i].transform.position;
            float newAltitude = p.y - Water.GetHeight(p);

            if (newAltitude < lastPointAltitudes[i] && newAltitude <= 0 && lastPointAltitudes[i] > 0)
            {
                float velocity = rb.GetPointVelocity(p).magnitude;
                if (velocity > 0.1f || lastPointAltitudes[i] - newAltitude > 0.1f)
                {
                    //Debug.Log("velocity? " + velocity);
                    if (velocity > 8f && Time.time - lastWaveCrashTime > 2.7f)
                    {
                        //Debug.Log("Wave Crash " + nextInPool + " " + (lastPointAltitudes[i] - newAltitude) + " " + velocity);
                        PlaySoundAtPos(p, NextWaveCrash(), Mathf.Clamp01((velocity-5f) / 10f), wavesCrashAgainstShipMixer, 128);
                        lastWaveCrashTime = Time.time + Random.Range(-1f, 1f);
                    }
                    else if (doWavesAgainstShip && Time.time - lastWaveClashTime > 0.71f)
                    {
                        //Debug.Log("Splash " + nextInPool + " " + (lastPointAltitudes[i] - newAltitude) + " " + velocity);
                        PlaySoundAtPos(p, NextWaveClash(), Mathf.Clamp(velocity / 12f, 0, 1.5f), wavesAgainstShipMixer);
                        lastWaveClashTime = Time.time;
                    }
                }
            }

            lastPointAltitudes[i] = newAltitude;
        }
    }


    int lastPlayedCollisionClip = -1;
    AudioClip NextShipCollisionClip()
    {
        AudioClip c = shipCrashClips[Random.Range(0, shipCrashClips.Length)];

        List<int> clips = new List<int>(shipCrashClips.Length);
        for (int i = 0; i < shipCrashClips.Length; ++i)
            if (i != lastPlayedCollisionClip) clips.Add(i);

        int chosen = clips[Random.Range(0, clips.Count)];
        lastPlayedCollisionClip = chosen;

        return shipCrashClips[chosen];
    }

    int lastWaveClashClip = -1;
    AudioClip NextWaveClash()
    {
        AudioClip c = wavesAgainstShipClips[Random.Range(0, wavesAgainstShipClips.Length)];

        List<int> clips = new List<int>(wavesAgainstShipClips.Length);
        for (int i = 0; i < wavesAgainstShipClips.Length; ++i)
            if (i != lastWaveClashClip) clips.Add(i);

        int chosen = clips[Random.Range(0, clips.Count)];
        lastWaveClashClip = chosen;

        return wavesAgainstShipClips[chosen];
    }

    int lastWaveCrashClip = -1;
    AudioClip NextWaveCrash()
    {
        AudioClip c = wavesCrashAgainstShipClips[Random.Range(0, wavesCrashAgainstShipClips.Length)];

        List<int> clips = new List<int>(wavesCrashAgainstShipClips.Length);
        for (int i = 0; i < wavesCrashAgainstShipClips.Length; ++i)
            if (i != lastWaveCrashClip) clips.Add(i);

        int chosen = clips[Random.Range(0, clips.Count)];
        lastWaveCrashClip = chosen;

        return wavesCrashAgainstShipClips[chosen];
    }

    public void PlaySoundAtPos(Vector3 p, AudioClip clip, float volume, UnityEngine.Audio.AudioMixerGroup mixer, int priority = 158, float minDistance = 1f)
    {
        AudioSource pooledSource = soundPool[nextInPool];
        nextInPool = (nextInPool + 1) % soundPool.Length;

        pooledSource.clip = clip;
        pooledSource.transform.position = p;
        pooledSource.gameObject.SetActive(true);
        pooledSource.volume = volume;
        pooledSource.outputAudioMixerGroup = mixer;
        pooledSource.priority = priority;
        pooledSource.minDistance = minDistance;
        pooledSource.Play();
    }

    float lastShipCollisionTime = 0;
    public void ShipCollision(Vector3 p, float magnitude)
    {
        if (!enabled)
            return;

        if (Time.time - lastShipCollisionTime > minTimeBetweenCollisionSounds)
        {
            lastShipCollisionTime = Time.time;

            //Debug.Log(magnitude);

            PlaySoundAtPos(p, NextShipCollisionClip(), Mathf.Clamp(magnitude / 7f, 0, 2f), shipCrashMixer, 128, 10f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.other.name);
        ShipCollision(collision.contacts[0].point, collision.relativeVelocity.magnitude);
    }

    private void OnDisable()
    {
        frontMoving.volume = 0f;
        backMoving.volume = 0f;
        stationary.volume = 0f;

        // pool

        for (int i = 0; i < soundPool.Length; ++i)
        {
            Destroy(soundPool[i].gameObject);
        }

        soundPool = null;
    }
}
