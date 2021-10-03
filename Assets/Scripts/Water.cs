using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using Photon.Pun;

public class Water : MonoBehaviour
{
    public static float Density => 100; // How much force to multiply to air


    const string NETWORK_TIME_SHADER_PARAMETER = "_NetworkTime";
    int NETWORK_TIME_ID;

    const double PI2 = System.Math.PI * 2d;

    private void Start()
    {
        NETWORK_TIME_ID = Shader.PropertyToID(NETWORK_TIME_SHADER_PARAMETER);
    }

    public static bool IsUnderwater(Vector3 position)
    {
        return position.y < GetHeight(position);
    }

    public static float GetHeight(Vector3 pos)
    {
        float time = (float)(PhotonNetwork.Time - ConnectionManager.roomCreatedTime);

        return Sin(time + pos.x * 1.1f) * 0.2f;
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            Shader.SetGlobalFloat(NETWORK_TIME_ID, (float)(PhotonNetwork.Time - ConnectionManager.roomCreatedTime));
        }
        else
        {
            Shader.SetGlobalFloat(NETWORK_TIME_ID, Time.time);
        }
    }
}
