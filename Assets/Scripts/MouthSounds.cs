using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthSounds : MonoBehaviour
{
    Transform mouth;
    AudioSource source;

    public AudioClip[] clips;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    public readonly static Vector2 minMaxLowestPitch = new Vector2(0.8f, 1.05f);
    public readonly static Vector2 minMaxVocalRange = new Vector2(0.1f, 0.25f);


    // Start is called before the first frame update
    void Start()
    {
        mouth = transform;
        source = mouth.GetComponent<AudioSource>();
    }

    bool mouthOpen = false;
    // Update is called once per frame
    void Update()
    {
        if (mouthOpen && !source.isPlaying)
            mouth.transform.localScale = Vector3.zero;
    }

    public static Vector2 GetRandomPitchRange() {
        float min = Random.Range(minMaxLowestPitch.x, minMaxLowestPitch.y);
        float range = Random.Range(minMaxVocalRange.x, minMaxVocalRange.y);

        return new Vector2(min, min + range);
    }

    public void MiauNetwork(Photon.Pun.PhotonView catView)
    {
        catView.RPC("Miau", Photon.Pun.RpcTarget.All, Random.Range(0, clips.Length), Random.Range(pitchRange.x, pitchRange.y));
    }

    public void PlayMiau(int which, float pitch)
    {
        if (source.isPlaying)
            source.Stop();

        source.clip = clips[which % clips.Length];
        source.pitch = pitch;
        source.Play();

        mouth.transform.localScale = Vector3.one;
        mouthOpen = true;
    }
}
