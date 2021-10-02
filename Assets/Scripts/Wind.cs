using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public static Wind Instance;
    void Awake() { Instance = this; }

    public float speed = 1;

    public static Vector3 Velocity => Instance.transform.forward * Instance.speed;
}
