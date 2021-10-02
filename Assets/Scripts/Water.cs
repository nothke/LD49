using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    //public static Water e;
    //void Awake() { e = this; }

    public static float Height => 0;

    public static bool IsUnderwater(Vector3 position)
    {
        return position.y < Height;
    }

    public static float GetHeight(Vector3 position)
    {
        return Height;
    }
}
