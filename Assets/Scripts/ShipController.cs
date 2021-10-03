using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShipController : MonoBehaviourPun, IPunObservable
{
    Rigidbody _rb;
    Rigidbody rb { get { if (!_rb) _rb = GetComponent<Rigidbody>(); return _rb; } }

    public float torqueMult = 1;
    public float forceMult = 1;

    public Transform[] rudders;
    float maxRudderSteeringAngle = 30;

    public Transform mast;

    float inputX;
    float inputY;
    float inputR;
    float mastAngle;

    void Update()
    {
        if (photonView.IsMine) {
            // TODO calculate input locally based on inputs of other players
            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");
            inputR = Input.GetAxis("Roll");
        }

        mastAngle += -inputR;
        mastAngle = Mathf.Clamp(mastAngle, -90, 90);

        foreach (var rudder in rudders)
        {
            rudder.localRotation = Quaternion.Euler(0, -inputX * maxRudderSteeringAngle + 90, 0);
        }

        mast.localRotation = Quaternion.Euler(0, mastAngle, 0);
    }

    private void FixedUpdate()
    {
        //rb.AddRelativeTorque(Vector3.up * inputX * torqueMult, ForceMode.Acceleration);
        rb.AddRelativeForce(Vector3.forward * inputY * forceMult, ForceMode.Acceleration);
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
            stream.SendNext(inputX);
            stream.SendNext(inputY);
            stream.SendNext(inputR);

            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.velocity);
            stream.SendNext(rb.angularVelocity); // Maybe unnecessary?
        }
        else {
            // TODO lerping/softening of input, position and velocity
            inputX = (float)stream.ReceiveNext();
            inputY = (float)stream.ReceiveNext();
            inputR = (float)stream.ReceiveNext();

            receivedPosition = (Vector3)stream.ReceiveNext();
            receivedRotation = (Quaternion)stream.ReceiveNext();
            receivedVelocity = (Vector3)stream.ReceiveNext();
            receivedAngularVelocity = (Vector3)stream.ReceiveNext();


            double msSinceSent = PhotonNetwork.ServerTimestamp - info.SentServerTimestamp;
            float deltaTime = (float)(msSinceSent / 1000d);

            deltaTime = Mathf.Min(1f, deltaTime);

            // Perform move calculations with received pos and deltatime of the packet


            networkSmoothingTime = NETWORK_SMOOTH_TIME;
        }
    }
}
