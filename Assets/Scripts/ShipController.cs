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

    // angles per second
    public float rudderTurningSpeed = 20f;
    public float mastTurningSpeed = 25f;

    public Transform mast;
    public Transform playableArea;

    public Transform cameraFocus;

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

    public float RudderAngleNormalized => rudderAngle / maxRudderSteeringAngle;

    [Header("Interactables")]
    public GameObject interactableSteeringWheel;

    public void UpdateWithCurrentInput(float dTime)
    {
        mastAngle += -inputR * mastTurningSpeed * dTime;
        mastAngle = Mathf.Clamp(mastAngle, -90, 90);

        rudderAngle += -inputX * rudderTurningSpeed * dTime;
        rudderAngle = Mathf.Clamp(rudderAngle, -maxRudderSteeringAngle, maxRudderSteeringAngle);

        //Debug.Log("Input is " + inputX + " " + inputR);
    }

    void Update()
    {
        foreach (var rudder in rudders)
        {
            rudder.localRotation = Quaternion.Euler(0, rudderAngle, 0);
        }

        mast.localRotation = Quaternion.Euler(0, mastAngle, 0);
    }

    public float SpeedKnots()
    {
        const float MS2KNOTS = 1.94384f;

        Vector3 horizontalVelo = rb.velocity;
        horizontalVelo.y = 0;

        return horizontalVelo.magnitude * MS2KNOTS;
    }

    private void FixedUpdate()
    {
        //rb.AddRelativeTorque(Vector3.up * inputX * torqueMult, ForceMode.Acceleration);
        rb.AddRelativeForce(Vector3.forward * inputY * forceMult, ForceMode.Acceleration);
    }
}
