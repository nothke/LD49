using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInteractable : Interactable
{
    public override Type GetType() { return Type.World; }

    public ShipPlayArea shipPlayArea;

    public override Vector3 GetClosestPoint(Vector3 fromPosition, out float distance)
    {
        Vector3 localPos = shipPlayArea.areaCenter.InverseTransformPoint(fromPosition);

        distance = Mathf.Min(Mathf.Abs(localPos.x - shipPlayArea.minMaxX.x), Mathf.Abs(localPos.x - shipPlayArea.minMaxX.y));
        distance = Mathf.Min(distance, Mathf.Abs(localPos.z - shipPlayArea.minMaxZ.x));
        //distance = Mathf.Min(distance, Mathf.Abs(shipPlayArea.minMaxZ.y - localPos.z));

        distance += Mathf.Abs(localPos.y - 1f);

        return Vector3.zero; // Actually not used!
    }

    public override float GetHandStartFactor()
    {
        throw new System.NotImplementedException();
    }

    public override void GetHandStartFactors(out float leftHand, out float rightHand, float handStartFactor)
    {
        throw new System.NotImplementedException();
    }

    public override Vector3 GetTargetBodyPosition(float leftHandStartFactor, float rightHandStartFactor)
    {
        throw new System.NotImplementedException();
    }

    public override void GetHandPositions(out Vector3 leftHand, out Vector3 rightHand, float leftHandFactor, float rightHandFactor)
    {
        throw new System.NotImplementedException();
    }

    public override void OnHighlighted()
    {
        ShipUI.instance.SetInteractionText("Press ACTION to jump off the ship");
    }
}
