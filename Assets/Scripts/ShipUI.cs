using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ShipUI : MonoBehaviour
{
    public ShipController ship;

    public TMP_Text speedText;

    void Update()
    {
        speedText.text = ((int)ship.SpeedKnots()).ToString();
    }
}
