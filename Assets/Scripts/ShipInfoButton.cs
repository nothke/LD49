using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShipInfoButton : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text buttonText;
    public TMP_Text capacity;
    public Button button;
    public ConnectionUI shipUi;
    public LayoutElement layoutElement;

    int shipId;

    public void SetInfo(int shipId, int players, int maxPlayers)
    {
        this.shipId = shipId;

        title.text = string.Format("Vessel #{0}", shipId + 1);
        buttonText.text = string.Format("Join Vessel #{0}", shipId + 1);
        capacity.text = string.Format("{0} / {1} Players", players, maxPlayers);

        button.interactable = players < maxPlayers;
        //Debug.Log(players + " /" + maxPlayers);
    }

    public void ButtonPressed() {
        shipUi.JoinVesselNumber(shipId);
    }
}
