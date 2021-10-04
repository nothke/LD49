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
    [Header("Player")]
    public float collisionRadius = 0.5f;
    public float speed = 10f;
    public float pushSpeed = 5f;
    public float boatPushWeight = 10f;

    bool interacting = false;
    ShipInteractables.InteractingThing interactingThing = ShipInteractables.InteractingThing.Rope;
    float interactingInput = 0;
    float leftHandInteractingPos = 0f;
    float rightHandInteractingPos = 0f;

    [Header("Hands")]
    public Transform rightHand, leftHand;
    public Transform restRightHand, restLeftHand;
    Vector3 restRightHandPos, restLeftHandPos;

    //[Header("Feet")]
    PlayerFeet feet;


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

        feet = GetComponent<PlayerFeet>();
    }

    float interactingAnimationTime = 0;
    Vector2 lastFramePosition;
    Vector2 instantVelocityAverage;
    Vector2 lastFacingDirection = Vector2.up;

    ShipInteractables.InteractingThing lastCloseTo;

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

                bool inReach = interactables.InInteractableReach(playArea.TransformPoint(pos) + Vector3.up, 
                    out ShipInteractables.InteractingThing thing, out float p);
                
                // On started interacting
                if (inputInteract > 0 && !interacting)
                {
                    if (inReach)
                    {
                        interactingThing = thing;
                        switch (thing)
                        {
                            case ShipInteractables.InteractingThing.LeftWheel:
                                // These are the positions of the hands in radians in the wheel
                                leftHandInteractingPos = p + Mathf.PI * 0.5f;
                                rightHandInteractingPos = -p + Mathf.PI * 0.5f;
                                ShipUI.instance.EnableWheelSlider(true);
                                break;
                            case ShipInteractables.InteractingThing.RightWheel:
                                // These are the positions of the hands in radians in the wheel
                                leftHandInteractingPos = p + Mathf.PI * 0.5f;
                                rightHandInteractingPos = -p + Mathf.PI * 0.5f;
                                ShipUI.instance.EnableWheelSlider(true);
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
                        ShipUI.instance.EnableWheelSlider(false);
                    }
                    interacting = true;
                    interactingAnimationTime = 0;
                }
                else if (inputInteract <= 0.01f && interacting)
                {
                    interacting = false;
                    interactingAnimationTime = 0;
                }

                if (!interacting || interactingThing == ShipInteractables.InteractingThing.Nothing)
                {
                    Vector3 camRight = Camera.main.transform.right;
                    camRight.y = 0;
                    camRight.Normalize();
                    Vector3 camForward = Camera.main.transform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Vector2 input = new Vector2(inputX, inputY);
                    if (input.sqrMagnitude > 1f) input.Normalize();

                    Vector3 camRelativeInput = camRight * input.x + camForward * input.y;
                    Vector2 shipRelativeInput = playArea.InverseTransformDirection(camRelativeInput).normalized * input.magnitude;

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

                    float intFactor = Mathf.Clamp01(interactingAnimationTime / 2f);
                    pos = Vector3.Lerp(pos, wantedInteractingShipPos, intFactor);

                    // input
                    interactingInput = inputX;
                }

                GetPushedByOtherPlayers(ref pos);

                // In reach detection

                if (!inReach || interacting) 
                    thing = ShipInteractables.InteractingThing.Nothing;

                // On got close to
                if (lastCloseTo != thing)
                {
                    //var ship = RoomController.i.ships[shipId];
                    interactables.Highlight(thing);
                }

                lastCloseTo = thing;
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

            facingDirection = Vector2.Lerp(lastFacingDirection, facingDirection, 20f * Time.deltaTime);
            facingDirection.Normalize();

            lastFacingDirection = facingDirection;

            transform.rotation = Quaternion.LookRotation(playArea.TransformDirection(facingDirection), Vector3.up);

            ////////////////
            // Body parts //
            ////////////////
            
            // Hands
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
            interactingAnimationTime += Time.deltaTime;

            float interactingFinishFactor = Mathf.Clamp01(interactingAnimationTime / 1f);

            // TODO add inertia instead of lerping
            leftHand.position = Vector3.Lerp(leftHand.position, wantedLeftHandPos, interactingFinishFactor);
            rightHand.position = Vector3.Lerp(rightHand.position, wantedRightHandPos, interactingFinishFactor);

            // Feet
            feet.UpdateFeet(pos, facingDirection);
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

        if (feet == null) feet = GetComponent<PlayerFeet>();
        feet.playArea = playArea;
        interactables = s.visualShip.GetComponent<ShipInteractables>();

        transform.SetParent(playArea.areaCenter);

        if (photonView.IsMine)
        {
            pos = new Vector2(Random.Range(playArea.minMaxX.x, playArea.minMaxX.y), playArea.minMaxZ.x);

            feet.Init(pos, Vector2.up);

            Camera.main.GetComponent<UnityGLTF.Examples.OrbitCameraController>().target = playArea.areaCenter.transform;

            if (ShipUI.instance)
            {
                ShipUI.instance.ship = s.visualShip;
                ShipUI.instance.shipIdText.text = string.Format("Vessel #{0}", shipId);
            }
        }
    }

    Vector2 receivedPos;
    bool wasInteracting = false;

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

            if (wasInteracting != interacting)
            {
                wasInteracting = interacting;
                interactingAnimationTime = 0f;
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
