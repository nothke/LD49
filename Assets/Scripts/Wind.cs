using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public static Wind e;
    void Awake() { e = this; }

    public float speed = 1;

    public Vector3 Velocity => transform.forward * speed;
}
