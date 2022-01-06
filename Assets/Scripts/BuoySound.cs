using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BuoySound : MonoBehaviourPun
{
    public AudioSource source;
    public AudioClip[] clips;

    public Vector2 minMaxTimeBetweenBells = new Vector2(2f, 8f);
    float timeUntilBellRing = 0;

    // Start is called before the first frame update
    void Start()
    {
        timeUntilBellRing = Random.Range(minMaxTimeBetweenBells.x, minMaxTimeBetweenBells.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && !source.isPlaying)
        {
            timeUntilBellRing -= Time.deltaTime;

            if (timeUntilBellRing <= 0)
            {
                playingCollision = false;
                photonView.RPC("PlaySound", RpcTarget.All, Random.Range(0, clips.Length), false);
                timeUntilBellRing = Random.Range(minMaxTimeBetweenBells.x, minMaxTimeBetweenBells.y);
            }
        }

    }

    bool playingCollision = false;
    public void OnCollisionEnter(Collision collision)
    {
        if (!playingCollision || !source.isPlaying)
        {
            photonView.RPC("PlaySound", RpcTarget.All, Random.Range(0, clips.Length), true);
        }
    }

    [PunRPC]
    void PlaySound(int which, bool collision)
    {
        if (source.isPlaying && (playingCollision || !collision)) return;

        if (source.isPlaying) source.Stop();

        source.clip = clips[which];

        source.Play();
        playingCollision = collision;

    }
}
