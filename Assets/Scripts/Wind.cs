using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public static Wind Instance;
    void Awake() { Instance = this; }

    public float speed = 1;

    public static Vector3 Velocity => Instance.transform.forward * Instance.speed;

    int bumpDirectionId;

    public Material waterShader;

    private void Start()
    {
        //bumpDirectionId = Shader.PropertyToID("_BumpDirectionAndSpeed");
    }

    private void Update()
    {
        Vector3 velo = Velocity;

        // Pack
        Vector4 v = new Vector4(velo.x, velo.z, velo.x, velo.z) * 1000.0f;

        v = new Vector3(100, 0, 0);

        //Debug.Log("Running water");
        Shader.SetGlobalVector("_BumpDirectionAndSpeed", v);
    }
}
