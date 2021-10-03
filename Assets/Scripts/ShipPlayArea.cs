using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPlayArea : MonoBehaviour
{
    public Vector2 minMaxX = new Vector2(-2,2);
    public Vector2 minMaxZ = new Vector2(-2, 2);

    public Transform areaCenter;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 TransformPoint(Vector2 local)
    {
        Vector3 l = new Vector3(local.x, 0, local.y);
        return areaCenter.TransformPoint(l);
    }

    public Vector2 InverseTransformPoint(Vector3 world)
    {
        Vector3 l = areaCenter.InverseTransformPoint(world);
        return new Vector2(l.x, l.z);
    }

    public void EnsureCircleInsideArea(ref Vector2 pos, float radius)
    {
        pos.x = Mathf.Max(minMaxX.x + radius, pos.x);
        pos.x = Mathf.Min(minMaxX.y - radius, pos.x);

        pos.y = Mathf.Max(minMaxZ.x + radius, pos.y);
        pos.y = Mathf.Min(minMaxZ.y - radius, pos.y);
    }

    private void OnDrawGizmos()
    {
        Transform t = areaCenter;
        if (t == null)
            t = transform;

        Gizmos.color = Color.black;

        Vector3 scale = new Vector3(minMaxX.y - minMaxX.x, 1.8f, minMaxZ.y - minMaxZ.x);
        Vector3 center = new Vector3((minMaxX.y + minMaxX.x) / 2f, 0.9f, (minMaxZ.y + minMaxZ.x) / 2f);
        Matrix4x4 m = Gizmos.matrix;

        Gizmos.matrix = t.localToWorldMatrix;

        Gizmos.DrawWireCube(center, scale);

        Gizmos.matrix = m;
    }
}
