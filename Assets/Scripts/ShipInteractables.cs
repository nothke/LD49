using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInteractables : MonoBehaviour
{
    public SteeringWheel leftWheel, rightWheel;
    public RopeHolder rope;

    public float interactionReach = 1f;

    public enum InteractingThing
    {
        Nothing,
        Rope,
        LeftWheel,
        RightWheel
    }

    public bool InInteractableReach(Vector3 pos, out InteractingThing thing, out float interactablePosition)
    {
        float distanceToLeftWheel = Vector3.Distance(leftWheel.transform.position, pos);
        float distanceToRightWheel = Vector3.Distance(rightWheel.transform.position, pos);
        float distanceToRope = rope.DistanceToRope(pos, out float closestRopeRelPoint);

        if (distanceToLeftWheel < distanceToRightWheel)
        {
            if (distanceToLeftWheel < distanceToRope)
            {
                thing = InteractingThing.LeftWheel;
                interactablePosition = Random.Range(Mathf.PI * 0.1f, Mathf.PI * 0.9f);

                return distanceToLeftWheel <= interactionReach;
            }
        }

        if (distanceToRightWheel < distanceToRope)
        {
            thing = InteractingThing.RightWheel;
            interactablePosition = Random.Range(Mathf.PI * 0.1f, Mathf.PI * 0.9f);

            return distanceToRightWheel <= interactionReach;
        }
        else {
            thing = InteractingThing.Rope;
            interactablePosition = closestRopeRelPoint;

            return distanceToRope <= interactionReach;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (leftWheel != null)
            Gizmos.DrawWireSphere(leftWheel.transform.position, interactionReach);

        if (rightWheel != null)
            Gizmos.DrawWireSphere(rightWheel.transform.position, interactionReach);

        if (rope != null)
        {
            Gizmos.DrawWireSphere(rope.center.position, interactionReach);
            Gizmos.DrawWireSphere(rope.left.position, interactionReach);
            Gizmos.DrawWireSphere(rope.right.position, interactionReach);
        }
    }
}