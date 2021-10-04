using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterParticlePoint : MonoBehaviour
{
    public ParticleSystem particles;

    Rigidbody rb;

    public bool influencesStartSpeed;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        Vector3 pos = transform.position;

        bool emit = false;
        if (Water.IsUnderwater(pos))
        {
            float h = Water.GetHeight(pos);
            pos.y = h;
            particles.transform.position = pos;
            emit = true;
        }

        var em = particles.emission;
        em.enabled = emit;

        if (influencesStartSpeed)
        {
            var main = particles.main;

            Vector3 velo = rb.velocity;
            float horizontalSpeed = new Vector2(velo.x, velo.z).magnitude;
            main.startSpeed = horizontalSpeed;
        }
    }
}
