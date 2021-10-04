using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ShipUI : MonoBehaviour
{
    public static ShipUI instance;
    void Awake() {
        instance = this;
    }

    public ShipController ship;

    public TMP_Text speedText;
    public TMP_Text shipIdText;

    public TMP_Text interactionText;

    public TMP_Text connectionInfo;

    public GameObject introPanel, ingamePanel, inRoomPanel;

    void Update()
    {
        if (!ship)
            speedText.text = "-";
        else
            speedText.text = ((int)ship.SpeedKnots()).ToString();
    }

    public void SetInteractionText(string str)
    {
        interactionText.text = str;
    }

    public void LogConnectionInfo(string str)
    {
        connectionInfo.text = string.Format("{0}\n{1}", str, connectionInfo.text);
    }

    public void ClearConnectionInfo() {
        connectionInfo.text = "";
    }

    public void ShowIntroPanel() {
        ClearConnectionInfo();

        introPanel.SetActive(true);
        inRoomPanel.SetActive(false);
        ingamePanel.SetActive(false);
    }

    public void ShowIngamePanel() {

        introPanel.SetActive(false);
        inRoomPanel.SetActive(false);
        ingamePanel.SetActive(true);
    }

    public void ShowInRoomPanel() {

        introPanel.SetActive(false);
        inRoomPanel.SetActive(true);
        ingamePanel.SetActive(false);
    }

    public void RandomShipPressed() {
        
    }

    public void NewShipPressed()
    {

    }
    public void JoinVesselNumber(int id)
    {

    }
}
