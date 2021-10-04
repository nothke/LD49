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

    public Transform rightHand, leftHand;
    public Transform rightFoot, leftFoot;

    public Transform restRightHand, restLeftHand;
    Vector3 restRightHandPos, restLeftHandPos;

    Vector3 gizmoDebugPos = Vector3.zero;


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

        lastFramePosition = receivedPos = pos;

        restRightHandPos = restRightHand.position;
        restLeftHandPos = restLeftHand.position;
    }

    float interactingTime = 0;
    Vector2 lastFramePosition;
    Vector2 instantVelocityAverage;
    Vector2 lastFacingDirection = Vector2.up;
    bool wasInteracting = false;

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
                    }
                    else
                    {
                        interactingThing = ShipInteractables.InteractingThing.Nothing;
                        leftHandInteractingPos = Random.Range(-0.2f, 0.2f);
                    }
                    interacting = true;
                    interactingTime = 0;
                }
                else if (inputInteract <= 0.01f && interacting)
                {
                    interacting = false;
                    interactingTime = 0;
                }

                if (!interacting || interactingThing == ShipInteractables.InteractingThing.Nothing)
                {
                    Vector3 camRight = Camera.main.transform.right;
                    camRight.y = 0;
                    camRight.Normalize();
                    Vector3 camForward = Camera.main.transform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Vector3 camRelativeInput = camRight * inputX + camForward * inputY;
                    Vector2 shipRelativeInput = playArea.InverseTransformDirection(camRelativeInput);

                    //Debug.Log(inputX + " "+ inputY + " => "+ camRelativeInput + " == "+shipRelativeInput);

                    pos += shipRelativeInput * speed * Time.deltaTime;

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
                            wantedInteractingPos -= interactables.leftWheel.transform.forward * 0.5f;
                            break;
                    }
                    gizmoDebugPos = wantedInteractingPos;

                    Vector2 wantedInteractingShipPos = playArea.InverseTransformPoint(wantedInteractingPos);

                    float intFactor = Mathf.Clamp01(interactingTime / 2f);
                    pos = Vector3.Lerp(pos, wantedInteractingShipPos, intFactor);

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

            Vector2 deltaPos = pos - lastFramePosition;
            lastFramePosition = pos;

            Vector2 instantVelocity = deltaPos / Time.deltaTime;
            instantVelocityAverage = instantVelocityAverage * 0.3f + instantVelocity * 0.7f;

            Vector2 facingDirection = interacting && interactingThing != ShipInteractables.InteractingThing.Nothing?
                Vector2.up : instantVelocityAverage.sqrMagnitude > 0.001f? instantVelocityAverage.normalized : lastFacingDirection;

            lastFacingDirection = facingDirection;

            transform.rotation = Quaternion.LookRotation(playArea.TransformDirection(facingDirection), Vector3.up);

            // Body parts
            restRightHandPos = Vector3.Lerp(restRightHandPos, restRightHand.position, Time.deltaTime * 5f);
            restLeftHandPos = Vector3.Lerp(restLeftHandPos, restLeftHand.position, Time.deltaTime * 5f);

            float maxHandDistance = 0.5f;
            if (Vector3.Distance(restRightHandPos, restRightHand.position) > maxHandDistance)
            {
                restRightHandPos = restRightHand.position + (restRightHandPos - restRightHand.position).normalized * maxHandDistance;
            }
            if (Vector3.Distance(restLeftHandPos, restLeftHand.position) > maxHandDistance)
            {
                restLeftHandPos = restLeftHand.position + (restLeftHandPos - restLeftHand.position).normalized * maxHandDistance;
            }

            Vector3 wantedLeftHandPos = restLeftHandPos;
            Vector3 wantedRightHandPos = restRightHandPos;
            if (interacting)
            {
                switch (interactingThing)
                {
                    case ShipInteractables.InteractingThing.LeftWheel:
                        wantedLeftHandPos = interactables.leftWheel.WheelPositionForAngle(leftHandInteractingPos);
                        wantedRightHandPos = interactables.leftWheel.WheelPositionForAngle(rightHandInteractingPos);
                        break;
                    case ShipInteractables.InteractingThing.RightWheel:
                        wantedLeftHandPos = interactables.rightWheel.WheelPositionForAngle(leftHandInteractingPos);
                        wantedRightHandPos = interactables.rightWheel.WheelPositionForAngle(rightHandInteractingPos);
                        break;
                    case ShipInteractables.InteractingThing.Rope:
                        wantedLeftHandPos = interactables.rope.RopeRelativePointToWorld(leftHandInteractingPos);
                        wantedRightHandPos = interactables.rope.RopeRelativePointToWorld(rightHandInteractingPos);
                        break;
                    case ShipInteractables.InteractingThing.Nothing:
                        wantedLeftHandPos.y += 0.5f + leftHandInteractingPos;
                        wantedRightHandPos.y += 0.5f - leftHandInteractingPos;
                        break;
                }
            }
            interactingTime += Time.deltaTime;

            float interactingFinishFactor = Mathf.Clamp01(interactingTime / 1f);

            leftHand.position = Vector3.Lerp(leftHand.position, wantedLeftHandPos, interactingFinishFactor);
            rightHand.position = Vector3.Lerp(rightHand.position, wantedRightHandPos, interactingFinishFactor);
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

    void OnDrawGizmos() {
        if (interacting && interactingThing != ShipInteractables.InteractingThing.Nothing)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(gizmoDebugPos, 0.2f);
        }
    }
}
