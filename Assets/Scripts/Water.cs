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

    static float sin(float t) {return Sin(t);}
    static float cos(float t) {return Cos(t);}

    public static float GetHeight(Vector3 pos)
    {
        float time = (float)(PhotonNetwork.Time - ConnectionManager.roomCreatedTime);

        if (!PhotonNetwork.IsConnected)
            time = Time.time;

        
        float f0 = sin(time * 0.7f + pos.x * 0.2f);
        float f1 = sin(time + pos.z * -0.7f + cos(time * 0.7f + pos.x * 0.1f - pos.z * 0.07f) * 2f + f0);
        float f2 = sin(time * 0.3f + pos.z * -0.07f);
        float f3 = sin(time * 0.4f + pos.x * 0.08f);
	
	    return f1 * 0.23f + (f2 * f3) * 0.7f;

        //return Sin(time + pos.z * -0.7f + Sin(time * 0.5f + pos.x * 0.2f)) * 0.23f + (Sin(time * 0.3f + pos.z * -0.07f) * Sin(time * 0.4f + pos.x * 0.08f)) * 0.7f;
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            double networkTime = PhotonNetwork.Time - ConnectionManager.roomCreatedTime;
            Shader.SetGlobalFloat(NETWORK_TIME_ID, (float)networkTime);
            //Debug.Log(PhotonNetwork.Time + " - "+ ConnectionManager.roomCreatedTime + " = "+(float)(PhotonNetwork.Time - ConnectionManager.roomCreatedTime));
        }
        else
        {
            Shader.SetGlobalFloat(NETWORK_TIME_ID, Time.time);
        }
    }
}
