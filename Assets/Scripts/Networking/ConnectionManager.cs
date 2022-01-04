using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public static ConnectionManager i;

    public bool wantOwnRoom = false;
    public string gameVersion = "0.1";
    public string roomName = "Adriatic";

    public bool kickAFKPlayers = true;
    public float maxAFKtime = 90f;
    float lastActiveTime = 0f;
    bool kickedForInactivity = false;
    Coroutine retryConnectionCoroutine = null;

    const string START_ROOM_TIME_KEY = "START_TIME";

    public static double roomCreatedTime = 0;

    public AudioSource successfulLoginSound;

    void Start()
    {
        roomCreatedTime = 0;
        i = this;
        lastActiveTime = Time.time;

#if !UNITY_EDITOR
        wantOwnRoom = false;
#endif

        Connect();
    }

    void Connect()
    {
        ConnectionUI.instance.ShowIntroPanel();
        ConnectionUI.instance.LogConnectionInfo(string.Format("Game Version {0}, connecting to server..", gameVersion));
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        if (kickAFKPlayers && PhotonNetwork.InRoom)
        {
            if (Application.isFocused)
                lastActiveTime = Time.time;
            else
            {
                if (Time.time - lastActiveTime > maxAFKtime)
                {
                    kickedForInactivity = true;
                    PhotonNetwork.Disconnect();
                    lastActiveTime = Time.time;
                }
            }
        }

        if (!PhotonNetwork.InRoom && (Input.GetMouseButtonDown(0) || Input.GetAxis("Submit") > 0) && retryConnectionCoroutine == null && Application.isFocused)
        {
            retryConnectionCoroutine = StartCoroutine(WaitABitAndTryToConnectAgain());
        }
    }


    #region photoncallbacks
    public override void OnConnectedToMaster()
    {
        ConnectionUI.instance.LogConnectionInfo(string.Format("Connected to server, fetching rooms.."));
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        ConnectionUI.instance.LogConnectionInfo(string.Format("Joining room.."));
        if (!wantOwnRoom)
            PhotonNetwork.JoinRandomRoom();
        else
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 20 });
    }

    public override void OnJoinedRoom()
    {
        ConnectionUI.instance.LogConnectionInfo(string.Format("Connected to room!"));
        Debug.Log("Connected to room");

        ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
        ht["FOCUS"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);


        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(START_ROOM_TIME_KEY, out object o))
        {
            roomCreatedTime = (double)o;
        }
        else if (roomCreatedTime == 0) {
            Debug.LogError("No room creation time found on joined room!, you better be the one who created the room..");
        }

        ConnectionUI.instance.ShowInRoomPanel();
        successfulLoginSound.Play();
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (returnCode == Photon.Realtime.ErrorCode.GameFull ||
            returnCode == Photon.Realtime.ErrorCode.ServerFull)
        {
            Debug.LogError(string.Format("Failed to join random room, server seems full :(, code: {0}, message: {1}", returnCode, message));
            ConnectionUI.instance.LogConnectionInfo(string.Format("Failed to join random room, server seems full :(, code: {0}, message: {1}", returnCode, message));

            if (retryConnectionCoroutine == null)
            {
                retryConnectionCoroutine = StartCoroutine(WaitABitAndTryToConnectAgain());
            }
        }
        else
        {
            ConnectionUI.instance.LogConnectionInfo(string.Format("Nobody seems online, creating room.."));
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 20 });
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {

        if (returnCode == Photon.Realtime.ErrorCode.GameFull ||
            returnCode == Photon.Realtime.ErrorCode.ServerFull)
        {
            Debug.LogError(string.Format("Failed to create room, server seems full :(, code: {0}, message: {1}", returnCode, message));
            ConnectionUI.instance.LogConnectionInfo(string.Format("Failed to create room, server seems full :(, code: {0}, message: {1}", returnCode, message));
        }
        else
        {
            Debug.LogError(string.Format("Failed to create room, code: {0}, message: {1}", returnCode, message));
            ConnectionUI.instance.LogConnectionInfo(string.Format("Failed to create room, code: {0}, message: {1}", returnCode, message));

            if (retryConnectionCoroutine == null)
            {
                retryConnectionCoroutine = StartCoroutine(WaitABitAndTryToConnectAgain());
            }
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room at time "+PhotonNetwork.Time);
        ConnectionUI.instance.LogConnectionInfo(string.Format("Created room!"));

        ExitGames.Client.Photon.Hashtable rt = new ExitGames.Client.Photon.Hashtable();
        rt[START_ROOM_TIME_KEY] = roomCreatedTime = PhotonNetwork.Time;
        PhotonNetwork.CurrentRoom.SetCustomProperties(rt);
    }

    IEnumerator WaitABitAndTryToConnectAgain()
    {
        Debug.Log("Retrying connection in 2 seconds..");
        ConnectionUI.instance.LogConnectionInfo("Retrying connection in 2 seconds..");

        yield return new WaitForSecondsRealtime(2f);

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                OnConnectedToMaster();
            }
        }
        else Connect();

        retryConnectionCoroutine = null;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ConnectionUI.instance.ClearConnectionInfo();
        if (kickedForInactivity)
        {
            Debug.LogWarning("Kicked for inactivity");

            ConnectionUI.instance.ShowIntroPanel();
            ConnectionUI.instance.LogConnectionInfo(string.Format("Will try to reconnect on return\nKicked for inactivity after {0} seconds AFK", maxAFKtime));
            kickedForInactivity = false;
        }
        else if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.LogError(string.Format("Disconnected! cause: {0}, reconnecting..", cause));
            ConnectionUI.instance.LogConnectionInfo(string.Format("Disconnected! cause: {0}, reconnecting..", cause));

            if (retryConnectionCoroutine == null)
            {
                retryConnectionCoroutine = StartCoroutine(WaitABitAndTryToConnectAgain());
            }
        }
    }

    #endregion photoncallbacks

#if !UNITY_EDITOR
    void OnApplicationFocus(bool focus)
    {
        ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
        ht["FOCUS"] = focus;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ht);

        if (!focus) // If we are not in focus, change master client for better game performance for other players
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length > 1)
            {
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (p != PhotonNetwork.LocalPlayer)
                    {
                        object o;
                        if (p.CustomProperties.TryGetValue("FOCUS", out o))
                        {
                            bool f = (bool)o;

                            if (f)
                            {
                                Debug.Log("Found better masterclient");
                                PhotonNetwork.SetMasterClient(p);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (!Application.isFocused && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length > 1)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p != PhotonNetwork.LocalPlayer)
                {
                    object o;
                    if (p.CustomProperties.TryGetValue("FOCUS", out o))
                    {
                        bool f = (bool)o;

                        if (f)
                        {
                            Debug.Log("Found better masterclient");
                            PhotonNetwork.SetMasterClient(p);
                        }
                    }
                }
            }
        }
    }
#endif
}
