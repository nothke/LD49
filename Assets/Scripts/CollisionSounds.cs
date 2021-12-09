using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSounds : MonoBehaviour
{
    ShipSounds shipSounds;

    // Start is called before the first frame update
    void Start()
    {
        shipSounds = GetComponentInParent<ShipSounds>();

        //if (!shipSounds) enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.other.name);
        if (shipSounds)
            shipSounds.ShipCollision(collision.contacts[0].point, collision.relativeVelocity.magnitude);
    }
}
