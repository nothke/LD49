using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTeller : MonoBehaviour
{
    public Rigidbody rb;

    LineRenderer _line;
    LineRenderer line { get { if (!_line) _line = GetComponent<LineRenderer>(); return _line; } }

    public float length = 0.2f;
    public float turbulence = 1;

    private void Update()
    {
        Vector3 velocity = rb ? rb.GetPointVelocity(transform.position) : Vector3.zero;

        Vector3 endPos = Wind.Velocity - velocity;
        endPos = endPos.normalized * length;

        endPos = transform.InverseTransformDirection(endPos);

        const int COUNT = 4;
        for (int i = 1; i < 4; i++)
        {
            Vector3 off = Random.insideUnitSphere;
            float factor = (float)i / COUNT;
            line.SetPosition(i, endPos * factor + off * factor * turbulence * 0.1f);
        }
    }
}
