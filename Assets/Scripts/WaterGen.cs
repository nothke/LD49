using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGen : MonoBehaviour
{
    public GameObject prefab;
    public float tileWidth = 40;
    public int count = 10;
    public Transform reflectionPivot;
    OrbitCam cam;

    [Range(0,1)]
    public float factor = 1;

    void Start()
    {
        for (int x = -count; x < count; x++)
        {
            for (int z = -count; z < count; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                var go = Instantiate(prefab, transform);
                go.transform.position = new Vector3(x * tileWidth, 0, z * tileWidth);
            }
        }

        cam = Camera.main.GetComponent<OrbitCam>();
    }

    private void Update()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        transform.position = new Vector3(cameraPos.x, transform.position.y, cameraPos.z);

        Vector3 reflectionPivotPos = transform.position;
        if (cam.target != null)
        {
            reflectionPivotPos = Vector3.Lerp(transform.position, cam.target.position, factor);
        }
        reflectionPivotPos.y = Water.GetHeight(reflectionPivotPos);

        reflectionPivot.transform.position = reflectionPivotPos;

    }
}
