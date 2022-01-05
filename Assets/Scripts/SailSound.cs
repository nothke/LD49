using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailSound : MonoBehaviour
{
    public AudioSource source;
    public Sail sail;
    public float minPitch = 0.5f;

    ShipSounds s;
    // Start is called before the first frame update
    void Start()
    {
        s = GetComponentInParent<ShipSounds>();

        if (s == null || !s.enabled)
        {
            enabled = false;
            source.enabled = false;
            return;
        }

        source.Play();
    }

    float filteredForce = 0f;

    // Update is called once per frame
    void Update()
    {
        if (s == null || !s.enabled)
        {
            enabled = false;
            source.Stop();
            source.enabled = false;
            return;
        }

        float filter = 0.69f;
        filteredForce = filter * filteredForce + sail.force.magnitude * (1f - filter);

        //Debug.Log(filteredForce + " " + sail.force.magnitude);

        float flailIntensity = Mathf.Clamp01(1.2f - filteredForce / 10f);

        float easedFlail = Easing.Quadratic.Out(flailIntensity);
        source.volume = easedFlail;
        source.pitch = Mathf.Lerp(minPitch, 1f, flailIntensity);
    }
}
