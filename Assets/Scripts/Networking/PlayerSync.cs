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
    public float speed = 10f;
    public float pushSpeed = 5f;

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

                pos.x += inputX * speed * Time.deltaTime;
                pos.y += inputY * speed * Time.deltaTime;

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
        }
        else
        {
            receivedPos = (Vector2)stream.ReceiveNext();
        }
    }
}
