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
        // calculate input locally based on input and other players
        if (photonView.IsMine)
        {
            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");
            inputR = Input.GetAxis("Roll");
        }
        else {
            inputX = Mathf.MoveTowards(inputX, 0, Time.deltaTime);
            inputY = Mathf.MoveTowards(inputY, 0, Time.deltaTime);
            inputR = Mathf.MoveTowards(inputR, 0, Time.deltaTime);
        }
    }
}
