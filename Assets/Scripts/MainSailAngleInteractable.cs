using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSailAngleInteractable : Interactable
{
    RopeHolder _rope;
    RopeHolder rope { get { if (!_rope) _rope = GetComponent<RopeHolder>(); return _rope; } }

    // transient state, used right after the closest point is requested
    float closestRopeRelFactor;

    public override Vector3 GetClosestPoint(Vector3 fromPosition, out float distance)
    {
        distance = rope.DistanceToRope(fromPosition, out closestRopeRelFactor);
        return Vector3.zero; // Actually not used!
    }

    public override float GetHandStartFactor()
    {
        return closestRopeRelFactor;
    }

    public override void GetHandStartFactors(out float leftHandFactor, out float rightHandFactor, float handStartFactor)
    {
        leftHandFactor = Mathf.Clamp(handStartFactor - 0.05f, -1f, 1f);
        rightHandFactor = Mathf.Clamp(handStartFactor + 0.05f, -1f, 1f);
    }

    public override void GetHandPositions(out Vector3 leftHand, out Vector3 rightHand, float leftHandStartFactor, float rightHandStartFactor)
    {
        leftHand = rope.RopeRelativePointToWorld(leftHandStartFactor);
        rightHand = rope.RopeRelativePointToWorld(rightHandStartFactor);
    }

    public override Vector3 GetTargetBodyPosition(float leftHandStartFactor, float rightHandStartFactor)
    {
        Vector3 bodyPos = rope.RopeRelativePointToWorld(Mathf.Lerp(leftHandStartFactor, leftHandStartFactor, 0.5f));
        bodyPos -= transform.forward * 0.5f; // interactables.leftWheel ??
        return bodyPos;
    }

    public override void OnHighlighted()
    {
        ShipUI.instance.SetInteractionText("Hold ACTION and < > to turn the sail");
    }

}
