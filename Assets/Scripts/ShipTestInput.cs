using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipTestInput : MonoBehaviour
{
    public ShipController _ship;
    ShipController ship { get { if (!_ship) _ship = GetComponent<ShipController>(); return _ship; } }

    void Update()
    {
        ship.inputX = Input.GetAxis("Horizontal");
        ship.inputY = 0;// Input.GetAxis("Vertical");
        ship.inputR = Input.GetAxis("Roll");

        ship.UpdateWithCurrentInput(Time.deltaTime);
    }
}
