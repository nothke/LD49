using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobblyBodyPart : MonoBehaviour
{
    public Transform toFollow;
    Vector3 lastPosition;

    public float drag = 3f;
    public float maxDistance = 0.5f;
    public float rotAngle = 90f;

    public float overDistanceElasticAcceleration = 2f;
    public float rotationReturnFactor = 5f;

    Vector3 currentPos;

    Vector3 currentVelocity;
    Quaternion currentRotation;



    void Start()
    {
        lastPosition = toFollow.position;
        currentPos = transform.position;
    }

    private void Update()
    {

        Vector3 instantVel = (toFollow.position - lastPosition)/ Time.deltaTime;

        lastPosition = toFollow.position;

        UpdateVisuals(instantVel);
    }

    public void UpdateVisuals(Vector3 instantVelocity)
    {
        instantVelocity.z = instantVelocity.y;
        instantVelocity.y = 0;

        currentVelocity = Vector3.Lerp(currentVelocity, instantVelocity, Time.deltaTime * drag);

        currentPos += currentVelocity * Time.deltaTime;

        Vector3 delta = currentPos - transform.parent.position;
        float distance = delta.magnitude;
        delta.Normalize();
        float deltaFactor = distance / maxDistance;
        Vector3 localDelta = transform.parent.InverseTransformDirection(delta);
        Quaternion wantedRot = Quaternion.Euler(localDelta.z * deltaFactor * rotAngle, 0f, -localDelta.x * deltaFactor * rotAngle);

        if (distance > maxDistance)
        {

            currentVelocity += -delta * overDistanceElasticAcceleration * Time.deltaTime;

            currentRotation = Quaternion.Lerp(currentRotation, wantedRot, Time.deltaTime * overDistanceElasticAcceleration);

            currentPos = transform.parent.position + delta * maxDistance;
        }
        else
        {
            currentRotation = Quaternion.Lerp(currentRotation, wantedRot, Time.deltaTime * rotationReturnFactor);
        }

        currentVelocity -= delta * overDistanceElasticAcceleration * Time.deltaTime * deltaFactor;

        transform.position = currentPos;
        transform.localRotation = currentRotation;
    }
}
