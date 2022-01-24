using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTeller : MonoBehaviour
{
    public Rigidbody rb;

    LineRenderer _line;
    public LineRenderer line { get { if (!_line) _line = GetComponent<LineRenderer>(); return _line; } }

    public float length = 0.2f;
    public float turbulence = 1;

    [HideInInspector]
    public bool correctPosition = false;

    private void Update()
    {
        Vector3 velocity = rb ? rb.GetPointVelocity(transform.position) : Vector3.zero;

        Vector3 endPos = Wind.Velocity - velocity;
        endPos = endPos.normalized * length;

        endPos = transform.InverseTransformDirection(endPos);

        const int COUNT = 4;
        if (correctPosition)
            line.SetPosition(0, -endPos * 0.4f);

        for (int i = 1; i < 4; i++)
        {
            Vector3 off = Random.insideUnitSphere;
            float factor = (float)i / COUNT;
            Vector3 pos = endPos * factor + off * factor * turbulence * 0.1f;
            if (correctPosition) pos -= endPos * 0.4f;

            line.SetPosition(i, pos);
        }
    }
}
