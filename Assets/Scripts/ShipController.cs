using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    Rigidbody _rb;
    Rigidbody rb { get { if (!_rb) _rb = GetComponent<Rigidbody>(); return _rb; } }

    public float torqueMult = 1;
    public float forceMult = 1;

    public Transform[] rudders;
    float maxRudderSteeringAngle = 30;

    public Transform mast;

    float inputX;
    float inputY;

    float mastAngle;

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        float inputR = Input.GetAxis("Roll");

        mastAngle += -inputR;
        mastAngle = Mathf.Clamp(mastAngle, -90, 90);

        foreach (var rudder in rudders)
        {
            rudder.localRotation = Quaternion.Euler(0, -inputX * maxRudderSteeringAngle + 90, 0);
        }

        mast.localRotation = Quaternion.Euler(0, mastAngle, 0);
    }

    private void FixedUpdate()
    {
        //rb.AddRelativeTorque(Vector3.up * inputX * torqueMult, ForceMode.Acceleration);
        rb.AddRelativeForce(Vector3.forward * inputY * forceMult, ForceMode.Acceleration);
    }
}
