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

        if (!shipSounds || !shipSounds.enabled) enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.other.name);
        if (shipSounds)
        {
            if (!shipSounds.enabled)
            {
                enabled = false;
                return;
            }
            shipSounds.ShipCollision(collision.contacts[0].point, collision.relativeVelocity.magnitude);
        }
    }
}
