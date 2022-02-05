using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticWavePositioning : MonoBehaviour
{
    Transform cam;
    private void Start()
    {
        cam = Camera.main.transform;
    }
    void LateUpdate()
    {
        Vector3 p = cam.position;

        p.y = Water.GetHeight(p);
        transform.position = p;
    }
}
