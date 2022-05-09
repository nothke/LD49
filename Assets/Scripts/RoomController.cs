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
    [UnityEngine.Serialization.FormerlySerializedAs("catColors")]
    public CatColors colors;
    public WorldPlayArea world;

    public static RoomController i;

    public TMPro.TMP_Text printableTextTemplate;

    public Dictionary<Player, PlayerSync> playerSyncs = new Dictionary<Player, PlayerSync>();
    public Dictionary<int, ShipSync> ships = new Dictionary<int, ShipSync>();
    public Dictionary<int, List<Player>> shipIdToOriginalPlayers = new Dictionary<int, List<Player>>();
    public Dictionary<Player, ShipSync> playerToShip = new Dictionary<Player, ShipSync>();
    public Dictionary<int, List<Player>> shipIdToBoardedPlayers = new Dictionary<int, List<Player>>();

    [Header("Sounds")]
    public AudioClip joinedOwnShip;
    public AudioClip joinedOtherShip, playerLeft;
    public UnityEngine.Audio.AudioMixerGroup playerJoinedMixer;

    private void Awake()
    {
        i = this;

        liveryColorUsage = new int[colors.liveryColorCombinations.Length];
        liverySailUsage = new int[colors.sailLiveries.Length];
        liveryHullUsage = new int[colors.hullLiveryTextures.Length];
        //for (int i = 0; i < liveryUsage.Length; ++i)
        //    liveryUsage[i] = 0;
        shipIdToBoardedPlayers[-1] = new List<Player>();

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
            if (shipIdToBoardedPlayers[kvp.Key].Count < maxPlayersPerShip)
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
        Vector2 playerVoice = MouthSounds.GetRandomPitchRange();
        object[] playerInstantiationData = new object[7] {
            shipId,
            Random.Range(0, colors.eyeColors.Length),
            Random.Range(0, colors.furColors.Length),
            Random.Range(0, colors.pantsColors.Length),
            Random.Range(0, colors.jacketColors.Length),
            playerVoice.x,
            playerVoice.y
        };
        PlayerSync newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0, playerInstantiationData).GetComponent<PlayerSync>();
        newPlayer.SetShipIdLocally(shipId);

        //ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
        //ht[PlayerSync.PLAYER_SHIP] = shipId;
        //PhotonNetwork.LocalPlayer.SetCustomProperties(ht);

        ConnectionUI.instance.ShowIngamePanel();
        //Music.instance.FadeOut();
    }

    int InstantiateNewShip() {
        int newShipId = 0;

        while (ships.ContainsKey(newShipId))
        {
            newShipId++;
        }

        GetNewLivery(out int colorCombination, out int sail, out int hull);
        object[] instantiationData = new object[4] { newShipId, colorCombination, sail, hull };
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

    void GetNewLivery(out int col, out int sail, out int hull)
    {
        col = GetRandomLowestUsage(liveryColorUsage);
        sail = GetRandomLowestUsage(liverySailUsage);
        hull = GetRandomLowestUsage(liveryHullUsage);
    }

    int GetRandomLowestUsage(int[] usageArray)
    {
        // In usage array, value is amount of uses, iterator is the wanted returned value
        List<int> options = new List<int>(usageArray.Length);

        int leastUsed = 100000; // pretend it's INT.MAX

        for (int i = 0; i < usageArray.Length; ++i)
        {
            if (usageArray[i] < leastUsed)
            {
                options.Clear();
                options.Add(i);
                leastUsed = usageArray[i];
            }
            else if (usageArray[i] == leastUsed)
                options.Add(i);
        }

        return options[Random.Range(0, options.Count)];
    }

    int[] liveryColorUsage;
    int[] liverySailUsage;
    int[] liveryHullUsage;
    public void RegisterShip(ShipSync s)
    {
        Debug.Log($"Registering ship in ship ${s.shipId}");
        if (!ships.ContainsKey(s.shipId))
        {
            ships.Add(s.shipId, s);

            if (shipIdToBoardedPlayers.ContainsKey(s.shipId))
            {
                foreach (Player p in shipIdToBoardedPlayers[s.shipId])
                {
                    RassignPlayerToShip(playerSyncs[p], s.shipId);
                }
            }
            else
            {
                shipIdToOriginalPlayers[s.shipId] = new List<Player>();
                shipIdToBoardedPlayers[s.shipId] = new List<Player>();
            }

            liveryColorUsage[s.shipLiveryColorCombination]++;
            liverySailUsage[s.shipLiverySailTexture]++;
            liveryHullUsage[s.shipLiveryBodyTexture]++;
        }
        else {
            Debug.LogError(string.Format("Trying to register ship {0} that is already registered!", s.shipId), this);
        }
    }

    public void DeregisterShip(ShipSync s)
    {
        ships.Remove(s.shipId);
        shipIdToOriginalPlayers.Remove(s.shipId);
        shipIdToBoardedPlayers.Remove(s.shipId);
        liveryColorUsage[s.shipLiveryColorCombination]--;
        liverySailUsage[s.shipLiverySailTexture]--;
        liveryHullUsage[s.shipLiveryBodyTexture]--;
    }

    public void RegisterPlayer(PlayerSync p, int originalShipId)
    {
        Debug.Log($"Registering player in ship {p.shipId}, originally from ship {originalShipId}");
        playerSyncs.Add(p.photonView.Owner, p);

        if (!shipIdToOriginalPlayers.ContainsKey(originalShipId))
            shipIdToOriginalPlayers.Add(originalShipId, new List<Player>());

        shipIdToOriginalPlayers[originalShipId].Add(p.photonView.Owner);

        // We do this to all players first
        shipIdToBoardedPlayers[-1].Add(p.photonView.Owner);

        if (ships.ContainsKey(p.shipId))
        {
            RassignPlayerToShip(p, -1);
        }
    }

    public void RassignPlayerToShip(PlayerSync ps, int fromShip)
    {
        Debug.Log($"Reassigning player to ship {ps.shipId}, from ship {fromShip}");
        Player p = ps.photonView.Owner;

        // Removals & Cleanup
        if (!shipIdToBoardedPlayers.ContainsKey(fromShip))
            Debug.LogWarning($"Reassigning player from ship ${fromShip}, but that ship was not present!", ps);
        else shipIdToBoardedPlayers[fromShip].Remove(p);

        if (playerToShip.ContainsKey(p))
        {
            playerToShip.Remove(p);
        }

        // Assignments

        if (ps.shipId != -1 && ships.ContainsKey(ps.shipId))
        {
            ShipSync s = ships[ps.shipId];
            playerToShip.Add(p, s);
            ps.PlaceOnShip(s, s.visualShip.GetComponent<ShipPlayArea>());
            if (!shipIdToBoardedPlayers[ps.shipId].Contains(p))
                shipIdToBoardedPlayers[ps.shipId].Add(p);

            bool localBoat = playerToShip.ContainsKey(PhotonNetwork.LocalPlayer) && playerToShip[PhotonNetwork.LocalPlayer] == s;
            s.shipSounds.PlaySoundAtPos(s.visualShip.transform.position, localBoat ? joinedOwnShip : joinedOtherShip, 1f, playerJoinedMixer, 128, 10f);

            if (shipIdToBoardedPlayers[ps.shipId].Count == 1 && ships[ps.shipId].photonView.IsMine && !ps.photonView.IsMine)
            {
                ships[ps.shipId].photonView.TransferOwnership(ps.photonView.Owner);
            }
        }
        else {
            // Ship not there yet?
            if (!shipIdToBoardedPlayers.ContainsKey(ps.shipId))
                shipIdToBoardedPlayers.Add(ps.shipId, new List<Player>());

            shipIdToBoardedPlayers[ps.shipId].Add(p);
        }
    }

    // Probably overcomplicated function that deals with disconnection of players and ship ownership
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log("Player left room");
        if (playerSyncs.ContainsKey(otherPlayer))
        {
            if (playerToShip.ContainsKey(otherPlayer))
            {
                ShipSync s = playerToShip[otherPlayer];
                if (s != null)
                {
                    shipIdToBoardedPlayers[s.shipId].Remove(otherPlayer);

                    if (playerToShip.ContainsKey(PhotonNetwork.LocalPlayer))
                        playerToShip[PhotonNetwork.LocalPlayer].shipSounds.PlaySoundAtPos(s.visualShip.transform.position, playerLeft, 1f, playerJoinedMixer, 132, 10f);

                    if (s.photonView.IsMine && (!playerToShip.ContainsKey(PhotonNetwork.LocalPlayer) || playerToShip[PhotonNetwork.LocalPlayer] != s))
                    { // Check for ownership transfer or for removal
                        if (shipIdToBoardedPlayers[s.shipId].Count == 0)
                        {// Empty ship

                            Debug.Log($"Ship {s.shipId} now empty");
                        }
                        else
                        { // Give the ship to a player on that ship
                            Debug.Log("Giving ownership to player driving ship");
                            s.photonView.TransferOwnership(shipIdToBoardedPlayers[s.shipId][0]);
                        }
                    }
                }

                playerToShip.Remove(otherPlayer);
            }

            // Double checking in case
            if (shipIdToBoardedPlayers[playerSyncs[otherPlayer].shipId].Contains(otherPlayer))
                shipIdToBoardedPlayers[playerSyncs[otherPlayer].shipId].Remove(otherPlayer);


            int originalShip = playerSyncs[otherPlayer].originalShipId;
            shipIdToOriginalPlayers[originalShip].Remove(otherPlayer);

            if (shipIdToOriginalPlayers[originalShip].Count == 0 && originalShip != -1 && ships.ContainsKey(originalShip))
            {// forgotten ship
                ShipSync s = ships[originalShip];
                if (s.photonView.IsMine)
                {
                    Debug.Log("Destroying empty ship");
                    PhotonNetwork.Destroy(s.gameObject);
                }
            }

            playerSyncs.Remove(otherPlayer);
        }
    }

    public void ShipOwnershipPeriodicCheckup()
    {
        if (!PhotonNetwork.InRoom) return;

        foreach (var kvp in ships)
        {
            ShipSync s = kvp.Value;
            if (!s.photonView.IsMine)
                continue;

            Player owner = s.photonView.Owner; // This is always going to be me because of prev condition

            if (shipIdToBoardedPlayers[s.shipId].Count == 0 && !owner.IsMasterClient)
            {
                Debug.Log("Giving ownership to master client");
                s.photonView.TransferOwnership(PhotonNetwork.MasterClient);
                continue;
            }


            bool ownedByPlayerDriving = false;
            foreach (var player in shipIdToBoardedPlayers[kvp.Value.shipId])
            {
                if (owner == player)
                {
                    ownedByPlayerDriving = true;
                    break;
                }
            }

            if (!ownedByPlayerDriving && shipIdToBoardedPlayers[s.shipId].Count > 0)
            {
                Debug.Log("Giving ownership to player driving ship");
                s.photonView.TransferOwnership(shipIdToBoardedPlayers[s.shipId][0]);
            }
        }
    }

    public override void OnLeftRoom()
    {
        playerSyncs.Clear();
        ships.Clear();
        shipIdToOriginalPlayers.Clear();
        playerToShip.Clear();
        shipIdToBoardedPlayers.Clear();
    }
}
