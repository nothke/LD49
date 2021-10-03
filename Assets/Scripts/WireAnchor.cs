using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireAnchor : MonoBehaviour
{
    public LineRenderer line;
    public int movingPointIndex;

    void Update()
    {
        Vector3 localPoint = line.transform.InverseTransformPoint(transform.position);
        line.SetPosition(movingPointIndex, localPoint);
    }
}
