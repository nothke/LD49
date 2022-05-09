using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeMovement : MonoBehaviour
{
    WorldPlayArea world;

    // Start is called before the first frame update
    void Start()
    {
        world = RoomController.i.world;
    }

    // Update is called once per frame
    public void UpdatePosition(ref Vector2 pos, bool local, float deltaTime)
    {
        
    }

    public void OnPhotonSerializeView(Photon.Pun.PhotonStream stream)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(enabled);
            if (enabled)
            {
                // TODO send info
            }
        }
        else {
            bool movementEnabled = (bool)stream.ReceiveNext();

            if (movementEnabled)
            {
                if (!enabled && movementEnabled)
                {
                    // Just enabled
                }
                else if (enabled && !movementEnabled)
                {
                    // Just disabled
                }

                // TODO receive info
            }
        }
    }
}
