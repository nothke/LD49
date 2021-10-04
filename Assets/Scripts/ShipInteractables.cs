using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInteractables : MonoBehaviour
{
    public SteeringWheel leftWheel, rightWheel;
    public RopeHolder rope;

    public float interactionReach = 1f;

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
