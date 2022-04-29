using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPlayArea : ShipPlayArea
{

    public float maxHeight = 200f;
    public float minHeight = -5f;
    public LayerMask hitMask;

    Ray ray;
    RaycastHit[] hits;

    private void Start()
    {
        ray = new Ray(Vector3.zero, Vector3.down);
        hits = new RaycastHit[1];
    }

    public override void EnsureCircleInsideArea(ref Vector2 pos, float radius)
    {
        return;
    }

    public override Vector3 TransformDirection(Vector2 local)
    {
        return new Vector3(local.x, 0, local.y);
    }

    public override Vector2 InverseTransformDirection(Vector3 world)
    {
        return new Vector2(world.x, world.z);
    }

    public override Vector3 TransformPoint(Vector2 local)
    {
        Vector3 p = new Vector3(local.x, 0, local.y);
        p.y = Water.GetHeight(p);

        ray.origin = new Vector3(local.x, maxHeight, local.y);

        if (Physics.RaycastNonAlloc(ray,  hits, maxHeight - minHeight, hitMask.value) > 0)
        {
            p.y = Mathf.Max(p.y, hits[0].point.y);
        }

        return p;
    }

    public override Vector2 InverseTransformPoint(Vector3 world)
    {
        return new Vector2(world.x, world.z);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        float middle = (maxHeight + minHeight)/ 2f;
        Gizmos.DrawWireCube(Vector3.up * middle, new Vector3(10000, maxHeight - minHeight, 10000));
    }
}
