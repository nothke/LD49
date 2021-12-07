using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSounds : MonoBehaviour
{
    ShipController ship;

    public AudioSource stationary, frontMoving, backMoving;
    [Range(0, 1)]
    public float frontBackDirectionalFactor = 0.7f;

    public float maxExpectedKnots = 20f;
    public float maxMovingVolume = 2f;

    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponent<ShipController>();

        frontMoving.volume = 0f;
        backMoving.volume = 0f;
        stationary.volume = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        float shipSpeed = ship.SpeedKnots();


        float movingFactor = Mathf.Clamp01(shipSpeed / maxExpectedKnots);

        float frontDot = -Vector3.Dot(Camera.main.transform.forward, frontMoving.transform.forward);

        //Debug.Log(frontDot);

        stationary.volume = (1f - movingFactor);
        frontMoving.volume = movingFactor * (1f - frontBackDirectionalFactor + frontBackDirectionalFactor * Mathf.Max(0, frontDot));
        backMoving.volume = movingFactor * (1f - frontBackDirectionalFactor + frontBackDirectionalFactor * Mathf.Max(0, -frontDot));

        
    }


}
