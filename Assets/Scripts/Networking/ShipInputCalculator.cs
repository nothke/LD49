using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShipInputCalculator : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateInput(ref float inputX, ref float inputY, ref float inputR)
    {
        if (photonView.IsMine)
        {
            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");
            inputR = Input.GetAxis("Roll");
        }
    }
}
