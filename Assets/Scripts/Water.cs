using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class Water : MonoBehaviour
{
    //public static Water e;
    //void Awake() { e = this; }

    //public static float Height => 0;
    public static float Density => 100; // How much force to multiply to air

    public static bool IsUnderwater(Vector3 position)
    {
        return position.y < GetHeight(position);
    }

    public static float GetHeight(Vector3 pos)
    {
        float time = Time.time;

        return Sin(time + pos.x * 1.1f) * 0.4f;
    }
}
