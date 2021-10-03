using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGen : MonoBehaviour
{
    public GameObject prefab;
    public float tileWidth = 40;
    public int count = 10;

    void Start()
    {
        for (int x = -count; x < count; x++)
        {
            for (int z = -count; z < count; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                var go = Instantiate(prefab);
                go.transform.position = new Vector3(x * tileWidth, 0, z * tileWidth);
            }
        }
    }
}
