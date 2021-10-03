using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    Rigidbody _rb;
    public Rigidbody rb { get { if (!_rb) _rb = GetComponent<Rigidbody>(); return _rb; } }

    public float torqueMult = 1;
    public float forceMult = 1;

    public Transform[] rudders;
    float maxRudderSteeringAngle = 30;

    public Transform mast;
    public Transform playableArea;

    [System.NonSerialized]
    public float inputX;
    [System.NonSerialized]
    public float inputY;
    [System.NonSerialized]
    public float inputR;

    [System.NonSerialized]
    public float mastAngle;
    [System.NonSerialized]
    public float rudderAngle;

    void Update()
    {
        mastAngle += -inputR;
        mastAngle = Mathf.Clamp(mastAngle, -90, 90);

        // TODO maybe make the rudder angle be affected by the inputX rather than assigned to it
        rudderAngle = -inputX * maxRudderSteeringAngle + 90;

        foreach (var rudder in rudders)
        {
            rudder.localRotation = Quaternion.Euler(0, rudderAngle, 0);
        }

        mast.localRotation = Quaternion.Euler(0, mastAngle, 0);
    }

    private void FixedUpdate()
    {
        //rb.AddRelativeTorque(Vector3.up * inputX * torqueMult, ForceMode.Acceleration);
        rb.AddRelativeForce(Vector3.forward * inputY * forceMult, ForceMode.Acceleration);
    }
}
