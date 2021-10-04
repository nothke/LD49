using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeHolder : MonoBehaviour
{
    public Transform left, center, right;

    public Vector3 RopeRelativePointToWorld(float rrp)
    {
        if (rrp < 0)
            return Vector3.Lerp(center.position, left.position, -rrp);
        else if (rrp > 0)
            return Vector3.Lerp(center.position, right.position, rrp);
        else return center.position;
    }

    // Very complicated function that calculates the distance to the rope and what rope "point" is the closest
    // the closestRopeRelativePoint is supposed to be fed back to this class to retrieve a world-position
    // a closestRopeRelativePoint of -1 is at the left anchor, 0 at the center, and 1 at the right
    public float DistanceToRope(Vector3 pos, out float closestRopeRelativePoint)
    {
        Vector3 leftRopeDelta = left.position - center.position;
        float leftRopeLength = leftRopeDelta.magnitude;

        Vector3 rightRopeDelta = right.position - center.position;
        float rightRopeLength = rightRopeDelta.magnitude;

        Vector3 projectedToLeft = Vector3.Project(pos - center.position, leftRopeDelta.normalized);
        Vector3 projectedToRight = Vector3.Project(pos - center.position, rightRopeDelta.normalized);

        float distanceToLeftRope = 0f;
        float dotLeft = Vector3.Dot(projectedToLeft, leftRopeDelta.normalized);
        if (dotLeft < 0) distanceToLeftRope = Vector3.Distance(pos, center.position);
        else if (dotLeft > leftRopeLength) distanceToLeftRope = Vector3.Distance(pos, left.position);
        else distanceToLeftRope = Vector3.Distance(pos, center.position + projectedToLeft);

        float distanceToRightRope = 0f;
        float dotRight = Vector3.Dot(projectedToRight, rightRopeDelta.normalized);
        if (dotRight < 0) distanceToRightRope = Vector3.Distance(pos, center.position);
        else if (dotRight > rightRopeLength) distanceToRightRope = Vector3.Distance(pos, right.position);
        else distanceToRightRope = Vector3.Distance(pos, center.position + projectedToRight);

        if (distanceToRightRope < distanceToLeftRope)
        {
            closestRopeRelativePoint = dotRight / rightRopeLength;
        }
        else
        {
            closestRopeRelativePoint = dotLeft / leftRopeLength;
        }


        return Mathf.Min(distanceToLeftRope, distanceToRightRope);
    }

    private void OnDrawGizmosSelected()
    {
        if (center != null && left != null && right != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(center.position, left.position);
            Gizmos.DrawLine(center.position, right.position);
        }
    }
}
