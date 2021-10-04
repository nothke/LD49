using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    public const string PLAYER_SHIP = "PLAYER_SHIP";
    public int shipId = -1;

    ShipPlayArea playArea;
    ShipInteractables interactables;

    Vector2 pos = Vector2.zero;
    public float collisionRadius = 0.5f;
    public float speed = 10f;
    public float pushSpeed = 5f;

    bool interacting = false;
    ShipInteractables.InteractingThing interactingThing = ShipInteractables.InteractingThing.Rope;
    float interactingInput = 0;
    float leftHandInteractingPos = 0f;
    float rightHandInteractingPos = 0f;


    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            if (photonView.Owner.CustomProperties.TryGetValue(PlayerSync.PLAYER_SHIP, out object o))
            {
                shipId = (int)o;
            }
            else
            {
                Debug.LogError("Instantiated playersync has no ship id!", this);
                return;
            }
        }
        RoomController.i.RegisterPlayer(this);

        receivedPos = pos;
    }

    // Update is called once per frame
    void Update()
    {
        if (playArea != null)
        {
            if (photonView.IsMine)
            {
                float inputX = Input.GetAxis("Horizontal");
                float inputY = Input.GetAxis("Vertical");
                float inputInteract = Input.GetAxis("Submit");

                if (inputInteract > 0 && !interacting)
                {
                    if (interactables.InInteractableReach(playArea.TransformPoint(pos) + Vector3.up, out ShipInteractables.InteractingThing thing, out float p))
                    {
                        interactingThing = thing;
                        switch (thing)
                        {
                            case ShipInteractables.InteractingThing.LeftWheel:
                                // These are the positions of the hands in radians in the wheel
                                leftHandInteractingPos = p + Mathf.PI * 0.5f;
                                rightHandInteractingPos = -p + Mathf.PI * 0.5f;
                                break;
                            case ShipInteractables.InteractingThing.RightWheel:
                                // These are the positions of the hands in radians in the wheel
                                leftHandInteractingPos = p + Mathf.PI * 0.5f;
                                rightHandInteractingPos = -p + Mathf.PI * 0.5f;
                                break;
                            case ShipInteractables.InteractingThing.Rope:
                                // Positions of hands in rope
                                leftHandInteractingPos = Mathf.Clamp(p - 0.05f, -1f, 1f);
                                rightHandInteractingPos = Mathf.Clamp(p + 0.05f, -1f, 1f);
                                break;
                        }
                        interacting = true;
                    }
                }
                else if (inputInteract <= 0.01f && interacting)
                    interacting = false;

                if (!interacting)
                {
                    pos.x += inputX * speed * Time.deltaTime;
                    pos.y += inputY * speed * Time.deltaTime;
                    interactingInput = 0;
                }
                else {
                    Vector3 wantedInteractingPos = Vector3.zero;
                    switch (interactingThing)
                    {
                        case ShipInteractables.InteractingThing.LeftWheel:
                            wantedInteractingPos = interactables.leftWheel.transform.position - interactables.leftWheel.transform.forward * 0.5f;
                            break;
                        case ShipInteractables.InteractingThing.RightWheel:
                            wantedInteractingPos = interactables.rightWheel.transform.position - interactables.rightWheel.transform.forward * 0.5f;
                            break;
                        case ShipInteractables.InteractingThing.Rope:
                            wantedInteractingPos = interactables.rope.RopeRelativePointToWorld(Mathf.Lerp(leftHandInteractingPos, rightHandInteractingPos, 0.5f));
                            break;
                    }

                    Vector2 wantedInteractingShipPos = playArea.InverseTransformPoint(wantedInteractingPos);

                    pos = Vector3.MoveTowards(pos, wantedInteractingShipPos, speed * Time.deltaTime * 0.5f);

                    // input
                    interactingInput = inputX;
                }

                GetPushedByOtherPlayers(ref pos);
            }
            else {

                GetPushedByOtherPlayers(ref receivedPos);
                playArea.EnsureCircleInsideArea(ref receivedPos, collisionRadius);

                pos = Vector2.Lerp(pos, receivedPos, 5f * Time.deltaTime);
            }

            playArea.EnsureCircleInsideArea(ref pos, collisionRadius);


            transform.position = playArea.TransformPoint(pos);
            transform.rotation = Quaternion.identity;

            // Body parts

        }
    }

    void GetPushedByOtherPlayers(ref Vector2 ownPosition)
    {
        Vector2 push = Vector2.zero;
        foreach (Photon.Realtime.Player p in RoomController.i.shipIdToPlayers[shipId])
        {
            if (p != photonView.Owner)
            {
                Vector2 otherPos = RoomController.i.playerSyncs[p].pos;

                float distance = Vector2.Distance(ownPosition, otherPos);
                if (distance < collisionRadius * 2f)
                {
                    float pushIntensity = 1 - (distance / (collisionRadius * 2f));
                    pushIntensity = Easing.Cubic.Out(pushIntensity);
                    Vector2 playerPush = (ownPosition - otherPos).normalized * pushIntensity;
                    push += playerPush;

                    //Debug.Log(string.Format("pushing! own {0}, other {1}, intensity {2}, playerPush {3}", ownPosition, otherPos, pushIntensity, playerPush));
                }
            }
        }

        //Debug.Log(push);
        ownPosition += push * pushSpeed * Time.deltaTime;
    }

    public void PlaceOnShip(ShipSync s)
    {
        playArea = s.visualShip.GetComponent<ShipPlayArea>();
        interactables = s.visualShip.GetComponent<ShipInteractables>();

        transform.SetParent(playArea.areaCenter);

        if (photonView.IsMine)
        {
            pos = new Vector2(Random.Range(playArea.minMaxX.x, playArea.minMaxX.y), playArea.minMaxZ.x);

            Camera.main.GetComponent<UnityGLTF.Examples.OrbitCameraController>().target = playArea.areaCenter.transform;

            if (ShipUI.instance)
                ShipUI.instance.ship = s.visualShip;
        }
    }

    Vector2 receivedPos;

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(pos);
            stream.SendNext(interacting);
            if (interacting)
            {
                stream.SendNext(interactingThing);
                stream.SendNext(interactingInput);
                stream.SendNext(rightHandInteractingPos);
                stream.SendNext(leftHandInteractingPos);
            }
        }
        else
        {
            receivedPos = (Vector2)stream.ReceiveNext();
            interacting = (bool)stream.ReceiveNext();
            if (interacting)
            {
                interactingThing = (ShipInteractables.InteractingThing)stream.ReceiveNext();
                interactingInput = (float)stream.ReceiveNext();
                rightHandInteractingPos = (float)stream.ReceiveNext();
                leftHandInteractingPos = (float)stream.ReceiveNext();
            }
        }
    }

    public bool IsInteracting() {
        return interacting;
    }

    public ShipInteractables.InteractingThing WhichInteractable() {
        return interactingThing;
    }

    public float InteractingInput() {
        return interactingInput;
    }
}
