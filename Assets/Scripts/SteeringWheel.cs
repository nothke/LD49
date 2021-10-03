using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public ShipController controller;
    public float steeringLockAngle = 360;

    float smoothAngle = 0;
    float smoothAngleVelo;

    void Update()
    {
        float targetAngle = -controller.inputX * steeringLockAngle;
        smoothAngle = Mathf.SmoothDamp(smoothAngle, targetAngle, ref smoothAngleVelo, 1);
        transform.localEulerAngles = new Vector3(0, 0, smoothAngle);
    }
}
