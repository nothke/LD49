#define NEW_INTERACTION

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

#if !NEW_INTERACTION
    ShipInteractables.InteractingThing interactingThing = ShipInteractables.InteractingThing.Rope;
#endif

    float interactingInput = 0;
    float leftHandHoldStartFactor = 0f;
    float rightHandHoldStartFactor = 0f;

    [Header("Hands")]
    public Transform rightHand, leftHand;
    public Transform restRightHand, restLeftHand;
    Vector3 restRightHandPos, restLeftHandPos;

    //[Header("Feet")]
    PlayerFeet feet;
    
    Vector3 gizmoDebugPos = Vector3.zero;

    float interactingAnimationTime = 0;
    Vector2 lastFramePosition;
    Vector2 instantVelocityAverage;
    Vector2 lastFacingDirection = Vector2.up;

#if !NEW_INTERACTION
    ShipInteractables.InteractingThing lastCloseTo;
#endif

    Interactable lastInteractableInRange;
    Interactable interactable;
    float handStartFactor;

    public Interactable CurrentlyInteractingWith => interactable;

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

#if NEW_INTERACTION
                {
                    Interactable interactableInRange =
                        interactables.InInteractableReach(playArea.TransformPoint(pos) + Vector3.up);

                    bool startedInteracting = inputInteract > 0 && !interacting;
                    bool endedInteracting = inputInteract <= 0.01f && interacting;

                    if (startedInteracting)
                    {
                        if (interactableInRange)
                        {
                            interactable = interactableInRange;
                            interacting = true;
                            interactable.OnStartedInteracting();

                            handStartFactor = interactable.GetHandStartFactor();
                            interactable.GetHandStartFactors(
                                out leftHandHoldStartFactor,
                                out rightHandHoldStartFactor,
                                handStartFactor);

                            Debug.Log("Started interacting with: " + interactable);
                        }
                        else // if no interactables in range
                        {
                            // Raise hand in the air
                            leftHandHoldStartFactor = Random.Range(-0.2f, 0.2f);
                            ShipUI.instance.EnableWheelSlider(false);
                        }

                        interactingAnimationTime = 0;
                    }
                    else if (endedInteracting)
                    {
                        interactingAnimationTime = 0;

                        interactable.OnEndedInteracting();
                        interactable = null;
                        interacting = false;
                    }

                    // Body position handling

                    if (interactable && interacting)
                    {
                        Vector3 desiredBodyPosition = interactable.GetTargetBodyPosition(leftHandHoldStartFactor, rightHandHoldStartFactor);

                        gizmoDebugPos = desiredBodyPosition;

                        Vector2 wantedInteractingShipPos = playArea.InverseTransformPoint(desiredBodyPosition);

                        float intFactor = Mathf.Clamp01(interactingAnimationTime / 2f);
                        pos = Vector3.Lerp(pos, wantedInteractingShipPos, intFactor);

                        // input
                        interactingInput = inputX;
                    }
                    else
                    {
                        // Player movement

                        Vector3 camRight = Camera.main.transform.right;
                        Vector3 camForward = Camera.main.transform.forward;
                        //camForward.y = 0;
                        //camForward.Normalize();
                        Vector3 camUp = Camera.main.transform.up;

                        Vector2 input = new Vector2(inputX, inputY);
                        if (input.sqrMagnitude > 1f) input.Normalize();

                        Vector3 camRelativeInput = camRight * input.x + camForward * input.y + camUp * input.y;
                        Vector2 shipRelativeInput = playArea.InverseTransformDirection(camRelativeInput).normalized * input.magnitude;

                        //Debug.Log(inputX + " "+ inputY + " => "+ camRelativeInput + " == "+shipRelativeInput);

                        pos += shipRelativeInput * speed * Time.deltaTime;

                        interactingInput = 0;
                    }

                    GetPushedByOtherPlayers(ref pos);

                    // On got close to
                    if (interactableInRange != lastInteractableInRange)
                    {
                        if (interactableInRange)
                            interactableInRange.Highlight();
                        else
                        {
                            Facepunch.Highlight.ClearAll();
                            Facepunch.Highlight.Rebuild();
                            ShipUI.instance.SetInteractionText("");
                        }
                    }

                    lastInteractableInRange = interactableInRange;
                }
