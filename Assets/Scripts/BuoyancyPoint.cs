using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyPoint : MonoBehaviour
{
    Rigidbody rb;

    public float forceMult = 1;
    public float dampMult = 0.1f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        float waterHeight = Water.GetHeight(pos);

        float spring = Mathf.Clamp(waterHeight - pos.y, 0, Mathf.Infinity) * forceMult;

        if (spring > 0)
        {
            Vector3 velo = rb.GetPointVelocity(pos);
            float vertVelo = velo.y;

            float damp = -vertVelo * dampMult;

            rb.AddForceAtPosition(Vector3.up * (spring + damp), transform.position, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
    }
}
