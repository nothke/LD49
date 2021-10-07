#define NEW_INTERACTION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShipInputCalculator : MonoBehaviourPun
{
    public float singlePlayerStrength = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateInput(int shipIt, ref float inputX, ref float inputY, ref float inputR)
    {

        inputY = 0f;

        float instantInputR = 0;
        float instantInputX = 0;

        foreach (Photon.Realtime.Player p in RoomController.i.shipIdToPlayers[shipIt])
        {
            PlayerSync ps = RoomController.i.playerSyncs[p];

            if (ps.IsInteracting())
            {
                float interactionAxis = ps.InteractingInput();

#if NEW_INTERACTION
                if (ps.CurrentlyInteractingWith is WheelInteractable)
                    instantInputX += interactionAxis * singlePlayerStrength;
                else
                    instantInputR += interactionAxis * singlePlayerStrength;

#else
                switch (ps.WhichInteractable()) {
                    case ShipInteractables.InteractingThing.Rope:
                        instantInputR += interactionAxis * singlePlayerStrength;
                        break;
                    case ShipInteractables.InteractingThing.LeftWheel:
                        instantInputX += interactionAxis * singlePlayerStrength;
                        break;
                    case ShipInteractables.InteractingThing.RightWheel:
                        instantInputX += interactionAxis * singlePlayerStrength;
                        break;
                    case ShipInteractables.InteractingThing.Nothing:
                        break;
                }
#endif
            }
        }
        //inputX = Mathf.MoveTowards(inputX, instantInputR, Time.deltaTime);
        //inputR = Mathf.MoveTowards(inputR, instantInputX, Time.deltaTime);
        inputX = instantInputX;
        inputR = instantInputR;
    }
}