#else

                /// OLD

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
                                leftHandHoldStartFactor = p + Mathf.PI * 0.5f;
                                rightHandHoldStartFactor = -p + Mathf.PI * 0.5f;
                                ShipUI.instance.EnableWheelSlider(true);
                                break;
                            case ShipInteractables.InteractingThing.RightWheel:
                                // These are the positions of the hands in radians in the wheel
                                leftHandHoldStartFactor = p + Mathf.PI * 0.5f;
                                rightHandHoldStartFactor = -p + Mathf.PI * 0.5f;
                                ShipUI.instance.EnableWheelSlider(true);
                                break;
                            case ShipInteractables.InteractingThing.Rope:
                                // Positions of hands in rope
                                leftHandHoldStartFactor = Mathf.Clamp(p - 0.05f, -1f, 1f);
                                rightHandHoldStartFactor = Mathf.Clamp(p + 0.05f, -1f, 1f);
                                break;
                        }
                    }
                    else
                    {
                        interactingThing = ShipInteractables.InteractingThing.Nothing;
                        // Raise hand in the air
                        leftHandHoldStartFactor = Random.Range(-0.2f, 0.2f);
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
                    Vector3 camForward = Camera.main.transform.forward;
                    //camForward.y = 0;
                    //camForward.Normalize();
                    Vector3 camUp = Camera.main.transform.up;

                    Vector2 input = new Vector2(inputX, inputY);
                    if (input.sqrMagnitude > 1f) input.Normalize();

                    Vector3 camRelativeInput = camRight * input.x + camForward * input.y + camUp * input.y;
                    Vector2 shipRelativeInput = playArea.InverseTransformDirection(camRelativeInput).normalized * input.magnitude;

                    //Debug.Log(inputX + " "+ inputY + " => "+ camRelativeInput + " == "+shipRelativeInput);

                    pos += shipRelativeInput * speed * Time.deltaTime;

                    interactingInput = Mathf.MoveTowards(interactingInput, 0, Time.deltaTime * 3f);
                }
                else // during interaction with an interactable
                { 
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
                            wantedInteractingPos = interactables.rope.RopeRelativePointToWorld(Mathf.Lerp(leftHandHoldStartFactor, rightHandHoldStartFactor, 0.5f));
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
#endif
            }
            else // if not photonView.IsMine
            {
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

#if NEW_INTERACTION
            bool _interacting = interacting && interactable;
#else
            bool _interacting = interacting && interactingThing != ShipInteractables.InteractingThing.Nothing;
#endif

            Vector2 facingDirection = _interacting ?
                Vector2.up : instantVelocityAverage.sqrMagnitude > 0.001f ? instantVelocityAverage.normalized : lastFacingDirection;

            facingDirection = Vector2.Lerp(lastFacingDirection, facingDirection, 20f * Time.deltaTime);
            facingDirection.Normalize();

            lastFacingDirection = facingDirection;

            transform.rotation = Quaternion.LookRotation(playArea.TransformDirection(facingDirection), Vector3.Slerp(Vector3.up, playArea.areaCenter.up, 0.2f));

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

#if NEW_INTERACTION
            // Hand positions
            if (interacting)
            {
                if (interactable)
                {
                    interactable.GetHandPositions(
                        out wantedLeftHandPos, out wantedRightHandPos,
                        leftHandHoldStartFactor, rightHandHoldStartFactor);
                }
                else
                {
                    // raise hands in the air
                    wantedLeftHandPos = restLeftHand.position + transform.forward * 0.3f + transform.up * (0.6f + 0.4f * leftHandHoldStartFactor) - transform.right * 0.3f;
                    wantedRightHandPos = restRightHand.position + transform.forward * 0.3f + transform.up * (0.6f + 0.4f * -leftHandHoldStartFactor) + transform.right * 0.3f;
                }
            }
#else

            if (interacting)
            {
                switch (interactingThing)
                {
                    case ShipInteractables.InteractingThing.LeftWheel:
                        wantedLeftHandPos = interactables.leftWheel.WheelPositionForAngle(leftHandHoldStartFactor);
                        wantedRightHandPos = interactables.leftWheel.WheelPositionForAngle(rightHandHoldStartFactor);
                        break;
                    case ShipInteractables.InteractingThing.RightWheel:
                        wantedLeftHandPos = interactables.rightWheel.WheelPositionForAngle(leftHandHoldStartFactor);
                        wantedRightHandPos = interactables.rightWheel.WheelPositionForAngle(rightHandHoldStartFactor);
                        break;
                    case ShipInteractables.InteractingThing.Rope:
                        wantedLeftHandPos = interactables.rope.RopeRelativePointToWorld(leftHandHoldStartFactor);
                        wantedRightHandPos = interactables.rope.RopeRelativePointToWorld(rightHandHoldStartFactor);
                        break;
                    case ShipInteractables.InteractingThing.Nothing:
                        wantedLeftHandPos = restLeftHand.position + transform.forward * 0.3f + transform.up * (0.6f + 0.4f * leftHandHoldStartFactor) - transform.right * 0.3f;
                        wantedRightHandPos = restRightHand.position + transform.forward * 0.3f + transform.up * (0.6f + 0.4f * -leftHandHoldStartFactor) + transform.right * 0.3f;
                        break;
                }
            }
#endif

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

            Camera.main.GetComponent<UnityGLTF.Examples.OrbitCameraController>().target = s.visualShip.cameraFocus;

            if (ShipUI.instance)
            {
                ShipUI.instance.ship = s.visualShip;
                ShipUI.instance.shipIdText.text = string.Format("Vessel #{0}", shipId);
            }
        }

        feet.Init(pos, Vector2.up);
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
#if NEW_INTERACTION
                stream.SendNext(interactable ? interactable.id : -1);
