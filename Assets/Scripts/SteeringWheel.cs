using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public ShipController controller;
    public float steeringLockAngle = 360;

    public float wheelRadius = 0.5f;

    float smoothAngle = 0;
    float smoothAngleVelo;

    void Update()
    {
        float targetAngle = -controller.inputX * steeringLockAngle;
        smoothAngle = Mathf.SmoothDamp(smoothAngle, targetAngle, ref smoothAngleVelo, 1);
        transform.localEulerAngles = new Vector3(0, 0, smoothAngle);
    }

    public Vector3 WheelPositionForAngle(float rad)
    {
        return transform.position + (transform.right * Mathf.Cos(rad) + transform.up * Mathf.Sin(rad)) * wheelRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        int circleCount = 12;
        float a = Mathf.PI * 2f / (float)circleCount;
        for (int i = 0; i < circleCount; ++i)
        {
            float angleBefore = i * a;
            float angleAfter = (i + 1)*a;

            Vector3 from = WheelPositionForAngle(angleBefore);
            Vector3 to = WheelPositionForAngle(angleAfter);
            Gizmos.DrawLine(from, to);
        }
    }
}
