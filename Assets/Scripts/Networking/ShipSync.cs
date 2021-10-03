using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShipSync : MonoBehaviourPun, IPunObservable
{

    public ShipController shipPrefab;
    public int physicsLayer = 6;
    ShipInputCalculator shipInput;

    [Header("Debug")]
    public Material remoteMaterial;

    ShipController localShip, remoteShip;

    // Start is called before the first frame update
    void Start()
    {
        // Setup remote ship simulation.
        // Ideally we would want 2 simulations running, the local and the remote, and lerp the actual values from one to the other
        // In this case I'm trying to lerp the local towards the remote without keeping track of the "untouched/unlerped" local values
        // This might look a bit worse but might be simpler and enough

        localShip = shipPrefab;
        remoteShip = Instantiate(localShip.gameObject).GetComponent<ShipController>();

        SetLayerRecursively(remoteShip.gameObject.transform, physicsLayer, remoteMaterial);

        remoteShip.gameObject.SetActive(!photonView.IsMine);

        shipInput = GetComponent<ShipInputCalculator>();
    }

    // Update is called once per frame
    void Update()
    {
        // calculate input locally based on input and other players
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        float inputR = Input.GetAxis("Roll");


        if (photonView.IsMine)
        {
            shipInput.UpdateInput(ref localShip.inputX, ref localShip.inputY, ref localShip.inputR);

            if (remoteShip.gameObject.activeInHierarchy)
            {
                remoteShip.gameObject.SetActive(false);
            }
        }
        else {

            networkSmoothingTime = Mathf.Max(0f, networkSmoothingTime - Time.deltaTime);
            float smoothFactor = 1f - networkSmoothingTime / NETWORK_SMOOTH_TIME;

            if (smoothFactor == 1f)
            {
                // localShip should represent closely the actual state of the ship, no lerping required
                if (remoteShip.gameObject.activeInHierarchy)
                {
                    remoteShip.gameObject.SetActive(false);
                }

                shipInput.UpdateInput(ref localShip.inputX, ref localShip.inputY, ref localShip.inputR);
            }
            else
            {
                shipInput.UpdateInput(ref localShip.inputX, ref localShip.inputY, ref localShip.inputR);
                shipInput.UpdateInput(ref remoteShip.inputX, ref remoteShip.inputY, ref remoteShip.inputR);

                localShip.inputX = Mathf.Lerp(localShip.inputX, remoteShip.inputX, smoothFactor);
                localShip.inputY = Mathf.Lerp(localShip.inputY, remoteShip.inputY, smoothFactor);
                localShip.inputR = Mathf.Lerp(localShip.inputR, remoteShip.inputR, smoothFactor);

                // Here we would want to simulate the movement of both local and remote, and then visually show an inbetween state.
                // But we are just hoping the physics has simulated that for us and we lerp the local to the remote instead.

                localShip.rb.MovePosition(Vector3.Lerp(localShip.rb.position, remoteShip.rb.position, smoothFactor));
                localShip.rb.MoveRotation(Quaternion.Lerp(localShip.rb.rotation, remoteShip.rb.rotation, smoothFactor));
                localShip.rb.velocity = Vector3.Lerp(localShip.rb.velocity, remoteShip.rb.velocity, smoothFactor);
                localShip.rb.angularVelocity = Vector3.Lerp(localShip.rb.angularVelocity, remoteShip.rb.angularVelocity, smoothFactor);
                localShip.mastAngle = Mathf.Lerp(localShip.mastAngle, remoteShip.mastAngle, smoothFactor);
                localShip.rudderAngle = Mathf.Lerp(localShip.rudderAngle, remoteShip.rudderAngle, smoothFactor);
            }
        }
    }

    Vector3 receivedPosition;
    Quaternion receivedRotation;
    Vector3 receivedVelocity;
    Vector3 receivedAngularVelocity;

    private const float NETWORK_SMOOTH_TIME = 0.3f;
    float networkSmoothingTime = 0;

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

            remoteShip.gameObject.SetActive(true);


            networkSmoothingTime = NETWORK_SMOOTH_TIME;
        }
    }

    void SetLayerRecursively(Transform t, int layer, Material m)
    {
        t.gameObject.layer = layer;
        MeshRenderer r = GetComponent<MeshRenderer>();
        if (r != null) r.material = m;

        foreach (Transform c in t)
        {
            SetLayerRecursively(c, layer, m);
        }
    }
}
