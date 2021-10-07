using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelInteractable : Interactable
{
    public SteeringWheel wheel;

    public override Vector3 GetClosestPoint(Vector3 fromPosition, out float distance)
    {
        Vector3 targetPosition = transform.position;
        distance = Vector3.Distance(fromPosition, targetPosition);
        return targetPosition;
    }

    public override float GetHandStartFactor()
    {
        return Random.Range(Mathf.PI * 0.1f, Mathf.PI * 0.9f);
    }

    public override void GetHandStartFactors(out float leftHand, out float rightHand, float handStartFactor)
    {
        // These are the positions of the hands in radians in the wheel
        leftHand = handStartFactor + Mathf.PI * 0.5f;
        rightHand = -handStartFactor + Mathf.PI * 0.5f;
        ShipUI.instance.EnableWheelSlider(true);
    }

    public override void GetHandPositions(out Vector3 leftHand, out Vector3 rightHand, float leftHandFactor, float rightHandFactor)
    {
        leftHand = wheel.WheelPositionForAngle(leftHandFactor);
        rightHand = wheel.WheelPositionForAngle(rightHandFactor);
    }

    public override Vector3 GetTargetBodyPosition(float leftHandStartFactor, float rightHandStartFactor)
    {
        return wheel.transform.position - wheel.transform.forward * 0.5f;

    }
}
