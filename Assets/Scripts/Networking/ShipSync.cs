using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShipSync : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    public int shipId = -1;
    public int shipLiveryColorCombination = -1;
    public int shipLiveryBodyTexture = -1;
    public int shipLiverySailTexture = -1;

    public ShipController shipPrefab;
    public int physicsLayer = 6;
    ShipInputCalculator shipInput;

    [Header("Debug")]
    public bool visualizeRemoteShip = true;
    public Material remoteMaterial;
    List<Renderer> remoteRenderers = new List<Renderer>();

    public ShipController localShip, remoteShip;
    [HideInInspector]
    public ShipSounds shipSounds;

    public ShipController visualShip
    {
        get { return localShip; }
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        shipId = (int)instantiationData[0];
        shipLiveryColorCombination = (int)instantiationData[1];
        shipLiverySailTexture = (int)instantiationData[2];
        shipLiveryBodyTexture = (int)instantiationData[3];

        name = "NetworkedShip " + shipId;

        Vector3 spawnPos = transform.position;
        spawnPos.y = shipPrefab.transform.position.y;

        // Setup remote ship simulation.
        localShip = Instantiate(shipPrefab.gameObject, spawnPos, shipPrefab.transform.rotation).GetComponent<ShipController>();
        remoteShip = Instantiate(shipPrefab.gameObject, spawnPos, shipPrefab.transform.rotation).GetComponent<ShipController>();

        localShip.name = "Catamaran " + shipId;
        remoteShip.name = "Catamaran " + shipId + "(Physics only)";

        PrepareRemoteShip(remoteShip.gameObject.transform, physicsLayer, remoteMaterial);

        remoteShip.gameObject.SetActive(!photonView.IsMine);

        shipInput = GetComponent<ShipInputCalculator>();

        var livery = localShip.gameObject.GetComponent<ShipLivery>();
        livery.printableTemplateText = RoomController.i.printableTextTemplate;
        livery.textToWrite = (shipId + 1).ToString().PadLeft(3, '0');
        livery.ApplyLivery(shipLiveryColorCombination, shipLiverySailTexture, shipLiveryBodyTexture);

        shipSounds = localShip.GetComponent<ShipSounds>();
        remoteShip.GetComponent<ShipSounds>().enabled = false;
        shipSounds.doWavesAgainstShip = false;
        shipSounds.Init();

        WorldShipInteractable si = GetComponent<WorldShipInteractable>();
        WorldInteractable wi = localShip.GetComponent<WorldInteractable>();
        si.shipPlayArea = wi.shipPlayArea;
        si.highlightRenderers = wi.highlightRenderers;

        //

        RoomController.i.RegisterShip(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            shipInput.UpdateInput(shipId, ref localShip.inputX, ref localShip.inputY, ref localShip.inputR);
            localShip.UpdateWithCurrentInput(Time.deltaTime);

            if (remoteShip.gameObject.activeInHierarchy)
            {
                remoteShip.gameObject.SetActive(false);
            }
        }
        else
        {
            networkSmoothingTime = Mathf.Max(0f, networkSmoothingTime - Time.deltaTime);
            float smoothFactor = 1f - networkSmoothingTime / NETWORK_SMOOTH_TIME;

            if (smoothFactor == 1f)
            {
                // localShip should represent closely the actual state of the ship, no lerping required
                if (remoteShip.gameObject.activeInHierarchy)
                {
                    remoteShip.gameObject.SetActive(false);

                    //localShip.rb.MovePosition(remoteShip.rb.position);
                    //localShip.rb.MoveRotation(remoteShip.rb.rotation);
                    //localShip.rb.velocity = remoteShip.rb.velocity;
                    //localShip.rb.angularVelocity = remoteShip.rb.angularVelocity;
                }

                shipInput.UpdateInput(shipId, ref localShip.inputX, ref localShip.inputY, ref localShip.inputR);
                localShip.UpdateWithCurrentInput(Time.deltaTime);
                stillFrames++;
            }
            else
            {
                smoothFrames++;


                localShip.mastAngle = Mathf.Lerp(localShip.mastAngle, remoteShip.mastAngle, Time.deltaTime * 4f);
                localShip.rudderAngle = Mathf.Lerp(localShip.rudderAngle, remoteShip.rudderAngle, Time.deltaTime * 4f);

                shipInput.UpdateInput(shipId, ref localShip.inputX, ref localShip.inputY, ref localShip.inputR);
                localShip.UpdateWithCurrentInput(Time.deltaTime);

                remoteShip.inputX = localShip.inputX;
                remoteShip.inputY = localShip.inputY;
                remoteShip.inputR = localShip.inputR;
                //shipInput.UpdateInput(shipId, ref remoteShip.inputX, ref remoteShip.inputY, ref remoteShip.inputR);
                remoteShip.UpdateWithCurrentInput(Time.deltaTime);

                //localShip.inputX = Mathf.Lerp(localShip.inputX, remoteShip.inputX, smoothFactor);
                //localShip.inputY = Mathf.Lerp(localShip.inputY, remoteShip.inputY, smoothFactor);
                //localShip.inputR = Mathf.Lerp(localShip.inputR, remoteShip.inputR, smoothFactor);

                // Here we would want to simulate the movement of both local and remote, and then visually show an inbetween state.
                // But we are just hoping the physics will simulate that for us and we lerp the local to the remote instead.

            }

            // Debug remote viz
            if (remoteRenderers[0].enabled != visualizeRemoteShip)
            {
                foreach (Renderer r in remoteRenderers)
                    r.enabled = visualizeRemoteShip;
            }
        }

        transform.position = localShip.transform.position;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            float smoothFactor = 1f - networkSmoothingTime / NETWORK_SMOOTH_TIME;

            if (smoothFactor < 1f)
            {
                localShip.rb.MovePosition(Vector3.Lerp(localShip.rb.position, remoteShip.rb.position, smoothFactor));
                localShip.rb.MoveRotation(Quaternion.Lerp(localShip.rb.rotation, remoteShip.rb.rotation, smoothFactor));
                localShip.rb.velocity = Vector3.Lerp(localShip.rb.velocity, remoteShip.rb.velocity, smoothFactor);
                localShip.rb.angularVelocity = Vector3.Lerp(localShip.rb.angularVelocity, remoteShip.rb.angularVelocity, smoothFactor);
            }
        }

        // Apply player weight to boat
        foreach (Photon.Realtime.Player p in RoomController.i.shipIdToPlayersCurrentlyBoarding[shipId])
        {
            PlayerSync ps = RoomController.i.playerSyncs[p];

            if (ps.IsJumping)
                continue;

            Vector3 playerPosition = ps.transform.position;
            // Make sure the position we apply the force is within bounds (otherwise it will tilt a lot)
            Vector3 localPlayerPos = localShip.playAreaManager.areaCenter.InverseTransformPoint(playerPosition);
            localShip.playAreaManager.EnsureCircleInsideArea(ref localPlayerPos, ps.collisionRadius);
            playerPosition = localShip.playAreaManager.areaCenter.TransformPoint(localPlayerPos);

            localShip.rb.AddForceAtPosition(Vector3.down * ps.boatPushWeight, playerPosition, ForceMode.Acceleration);

            if (remoteShip.gameObject.activeSelf)
                remoteShip.rb.AddForceAtPosition(Vector3.down * ps.boatPushWeight, playerPosition, ForceMode.Acceleration);
        }

        // Pushing forces?
        foreach (Photon.Realtime.Player p in RoomController.i.shipIdToPlayersCurrentlyBoarding[-1])
        { // For each player in the world
            PlayerSync ps = RoomController.i.playerSyncs[p];

            if (!ps.IsGrownded)
                continue;

            Vector3 playerPosition = ps.transform.position + Vector3.up;
            // Make sure the position we apply the force is within bounds (otherwise it will tilt a lot)
            Vector3 localPlayerPos = localShip.playAreaManager.areaCenter.InverseTransformPoint(playerPosition);
            localShip.playAreaManager.ClosestInsidePoint(ref localPlayerPos, Vector3.zero, new Vector2(-1f, 3f));
            Vector3 forcePosition = localShip.playAreaManager.areaCenter.TransformPoint(localPlayerPos);

            Vector3 forceDirection = forcePosition - playerPosition;
            float sqrMagnitude = forceDirection.sqrMagnitude;
            float maxPushDistanceSqr = 1.5f * 1.5f;
            if (sqrMagnitude < maxPushDistanceSqr)
            { // Close to the ship, so push

                forceDirection.y = 0;

                if (forceDirection.magnitude < 0.0001)
                {
                    forceDirection = localShip.playAreaManager.areaCenter.position - playerPosition;
                    forceDirection.y = 0;
                    if (forceDirection.magnitude < 0.0001)
                    {
                        forceDirection = Vector3.up;
                    }
                }

                Vector3 pushForce = forceDirection.normalized * (1f - (sqrMagnitude / maxPushDistanceSqr)) * 3.7f; //* ps.boatPushWeight
                localShip.rb.AddForceAtPosition(pushForce, forcePosition, ForceMode.Acceleration);

                ps.ApplyAcceleration(-pushForce);

                //Debug.Log("Pushing boat from world force:" + pushForce);
                if (remoteShip.gameObject.activeSelf)
                   remoteShip.rb.AddForceAtPosition(pushForce, forcePosition, ForceMode.Acceleration);
            }


        }
    }

    Vector3 receivedPosition;
    Quaternion receivedRotation;
    Vector3 receivedVelocity;
    Vector3 receivedAngularVelocity;

    private const float NETWORK_SMOOTH_TIME = 0.2f;
    float networkSmoothingTime = 0;
    int smoothFrames = 0;
    int stillFrames = 0;

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(localShip.inputX);
            stream.SendNext(localShip.inputY);
            stream.SendNext(localShip.inputR);
            stream.SendNext(localShip.mastAngle);
            stream.SendNext(localShip.rudderAngle);

            stream.SendNext(localShip.rb.position);
            stream.SendNext(localShip.rb.rotation);
            stream.SendNext(localShip.rb.velocity);
            stream.SendNext(localShip.rb.angularVelocity); // Maybe unnecessary?

            // These are to prevent big jumps in case of ownership change:
            receivedPosition = localShip.rb.position;
            receivedRotation = localShip.rb.rotation;
            receivedVelocity = localShip.rb.velocity;
            receivedAngularVelocity = localShip.rb.angularVelocity;

            remoteShip.rb.MovePosition(receivedPosition);
            remoteShip.rb.MoveRotation(receivedRotation);
        }
        else
        {
            // TODO lerping/softening of input, position and velocity
            remoteShip.inputX = (float)stream.ReceiveNext();
            remoteShip.inputY = (float)stream.ReceiveNext();
            remoteShip.inputR = (float)stream.ReceiveNext();
            remoteShip.mastAngle = (float)stream.ReceiveNext();
            remoteShip.rudderAngle = (float)stream.ReceiveNext();

            receivedPosition = (Vector3)stream.ReceiveNext();
            receivedRotation = (Quaternion)stream.ReceiveNext();
            receivedVelocity = (Vector3)stream.ReceiveNext();
            receivedAngularVelocity = (Vector3)stream.ReceiveNext();


            double msSinceSent = PhotonNetwork.ServerTimestamp - info.SentServerTimestamp;
            float deltaTime = (float)(msSinceSent / 1000d);

            deltaTime = Mathf.Min(1f, deltaTime);

            // Perform move calculations with received pos and deltatime of the packet

            receivedPosition += receivedVelocity * deltaTime;
            receivedRotation = receivedRotation * Quaternion.Euler(receivedAngularVelocity * Mathf.Rad2Deg * deltaTime * 0.7f); // mb unecessary, the 0.7 is just to pretend to drag

            remoteShip.rb.MovePosition(receivedPosition);
            remoteShip.rb.MoveRotation(receivedRotation);
            remoteShip.rb.velocity = receivedVelocity;
            remoteShip.rb.angularVelocity = receivedAngularVelocity;

            shipInput.UpdateInput(shipId, ref remoteShip.inputX, ref remoteShip.inputY, ref remoteShip.inputR);
            remoteShip.UpdateWithCurrentInput(deltaTime);

            remoteShip.gameObject.SetActive(true);


            networkSmoothingTime = NETWORK_SMOOTH_TIME;


            //Debug.Log(smoothFrames + " " + stillFrames);
            smoothFrames = 0;
            stillFrames = 0;
        }
    }

    void PrepareRemoteShip(Transform t, int layer, Material m)
    {
        t.gameObject.layer = layer;

        ParticleSystem p = t.GetComponent<ParticleSystem>();
        if (p != null)
        {
            t.gameObject.SetActive(false);
        }
        else
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = m;
                remoteRenderers.Add(r);
                if (!visualizeRemoteShip)
                    r.enabled = false;
            }
        }

        foreach (Transform c in t)
        {
            PrepareRemoteShip(c, layer, m);
        }
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            if (localShip != null) Destroy(localShip.gameObject);
            if (remoteShip != null) Destroy(remoteShip.gameObject);

            RoomController.i.DeregisterShip(this);
        }
    }
}
