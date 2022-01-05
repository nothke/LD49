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

    public Transform cameraFocusBottom;
    public Transform cameraFocusTop;

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

    [System.NonSerialized]
    public float instantNormalizedRudderSpeed;
    [System.NonSerialized]
    public float instantNormalizedMastSpeed;

    [Header("Interactables")]
    public GameObject interactableSteeringWheel;

    float lastRudderAngle = 0;
    float lastMastAngl = 0;

    public void UpdateWithCurrentInput(float dTime)
    {
        mastAngle += -inputR * mastTurningSpeed * dTime;
        mastAngle = Mathf.Clamp(mastAngle, -90, 90);

        rudderAngle += -inputX * rudderTurningSpeed * dTime;
        rudderAngle = Mathf.Clamp(rudderAngle, -maxRudderSteeringAngle, maxRudderSteeringAngle);

        if (Mathf.Abs(rudderAngle) < 1f)
            rudderAngle = Mathf.MoveTowardsAngle(rudderAngle, 0f, dTime * rudderTurningSpeed * 0.1f);

        instantNormalizedRudderSpeed = (rudderAngle - lastRudderAngle) / dTime;
        instantNormalizedRudderSpeed /= rudderTurningSpeed;

        lastRudderAngle = rudderAngle;

        instantNormalizedMastSpeed = (mastAngle - lastMastAngl) / dTime;
        instantNormalizedMastSpeed /= mastTurningSpeed;

        lastMastAngl = mastAngle;
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
