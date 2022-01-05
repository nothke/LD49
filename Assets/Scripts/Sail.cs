using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sail : MonoBehaviour
{
    public float width = 1, height = 1;
    public float forceMultiplier = 1;

    Rigidbody rb;

    public Vector3 Normal => transform.forward;

    [System.NonSerialized]
    public Vector3 force;
    Vector3 windVelocity;

    public float bendForceForward = 0;

    public Cloth cloth;

    public float maxForce;

    public bool airOnly;

    static readonly Vector3 zero;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 sailPos = transform.position;

        bool underwater = Water.IsUnderwater(sailPos);

        Vector3 sailVelocity = rb.GetPointVelocity(sailPos);

        windVelocity =
            underwater ?
                (!airOnly ? -sailVelocity * Water.Density : zero) :
                -sailVelocity + Wind.Velocity; // * 0.1f

        float area = width * height;

        force = Vector3.Project(windVelocity, Normal) * area * forceMultiplier;

        if (bendForceForward != 0)
        {
            //float bendAngle = Vector3.SignedAngle(force, transform.forward, transform.up) > 0 ? -bendForceForward : bendForceForward;
            float bendAngle = bendForceForward;
            force = Vector3.RotateTowards(force, rb.transform.forward, bendAngle, 0.0f);
        }

        if (maxForce > 0)
        {
            force = Vector3.ClampMagnitude(force, maxForce);
        }

        rb.AddForceAtPosition(force, sailPos);
    }

    private void Update()
    {
        if (cloth)
        {
            cloth.externalAcceleration = force * 10;
        }
    }

    private void OnDrawGizmos()
    {
        bool underwater = Water.IsUnderwater(transform.position);

        Gizmos.color = underwater ? Color.cyan : Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward);

        Vector3 p0 = transform.TransformPoint(new Vector2(-width, -height));
        Vector3 p1 = transform.TransformPoint(new Vector2(width, -height));
        Vector3 p2 = transform.TransformPoint(new Vector2(-width, height));
        Vector3 p3 = transform.TransformPoint(new Vector2(width, height));

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p2, p3);

        Gizmos.DrawLine(p0, p2);
        Gizmos.DrawLine(p1, p3);

        if (Application.isPlaying)
        {
            if (!underwater)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, Wind.Velocity);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, force);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, windVelocity);
        }
    }
}
