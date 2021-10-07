using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    public static ConnectionUI instance;
    void Awake()
    {
        instance = this;
    }

    public TMP_Text connectionInfo;
    public GameObject introPanel, ingamePanel, inRoomPanel;

    public ShipInfoButton shipInfoPrefab;
    public Transform shipInfoPanel;

    List<ShipInfoButton> shipInfos = new List<ShipInfoButton>();

    private void Update()
    {
        if (inRoomPanel.activeInHierarchy)
        {
            int count = 0;
            foreach (KeyValuePair<int, List<Photon.Realtime.Player>> kvp in RoomController.i.shipIdToPlayers)
            {
                ShipInfoButton inf = GetInfo(count);

                inf.SetInfo(kvp.Key, kvp.Value.Count, RoomController.i.maxPlayersPerShip);
                count++;
            }

            DisableInfosOver(count);
        }
    }

    ShipInfoButton GetInfo(int i)
    {
        if (i >= shipInfos.Count)
        {
            ShipInfoButton newInfo = Instantiate(shipInfoPrefab.gameObject, shipInfoPanel).GetComponent<ShipInfoButton>();
            newInfo.shipUi = this;
            shipInfos.Add(newInfo);
            return newInfo;
        }
        else
        {
            ShipInfoButton info = shipInfos[i];
            info.gameObject.SetActive(true);
            return info;
        }
    }

    void DisableInfosOver(int count)
    {
        for (int i = count; i < shipInfos.Count; ++i)
            shipInfos[i].gameObject.SetActive(false);
    }

    public void LogConnectionInfo(string str)
    {
        connectionInfo.text = string.Format("{0}\n{1}", str, connectionInfo.text);
    }

    public void ClearConnectionInfo()
    {
        connectionInfo.text = "";
    }

    public void ShowIntroPanel()
    {
        ClearConnectionInfo();

        introPanel.SetActive(true);
        inRoomPanel.SetActive(false);
        ingamePanel.SetActive(false);
    }

    public void ShowIngamePanel()
    {

        introPanel.SetActive(false);
        inRoomPanel.SetActive(false);
        ingamePanel.SetActive(true);
    }

    public void ShowInRoomPanel()
    {

        introPanel.SetActive(false);
        inRoomPanel.SetActive(true);
        ingamePanel.SetActive(false);
    }

    public void RandomShipPressed()
    {
        RoomController.i.JoinRandomShip();
        Debug.Log("Random ship pressed");
    }

    public void NewShipPressed()
    {
        RoomController.i.RequestAndJoinNewShip();
    }

    public void JoinVesselNumber(int id)
    {
        RoomController.i.JoinSpecificShip(id);
    }
}
