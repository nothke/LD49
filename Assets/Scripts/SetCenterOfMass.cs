using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCenterOfMass : MonoBehaviour
{
    public Vector3 centerOfMass;

    private void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 p = transform.TransformPoint(centerOfMass);
        float size = UnityEditor.HandleUtility.GetHandleSize(p) * 0.3f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(p - transform.up * size * 0.5f, transform.up * size);
        Gizmos.DrawRay(p - transform.right * size * 0.5f, transform.right * size);
        Gizmos.DrawRay(p - transform.forward * size * 0.5f, transform.forward * size);
    }
#endif
}
