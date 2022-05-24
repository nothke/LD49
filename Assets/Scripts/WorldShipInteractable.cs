using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldShipInteractable : Interactable
{
    public override Type GetType() { return Type.Ship; }

    public ShipPlayArea shipPlayArea;

    ShipSync sync;
    ShipInteractables worldInteractables;

    private void Awake()
    {
        sync = GetComponent<ShipSync>();

        worldInteractables = RoomController.i.worldInteractables;
        worldInteractables.interactables.Add(this);
    }

    public override Vector3 GetClosestPoint(Vector3 fromPosition, out float distance)
    {
        Vector3 localPos = shipPlayArea.areaCenter.InverseTransformPoint(fromPosition);
        localPos.y = 0;
        shipPlayArea.EnsureCircleInsideArea(ref localPos, 0);

        distance = Vector3.Distance(fromPosition, shipPlayArea.areaCenter.TransformPoint(localPos));
        //Debug.Log(distance);

        return Vector3.zero; // unused
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

    private void OnDestroy()
    {
        worldInteractables.interactables.Remove(this);
    }

    public override void OnHighlighted()
    {
        ShipUI.instance.SetInteractionText($"Press ACTION to board ship #{sync.shipId+1}");
    }
}
