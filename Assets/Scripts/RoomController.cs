using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomController : MonoBehaviourPunCallbacks
{
    public int maxPlayersPerShip = 5;
    public float instantiationDistanceBetweenBoats = 15;
    public ShipSync shipPrefab;
    public PlayerSync playerPrefab;
    public CatColors catColors;

    public static RoomController i;

    public Dictionary<Player, PlayerSync> playerSyncs = new Dictionary<Player, PlayerSync>();
    public Dictionary<int, ShipSync> ships = new Dictionary<int, ShipSync>();
    public Dictionary<int, List<Player>> shipIdToPlayers = new Dictionary<int, List<Player>>();
    public Dictionary<Player, ShipSync> playerToShip = new Dictionary<Player, ShipSync>();

    private void Awake()
    {
        i = this;

        InvokeRepeating("ShipOwnershipPeriodicCheckup", 5f, 5f); // very much a patch
    }

    public override void OnJoinedRoom()
    {
        //photonView.RPC("RequestShipForPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log("Game version is " + PhotonNetwork.AppVersion);
    }

    public void JoinRandomShip()
    {
        photonView.RPC("RequestShipForPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, false);
    }

    public void JoinSpecificShip(int id)
    {

        if (ships.ContainsKey(id))
            PleaseJoinShip(id);
        else Debug.LogError(string.Format("Cannot join specific ship {0}, it does not exist!", id));
    }

    public void RequestAndJoinNewShip()
    {
        photonView.RPC("RequestShipForPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, true);
    }

    [PunRPC]
    void RequestShipForPlayer(int actorNumber, bool wantsNew)
    {
        bool foundShip = false;
        int shipId = -1;
        foreach (KeyValuePair<int, ShipSync> kvp in ships)
        {
            if (shipIdToPlayers[kvp.Key].Count < maxPlayersPerShip)
            {
                shipId = kvp.Key;
                foundShip = true;
                break;
            }
        }

        if (!foundShip || wantsNew)
        {
            shipId = InstantiateNewShip();
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == actorNumber)
            {
                photonView.RPC("PleaseJoinShip", p, shipId);
                return;
            }
        }
        Debug.LogError("Couldn't find player that requested for ship!", this);
    }

    // Only executed locally on the creating plauyer
    [PunRPC]
    void PleaseJoinShip(int shipId)
    {
        object[] playerInstantiationData = new object[5] {
            shipId,
            Random.Range(0, catColors.eyeColors.Length),
            Random.Range(0, catColors.furColors.Length),
            Random.Range(0, catColors.pantsColors.Length),
            Random.Range(0, catColors.jacketColors.Length)
        };
        PlayerSync newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0, playerInstantiationData).GetComponent<PlayerSync>();
        //newPlayer.shipId = shipId;

        ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
        ht[PlayerSync.PLAYER_SHIP] = shipId;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);

        ConnectionUI.instance.ShowIngamePanel();
    }

    int InstantiateNewShip() {
        int newShipId = 0;

        while (ships.ContainsKey(newShipId))
        {
            newShipId++;
        }

        object[] instantiationData = new object[1] { newShipId };
        Vector3 pos = Vector3.zero;
        pos = CorrectPositionToSpawnShip(pos);
        ShipSync newShip = PhotonNetwork.InstantiateRoomObject(shipPrefab.name, pos, Quaternion.identity, 0, instantiationData).GetComponent<ShipSync>();

        return newShipId;
    }

    Vector3 CorrectPositionToSpawnShip(Vector3 wantedPos)
    {
        Vector2 wp = new Vector2(wantedPos.x, wantedPos.z);
        foreach (KeyValuePair<int,ShipSync> kvp in ships)
        {
            Vector3 shipPos = kvp.Value.localShip.transform.position;
            if (Vector2.Distance(wp, new Vector2(shipPos.x, shipPos.z)) < instantiationDistanceBetweenBoats)
            {
                Debug.Log("Found to want to spawn ship ontop of eachother, moving back..");
                return CorrectPositionToSpawnShip(wantedPos - Vector3.forward * instantiationDistanceBetweenBoats - Vector3.right * instantiationDistanceBetweenBoats * 0.5f);
            }
        }

        return wantedPos;
    }
    public void RegisterShip(ShipSync s)
    {
        if (!ships.ContainsKey(s.shipId))
        {
            ships.Add(s.shipId, s);

            if (shipIdToPlayers.ContainsKey(s.shipId))
            {
                foreach (Player p in shipIdToPlayers[s.shipId])
                {
                    playerToShip.Add(p, s);
                    playerSyncs[p].PlaceOnShip(s);
                }
            }
            else shipIdToPlayers[s.shipId] = new List<Player>();
        }
        else {
            Debug.LogError(string.Format("Trying to register ship {0} that is already registered!", s.shipId), this);
        }
    }

    public void DeregisterShip(ShipSync s)
    {
        ships.Remove(s.shipId);
        shipIdToPlayers.Remove(s.shipId);
    }

    public void RegisterPlayer(PlayerSync p)
    {
        playerSyncs.Add(p.photonView.Owner, p);

        if (p.shipId == -1)
        {
            if (p.photonView.Owner.CustomProperties.TryGetValue(PlayerSync.PLAYER_SHIP, out object o))
            {
                p.shipId = (int)o;
            }
        }

        if (ships.ContainsKey(p.shipId))
        {
            p.PlaceOnShip(ships[p.shipId]);
            playerToShip.Add(p.photonView.Owner, ships[p.shipId]);
        }

        if (!shipIdToPlayers.ContainsKey(p.shipId))
            shipIdToPlayers.Add(p.shipId, new List<Player>());

        shipIdToPlayers[p.shipId].Add(p.photonView.Owner);

        if (shipIdToPlayers[p.shipId].Count == 1 && ships[p.shipId].photonView.IsMine && !p.photonView.IsMine)
        {
            ships[p.shipId].photonView.TransferOwnership(p.photonView.Owner);
        }
    }

    // Probably overcomplicated function that deals with disconnection of players and ship ownership
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("Player left room");
        if (playerSyncs.ContainsKey(otherPlayer))
        {
            //Debug.Log("playerSyncs had player");
            if (playerToShip.ContainsKey(otherPlayer))
            {
                //Debug.Log("playerToShip had player");
                ShipSync s = playerToShip[otherPlayer];
                shipIdToPlayers[s.shipId].Remove(otherPlayer);

                if (s.photonView.IsMine && playerToShip[PhotonNetwork.LocalPlayer] != s)
                { // Check for ownership transfer or for removal
                    if (shipIdToPlayers[s.shipId].Count == 0)
                    {// Empty ship

                        Debug.Log("Destroying empty ship");
                        PhotonNetwork.Destroy(s.gameObject);
                    }
                    else
                    { // Give the ship to a player on that ship
                        Debug.Log("Giving ownership to player driving ship");
                        s.photonView.TransferOwnership(shipIdToPlayers[s.shipId][0]);
                    }
                }

                playerToShip.Remove(otherPlayer);
            }

            // Double checking in case
            if (shipIdToPlayers[playerSyncs[otherPlayer].shipId].Contains(otherPlayer))
                shipIdToPlayers[playerSyncs[otherPlayer].shipId].Remove(otherPlayer);

            playerSyncs.Remove(otherPlayer);
        }
    }

    public void ShipOwnershipPeriodicCheckup()
    {
        if (!PhotonNetwork.InRoom) return;

        foreach (var kvp in ships)
        {
            ShipSync s = kvp.Value;
            if (!s.photonView.IsMine || shipIdToPlayers[s.shipId].Count == 0)
                continue;

            Player owner = s.photonView.Owner; // This is always going to be me because of prev condition

            bool ownedByPlayerDriving = false;
            foreach (var player in shipIdToPlayers[kvp.Value.shipId])
            {
                if (owner == player)
                {
                    ownedByPlayerDriving = true;
                    break;
                }
            }

            if (!ownedByPlayerDriving)
            {
                s.photonView.TransferOwnership(shipIdToPlayers[s.shipId][0]);
                Debug.Log("Giving ownership to player driving ship");
            }
        }
    }

    public override void OnLeftRoom()
    {
        playerSyncs.Clear();
        ships.Clear();
        shipIdToPlayers.Clear();
        playerToShip.Clear();
    }
}
