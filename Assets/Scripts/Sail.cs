using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sail : MonoBehaviour
{
    public float width = 1, height = 1;
    public float forceMultiplier = 1;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 sailNormal = transform.forward;
        Vector3 windVelocity = -rb.velocity + Wind.e.Velocity;

        float area = width * height;

        Vector3 force = Vector3.Project(windVelocity, sailNormal) * area * forceMultiplier;
        rb.AddForceAtPosition(force, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward);

        Vector3 p0 = transform.TransformPoint(new Vector2(-width, -height));
        Vector3 p1 = transform.TransformPoint(new Vector2(width, -height));
        Vector3 p2 = transform.TransformPoint(new Vector2(-width, height));
        Vector3 p3 = transform.TransformPoint(new Vector2(width, height));

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p2, p3);

        Gizmos.DrawLine(p0, p2);
        Gizmos.DrawLine(p1, p3);
    }
}
