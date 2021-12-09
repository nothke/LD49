using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSounds : MonoBehaviour
{
    ShipController ship;

    [Header("Ship sound pool")]
    public AudioSource soundPrefab;
    public int poolSize = 64;
    AudioSource[] soundPool;
    int nextInPool = 0;
    public float minTimeBetweenCollisionSounds = 0.21f;

    [Header("Ship clash sounds")]
    public AudioClip[] shipCrashClips;


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

    public void PlaySoundAtPos(Vector3 p, AudioClip clip, float volume)
    {
        AudioSource pooledSource = soundPool[nextInPool];
        nextInPool = (nextInPool + 1) % soundPool.Length;

        pooledSource.clip = clip;
        pooledSource.transform.position = p;
        pooledSource.gameObject.SetActive(true);
        pooledSource.volume = volume;
        pooledSource.Play();
    }

    float lastShipCollisionTime = 0;
    public void ShipCollision(Vector3 p, float magnitude)
    {
        if (Time.time - lastShipCollisionTime > minTimeBetweenCollisionSounds)
        {
            lastShipCollisionTime = Time.time;

            //Debug.Log(magnitude);

            PlaySoundAtPos(p, NextShipCollisionClip(), Mathf.Clamp(magnitude / 7f, 0, 2f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.other.name);
        ShipCollision(collision.contacts[0].point, collision.relativeVelocity.magnitude);
    }
}
