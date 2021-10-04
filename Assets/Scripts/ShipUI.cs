using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ShipUI : MonoBehaviour
{
    public static ShipUI instance;
    void Awake() { instance = this; }

    public ShipController ship;

    public TMP_Text speedText;

    public TMP_Text interactionText;

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
}
