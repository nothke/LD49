using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    public const string PLAYER_SHIP = "PLAYER_SHIP";
    public int shipId = -1;

    ShipPlayArea playArea;
    Vector2 pos = Vector2.zero;
    public float collisionRadius = 0.5f;
    public float speed = 1f;

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

                pos.x += inputX * speed * Time.deltaTime;
                pos.y += inputY * speed * Time.deltaTime;
            }

            playArea.EnsureCircleInsideArea(ref pos, collisionRadius);

            transform.position = playArea.TransformPoint(pos);
            transform.rotation = Quaternion.identity;
        }
    }

    public void PlaceOnShip(ShipSync s)
    {
        playArea = s.GetComponent<ShipPlayArea>();

        transform.SetParent(playArea.areaCenter);

        if (photonView.IsMine)
        {
            Camera.main.GetComponent<UnityGLTF.Examples.OrbitCameraController>().target = playArea.areaCenter.transform;
        }
    }


    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(pos);
        }
        else {
            pos = (Vector2)stream.ReceiveNext();
        }
    }
}
