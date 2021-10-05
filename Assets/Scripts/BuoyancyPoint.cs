using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyPoint : MonoBehaviour
{
    Rigidbody rb;

    public float forceMult = 1;
    public float dampMult = 0.1f;
    public float dragMult = 0.001f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    static readonly Vector3 up = Vector3.up;

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        float waterHeight = Water.GetHeight(pos);
        float diff = waterHeight - pos.y;

        float spring = Mathf.Clamp(diff, 0, Mathf.Infinity) * forceMult;

        if (diff > 0)
        {
            Vector3 velo = rb.GetPointVelocity(pos);
            float vertVelo = velo.y;

            float damp = -vertVelo * dampMult;

            Vector3 drag;
            if (dragMult > 0)
            {
                drag = -velo.normalized * velo.sqrMagnitude * dragMult;
                drag.y = 0;
            }
            else drag = default;

            rb.AddForceAtPosition(up * (spring + damp) + drag, transform.position, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, Vector3.up * 0.2f);
        float scale = 0.2f;
        Gizmos.DrawRay(transform.position - Vector3.right * scale * 0.5f, Vector3.right * scale);
        Gizmos.DrawRay(transform.position - Vector3.forward * scale * 0.5f, Vector3.forward * scale);
        //Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
    }
}
