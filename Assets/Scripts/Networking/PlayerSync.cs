#define NEW_INTERACTION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    public int shipId { get; private set; } = -1;
    public int originalShipId { get; private set; } = -1;

    ShipPlayArea playArea;
    ShipInteractables interactables;

    Vector3 pos = Vector3.zero;
    [Header("Player")]
    public float collisionRadius = 0.5f;
    public float speed = 10f;
    public float pushSpeed = 5f;
    public float boatPushWeight = 10f;
    public float gravity = -9.81f;

    public float jumpSpeed = 2f;
    float verticalSpeed = 0;

    bool interacting = false;

    float interactingInput = 0;
    float leftHandHoldStartFactor = 0f;
    float rightHandHoldStartFactor = 0f;

    [Header("Hands")]
    public Transform rightHand, leftHand;
    public Transform restRightHand, restLeftHand;
    Vector3 restRightHandPos, restLeftHandPos;

    PlayerFeet feet;

    Vector3 gizmoDebugPos = Vector3.zero;

    float interactingAnimationTime = 0;
    Vector3 lastFramePosition;
    bool lastFrameUnderwater = false;
    Vector2 instantVelocityAverage;
    Vector2 lastFacingDirection = Vector2.up;

    Interactable lastInteractableInRange;
    Interactable interactable;
    float handStartFactor;

    Vector3 receivedPos;
    bool wasInteracting = false;
    Interactable highlightedInteractable;

    public Interactable CurrentlyInteractingWith => interactable;

    [Header("Sounds that should probably be in another script")]
    public AudioClip[] holdSailSounds;
    public AudioClip[] releaseSailSounds;
    public UnityEngine.Audio.AudioMixerGroup ownStepsMixerGroup, othersStepsMixerGroup;
    public UnityEngine.Audio.AudioMixerGroup ownGrabsMixerGroup, othersGrabsMixerGroup;
    [Header("Mouth")]
    public MouthSounds mouth;

    [Header("World movement")]
    public PlayerFreeMovement worldMovement;
    public float waterDynamicFriction = 0.1f;
    public float waterToBodyDensityRatio = 1.1f; // more than 1 means denser water than body

    bool jumping = false;

    public bool IsJumping {
        get { return jumping; }
    }


    void Start()
    {
        /*if (!photonView.IsMine)
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
        RoomController.i.RegisterPlayer(this);*/
    }

    public void SetShipIdLocally(int sid) {
        int prev = shipId;
        shipId = sid;

        RoomController.i.RassignPlayerToShip(this, prev);
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;

        PlayerColors colorManager = GetComponent<PlayerColors>();
        if (colorManager)
        {
            colorManager.SetColors((int)instantiationData[1], (int)instantiationData[2], (int)instantiationData[3], (int)instantiationData[4]);
        }

        if (mouth)
        {
            mouth.pitchRange = new Vector2((float)instantiationData[5], (float)instantiationData[6]);
        }

        lastFramePosition = receivedPos = pos;

        restRightHandPos = restRightHand.position;
        restLeftHandPos = restLeftHand.position;

        feet = GetComponent<PlayerFeet>();
        //

        originalShipId = (int)instantiationData[0];
        ShipSync s = RoomController.i.ships[originalShipId];
        colorManager.SetShipColor(s.shipLiveryColorCombination);

        RoomController.i.RegisterPlayer(this, originalShipId);
    }

    float lastJumpingAxis = 0;

    void Update()
    {
        if (playArea != null)
        {
            if (photonView.IsMine)
            {
                float inputX = Input.GetAxis("Horizontal");
                float inputY = Input.GetAxis("Vertical");
                float inputInteract = Input.GetAxis("Submit");
                float inputJump = Input.GetAxis("Jump");

                Interactable interactableInRange =
                    interactables.InInteractableReach(playArea.areaCenter.TransformPoint(pos) + Vector3.up);

                bool startedInteracting = inputInteract > 0 && !interacting;
                bool endedInteracting = inputInteract <= 0.01f && interacting;
                bool startedJumping = inputJump > 0 && lastJumpingAxis <= 0.01f;
                lastJumpingAxis = inputJump;

                if (startedInteracting)
                {
                    if (interactableInRange)
                    {
                        interactable = interactableInRange;
                        interactable.OnStartedInteracting();

                        PlayStartInteractingSound(interactable);

                        handStartFactor = interactable.GetHandStartFactor();
                        interactable.GetHandStartFactors(
                            out leftHandHoldStartFactor,
                            out rightHandHoldStartFactor,
                            handStartFactor);

                        //Debug.Log("Local player Started interacting with: " + interactable);
                    }
                    else // if no interactables in range
                    {
                        // Raise hand in the air
                        leftHandHoldStartFactor = Random.Range(-0.2f, 0.2f); // used for raised-hand positioning
                        ShipUI.instance.EnableWheelSlider(false);
                        ShipUI.instance.EnableWindViz(false);

                        mouth.MiauNetwork(photonView);
                    }

                    interacting = true;
                    interactingAnimationTime = 0;
                }
                else if (endedInteracting)
                {
                    interactingAnimationTime = 0;

                    if (interactable)
                    {
                        interactable.OnEndedInteracting();

                        PlayEndInteractingSound(interactable);
                    }
                    interactable = null;
                    interacting = false;
                }

                if (!interacting && startedJumping)
                {
                    // Jump
                    // TODO

                    if (shipId != -1)
                    {
                        worldMovement.enabled = !worldMovement.enabled;

                        int prevShip = shipId;
                        if (worldMovement.enabled)
                        {
                            shipId = -1;
                        }
                        else
                        {
                            shipId = RoomController.i.ClosestShipTo(transform.position);
                        }

                        RoomController.i.RassignPlayerToShip(this, prevShip);
                    }
                    else
                    {
                        if (!jumping)
                        {
                            verticalSpeed = jumpSpeed;
                            jumping = true;

                            // TODO prevent jumping if boat tilted? force a jump to the sea in that case
                        }
                    }
                    /*
                    worldMovement.enabled = !worldMovement.enabled;

                    int prevShip = shipId;
                    if (worldMovement.enabled)
                    {
                        shipId = -1;
                    }
                    else {
                        shipId = RoomController.i.ClosestShipTo(transform.position);
                    }

                    RoomController.i.RassignPlayerToShip(this, prevShip);
                    */
                }


                // Body position handling
                if (interactable && interacting)
                {
                    Vector3 desiredBodyPosition = interactable.GetTargetBodyPosition(leftHandHoldStartFactor, rightHandHoldStartFactor);

                    gizmoDebugPos = desiredBodyPosition;

                    desiredBodyPosition.y = 0;

                    Vector3 wantedInteractingShipPos = playArea.areaCenter.InverseTransformPoint(desiredBodyPosition);

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

                    
                    if (worldMovement.enabled)
                    {
                        //shipRelativeInput *= 3f;

                        if (Input.GetKey(KeyCode.LeftShift))
                            shipRelativeInput *= 5f;
                    }

                    //Debug.Log(inputX + " "+ inputY + " => "+ camRelativeInput + " == "+shipRelativeInput);

                    pos += new Vector3(shipRelativeInput.x, 0, shipRelativeInput.y) * speed * Time.deltaTime;

                    interactingInput = 0;
                }

                GetPushedByOtherPlayers(ref pos);

                // Highligting should only happen if not interacting
                if (!interacting)
                {
                    // On got close to
                    if (interactableInRange != lastInteractableInRange || endedInteracting)
                    {
                        if (interactableInRange)
                        {
                            interactableInRange.Highlight();
                            highlightedInteractable = interactableInRange;
                        }
                        else if (highlightedInteractable)
                        {
                            Facepunch.Highlight.ClearAll();
                            Facepunch.Highlight.Rebuild();
                            ShipUI.instance.SetInteractionText("");
                            highlightedInteractable = null;
                        }
                    }
                }
                else if (highlightedInteractable)
                {
                    Facepunch.Highlight.ClearAll();
                    Facepunch.Highlight.Rebuild();
                    ShipUI.instance.SetInteractionText("");
                    highlightedInteractable = null;
                }

                lastInteractableInRange = interactableInRange;

                // Jumping
                if (shipId >= 0)
                { // On a boat
                    if (jumping || pos.y > 0)
                    {
                        pos.y += verticalSpeed * Time.deltaTime;
                        verticalSpeed += Time.deltaTime * gravity;

                        if (pos.y <= 0)
                        {
                            // Hit the boat

                            ShipSync ss = RoomController.i.ships[shipId];
                            Vector3 impulse = Vector3.up * verticalSpeed;
                            //ss.localShip.rb.AddForceAtPosition(impulse, transform.position, ForceMode.Impulse);
                            Debug.Log("hit a boat after jumping");
                            jumping = false;
                            pos.y = 0;
                            receivedPos.y = 0;
                            verticalSpeed = 0;
                        }
                    }
                    else
                    {
                        pos.y = 0;
                        receivedPos.y = 0;
                        verticalSpeed = 0;
                        jumping = false;
                    }
                }
                else
                { // On water or land
                    float minPos = ((WorldPlayArea)playArea).GetMinPlayerPositionY(pos);
                    float waterLvl = Water.GetHeight(pos);

                    float prevSpeed = verticalSpeed;

                    bool isUnderwater = pos.y < waterLvl;
                    if (isUnderwater)
                    {
                        float depth = waterLvl - pos.y;
                        float buoyancyFactor = waterToBodyDensityRatio *  Mathf.Clamp01(depth / 1.1f);
                        verticalSpeed += -gravity * buoyancyFactor * Time.deltaTime;
                    }

                    if (jumping || pos.y > minPos || isUnderwater)
                    {
                        pos.y += verticalSpeed * Time.deltaTime;
                        verticalSpeed += Time.deltaTime * gravity;

                        //Debug.Log("Being affected by gravity");

                        if (pos.y <= minPos && verticalSpeed < 0)
                        {
                            // Hit the boat

                            //ShipSync ss = RoomController.i.ships[shipId];
                            //Vector3 impulse = Vector3.up * verticalSpeed;
                            //ss.localShip.rb.AddForceAtPosition(impulse, transform.position, ForceMode.Impulse);
                            //Debug.Log("hit a world after jumping");
                            jumping = false;
                            pos.y = minPos;
                            receivedPos.y = minPos;

                            if (!isUnderwater || verticalSpeed < 0)
                                verticalSpeed = 0;
                        }
                    }
                    else if (!jumping && !isUnderwater)
                    {
                        pos.y = minPos;
                        receivedPos.y = minPos;
                        //Debug.Log("standing on world, above water");
                    }

                    if (isUnderwater)
                    {
                        if (prevSpeed < 0 && verticalSpeed >= 0 && jumping)
                        {
                            jumping = false;
                            //Debug.Log("not jumping because water pushing us up now");
                        }
                        if (verticalSpeed < 0) verticalSpeed = Mathf.Lerp(verticalSpeed, 0, Time.deltaTime * waterDynamicFriction);
                    }

                    //Debug.Log($"underwater: {underwater}, jumping: {jumping}, verticalSpeed: {verticalSpeed}, [minpos, waterLevel]: [{minPos}, {waterLevel}]:{pos.y}");
                }
            }
            else // if not photonView.IsMine
            {
                GetPushedByOtherPlayers(ref receivedPos);

                // Jumping
                if (shipId >= 0)
                { // On a boat
                    if (jumping || receivedPos.y > 0)
                    {
                        receivedPos.y += verticalSpeed * Time.deltaTime;
                        pos.y += verticalSpeed * Time.deltaTime;
                        verticalSpeed += Time.deltaTime * gravity;

                        if (pos.y <= 0)
                        {
                            // Hit the boat

                            ShipSync ss = RoomController.i.ships[shipId];
                            Vector3 impulse = Vector3.up * verticalSpeed;
                            //ss.localShip.rb.AddForceAtPosition(impulse, transform.position, ForceMode.Impulse); ;
                            //if (ss.remoteShip.gameObject.activeSelf)
                            //    ss.remoteShip.rb.AddForceAtPosition(impulse, transform.position, ForceMode.Impulse);
                            Debug.Log("hit a boat after jumping");
                            jumping = false;
                            pos.y = 0;
                            receivedPos.y = 0;
                            verticalSpeed = 0;
                        }
                    }
                    else
                    {
                        pos.y = 0;
                        receivedPos.y = 0;
                        verticalSpeed = 0;
                        jumping = false;
                    }
                }
                else
                { // On water or land

                }

                playArea.EnsureCircleInsideArea(ref receivedPos, collisionRadius);

                pos = Vector3.Lerp(pos, receivedPos, 5f * Time.deltaTime);
            }

            //////////////////
            // Actual moving
            //////////////////

            playArea.EnsureCircleInsideArea(ref pos, collisionRadius);

            transform.position = playArea.areaCenter.TransformPoint(pos) - Vector3.up * 0.15f;

            ////////////////
            // Water particles player
            ////////////////

            float waterLevel = Water.GetHeight(pos);
            bool underwater = waterLevel < pos.y;

            if (underwater && !lastFrameUnderwater && verticalSpeed < 0)
            { // Splash TODO

            }
            if (underwater != lastFrameUnderwater)
            { // update water swimming splash particles TODO

            }
            lastFrameUnderwater = underwater;
            //

            Vector3 deltaPos = pos - lastFramePosition;
            lastFramePosition = pos;

            Vector2 instantVelocity = new Vector2(deltaPos.x, deltaPos.z) / Time.deltaTime;
            instantVelocityAverage = instantVelocityAverage * 0.3f + instantVelocity * 0.7f;

            bool _interacting = interacting && interactable;

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

            float maxHandDistance = 0.2f;
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
                    wantedLeftHandPos = restLeftHand.position + transform.forward * 0.2f + transform.up * (0.6f + 0.4f * leftHandHoldStartFactor) - transform.right * 0.4f;
                    wantedRightHandPos = restRightHand.position + transform.forward * 0.2f + transform.up * (0.6f + 0.4f * -leftHandHoldStartFactor) + transform.right * 0.4f;
                }
            }

            interactingAnimationTime += Time.deltaTime;

            float interactingFinishFactor = Mathf.Clamp01(interactingAnimationTime / 1f);

            // TODO add inertia instead of lerping
            leftHand.position = Vector3.Lerp(leftHand.position, wantedLeftHandPos, interactingFinishFactor);
            rightHand.position = Vector3.Lerp(rightHand.position, wantedRightHandPos, interactingFinishFactor);

            // Feet
            feet.UpdateFeet(new Vector2(pos.x, pos.z), facingDirection);
        }
    }

    public void ApplyAcceleration(Vector3 acc)
    {
        Vector3 localAcceleration = playArea.areaCenter.InverseTransformDirection(acc);
        pos += Time.deltaTime * localAcceleration;
    }

    void GetPushedByOtherPlayers(ref Vector3 ownPosition)
    {
        Vector3 push = Vector3.zero;
        foreach (Photon.Realtime.Player p in RoomController.i.shipIdToPlayersCurrentlyBoarding[shipId])
        {
            if (p != photonView.Owner)
            {
                Vector3 otherPos = RoomController.i.playerSyncs[p].pos;

                float distance = Vector3.Distance(ownPosition, otherPos);
                if (distance < collisionRadius * 2f)
                {
                    float pushIntensity = 1 - (distance / (collisionRadius * 2f));
                    pushIntensity = Easing.Cubic.Out(pushIntensity);
                    Vector3 playerPush = (ownPosition - otherPos).normalized * pushIntensity;
                    push += playerPush;

                    //Debug.Log(string.Format("pushing! own {0}, other {1}, intensity {2}, playerPush {3}", ownPosition, otherPos, pushIntensity, playerPush));
                }
            }
        }

        //Debug.Log(push);
        ownPosition += push * pushSpeed * Time.deltaTime;
    }

    public void PlaceOnShip(ShipSync s, ShipPlayArea area, ShipInteractables shipInteractables)
    {
        ShipPlayArea previousArea = playArea;
        playArea = area;

        if (feet == null) feet = GetComponent<PlayerFeet>();
        feet.playArea = playArea;
        interactables = shipInteractables;//

        transform.SetParent(playArea.areaCenter);

        if (photonView.IsMine)
        {

            if (previousArea == null)
                pos = new Vector2(Random.Range(playArea.minMaxX.x, playArea.minMaxX.y), playArea.minMaxZ.x);
            else {
                pos = playArea.areaCenter.InverseTransformPoint(previousArea.areaCenter.TransformPoint(pos));
            }

            if (s != null)
            {
                Camera.main.GetComponent<OrbitCam>().SmoothLerpTo(s.visualShip.cameraFocusBottom, s.visualShip.cameraFocusTop, 0.7f);


                if (ShipUI.instance)
                {
                    ShipUI.instance.ship = s.visualShip;
                    ShipUI.instance.shipIdText.text = string.Format("Vessel #{0}", shipId + 1);
                }

                if (Music.instance)
                {
                    Music.instance.SetShip(s.visualShip);
                }

                s.shipSounds.doWavesAgainstShip = true;
            }
            else {
                // On land/water
                // TODO

                Camera.main.GetComponent<OrbitCam>().SmoothLerpTo(transform, null, 0.7f);

                if (ShipUI.instance)
                {
                    ShipUI.instance.ship = null;
                    ShipUI.instance.shipIdText.text = "";
                }
            }
        }

        feet.Init(pos, Vector2.up, photonView.IsMine? ownStepsMixerGroup : othersStepsMixerGroup);
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(shipId);
            stream.SendNext(pos);
            stream.SendNext(interacting);
            if (interacting)
            {
                stream.SendNext(interactable ? interactable.id : -1);
                stream.SendNext(interactingInput);
                stream.SendNext(rightHandHoldStartFactor);
                stream.SendNext(leftHandHoldStartFactor);
            }
            stream.SendNext(jumping);
            stream.SendNext(verticalSpeed);
        }
        else // if reading
        {
            int newShipId = (int)stream.ReceiveNext();
            receivedPos = (Vector3)stream.ReceiveNext();
            if (newShipId != shipId)
            {
                int oldSHipId = shipId;
                shipId = newShipId;

                pos = receivedPos;
                RoomController.i.RassignPlayerToShip(this, oldSHipId);
            }

            interacting = (bool)stream.ReceiveNext();
            if (interacting)
            {
                SetInteractableFromId((int)stream.ReceiveNext());
                interactingInput = (float)stream.ReceiveNext();
                rightHandHoldStartFactor = (float)stream.ReceiveNext();
                leftHandHoldStartFactor = (float)stream.ReceiveNext();
            }

            if (wasInteracting != interacting)
            {
                wasInteracting = interacting;
                interactingAnimationTime = 0f;

                if (interacting && interactable != null)
                    PlayStartInteractingSound(interactable);
            }

            bool wasJumping = jumping;
            jumping = (bool)stream.ReceiveNext();
            verticalSpeed = (float)stream.ReceiveNext();

            if (jumping && !wasJumping)
            { // let's catch up
                float deltaTime = (float)(PhotonNetwork.Time - info.SentServerTime);

                float deltaHeight = verticalSpeed * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
                receivedPos.y += deltaHeight;
                verticalSpeed = verticalSpeed + gravity * deltaTime;
            }
        }
        // Basically I'm hoping of placing all logic about world movement in a separate script
        worldMovement.OnPhotonSerializeView(stream);
    }

    public bool IsInteracting()
    {
        return interacting;
    }

    void SetInteractableFromId(int id)
    {
        if (id < 0)
        {
            if (interactable != null)
                PlayEndInteractingSound(interactable);
            interactable = null;
        }
        else if (id < interactables.interactables.Length)
            interactable = interactables.interactables[id];
        else
            Debug.LogError("Attempting to set an id that is out of range of interactables, you might be running a wrong version with a different number of interactables?");
    }

    public float InteractingInput()
    {
        return interactingInput;
    }

    void PlayStartInteractingSound(Interactable i)
    {
        if (i.GetType() == Interactable.Type.Rope)
        {
            ShipSync s = RoomController.i.ships[shipId];
            if (s != null)
            {
                s.shipSounds.PlaySoundAtPos(leftHand.position, holdSailSounds[Random.Range(0, holdSailSounds.Length)], 1f, photonView.IsMine ? ownGrabsMixerGroup : othersGrabsMixerGroup);
            }
        }
    }

    void PlayEndInteractingSound(Interactable i)
    {
        if (i.GetType() == Interactable.Type.Rope)
        {
            ShipSync s = RoomController.i.ships[shipId];
            if (s != null)
            {
                s.shipSounds.PlaySoundAtPos(leftHand.position, releaseSailSounds[Random.Range(0, releaseSailSounds.Length)], 1f, photonView.IsMine ? ownGrabsMixerGroup : othersGrabsMixerGroup);
            }
        }
    }

    [PunRPC]
    public void Miau(int which, float pitch)
    {
        if (mouth)
            mouth.PlayMiau(which, pitch);
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
