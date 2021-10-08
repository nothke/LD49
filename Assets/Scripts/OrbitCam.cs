using UnityEngine;

public class OrbitCam : MonoBehaviour
{
    public Transform target;
    public Transform target2;

    public float distance = 5.0f;
    public float turnSpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    public float distanceScrollSpeed = 5;

    float distanceInput;
    float smoothDistanceVelo;

    private Rigidbody cameraRigidBody;

    float x = 0.0f;
    float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        cameraRigidBody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (cameraRigidBody != null)
        {
            cameraRigidBody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    public void SetLerpByDistanceTargets(Transform t1, Transform t2)
    {
        target = t1;
        target2 = t2;
    }

    void UpdateCamera()
    {
        if (!target)
            return;

        const float pow = 3.0f;
        float maxInputDistance = Mathf.Pow(distanceMax, 1.0f / pow);
        float minInputDistance = Mathf.Pow(distanceMin, 1.0f / pow);

        const float FRAME_TIME = 1.0f / 60.0f;

        x += Input.GetAxis("Mouse X") * turnSpeed * FRAME_TIME;
        y -= Input.GetAxis("Mouse Y") * turnSpeed * FRAME_TIME;

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);


        float scroll = Input.GetAxis("Mouse ScrollWheel");


        distanceInput += -scroll * distanceScrollSpeed;
        distanceInput = Mathf.Clamp(distanceInput, minInputDistance, maxInputDistance);

        float distanceTarget = Mathf.Pow(distanceInput, pow);

        distance = Mathf.SmoothDamp(distance, distanceTarget, ref smoothDistanceVelo, 0.1f);

        Vector3 targetPos;
        if (!target2)
        {
            targetPos = target.position;
        }
        else
        {
            float t = Mathf.InverseLerp(distanceMin, distanceMax, distance);
            targetPos = Vector3.Lerp(target.position, target2.position, t);
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + targetPos;

        transform.rotation = rotation;
        transform.position = position;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