#else
                stream.SendNext(interactingThing);
#endif
                stream.SendNext(interactingInput);
                stream.SendNext(rightHandHoldStartFactor);
                stream.SendNext(leftHandHoldStartFactor);
            }
        }
        else // if reading
        {
            receivedPos = (Vector2)stream.ReceiveNext();

            interacting = (bool)stream.ReceiveNext();
            if (interacting)
            {
#if NEW_INTERACTION
                SetInteractableFromId((int)stream.ReceiveNext());
#else
                interactingThing = (ShipInteractables.InteractingThing)stream.ReceiveNext();
#endif
                interactingInput = (float)stream.ReceiveNext();
                rightHandHoldStartFactor = (float)stream.ReceiveNext();
                leftHandHoldStartFactor = (float)stream.ReceiveNext();
            }

            if (wasInteracting != interacting)
            {
                wasInteracting = interacting;
                interactingAnimationTime = 0f;
            }
        }
    }

    public bool IsInteracting()
    {
        return interacting;
    }

#if !NEW_INTERACTION
    public ShipInteractables.InteractingThing WhichInteractable() {
        return interactingThing;
    }
#endif

    void SetInteractableFromId(int id)
    {
        if (id < 0)
            interactable = null;
        else if (id < interactables.interactables.Length)
            interactable = interactables.interactables[id];
        else
            Debug.LogError("Attempting to set an id that is out of range of interactables, you might be running a wrong version with a different number of interactables?");
    }

    public float InteractingInput()
    {
        return interactingInput;
    }

    void OnDrawGizmos()
    {
#if NEW_INTERACTION
        bool _interacting = interacting;
#else
        bool _interacting = interacting && interactingThing != ShipInteractables.InteractingThing.Nothing;
#endif

        if (_interacting)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(gizmoDebugPos, 0.2f);
        }
    }
}
