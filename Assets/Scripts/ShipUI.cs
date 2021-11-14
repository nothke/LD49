using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ShipUI : MonoBehaviour
{
    public static ShipUI instance;
    void Awake()
    {
        instance = this;
    }

    public ShipController ship;

    public Gradient accelerationGradient;
    public float acceleartionScale = 0.1f;
    public Slider accelerationSlider;

    public TMP_Text speedText;
    public TMP_Text shipIdText;

    public TMP_Text interactionText;

    public Slider steeringWheelSlider;
    public RectTransform steeringRudder;

    float prevSpeed;
    float acceleration;
    float smoothAccelerationVelo;

    [Header("Remove this")]
    public TMP_Text connectionInfo;
    public GameObject introPanel, ingamePanel, inRoomPanel;
    public ShipInfoButton shipInfoPrefab;
    public Transform shipInfoPanel;

    private void FixedUpdate()
    {
        if (ship)
        {
            float speed = ship.SpeedKnots();
            float accelerationTarget = (speed - prevSpeed) / Time.deltaTime * acceleartionScale;
            acceleration = Mathf.SmoothDamp(acceleration, accelerationTarget, ref smoothAccelerationVelo, 0.1f);

            prevSpeed = speed;
        }
    }

    void Update()
    {
        if (!ship)
        {
            speedText.text = "-";
            speedText.color = Color.white;
        }
        else
        {
            float speed = ship.SpeedKnots();

            speedText.text = ((int)speed).ToString();
            speedText.color = accelerationGradient.Evaluate((acceleration) + 0.5f);

            if (accelerationSlider)
                accelerationSlider.value = acceleration * 3.0f;
        }

        if (steeringWheelSlider.gameObject.activeInHierarchy)
        {
            steeringWheelSlider.value = -ship.RudderAngleNormalized;
            steeringRudder.rotation = Quaternion.AngleAxis(-ship.rudderAngle, Vector3.forward);
        }
    }

    public void SetInteractionText(string str)
    {
        interactionText.text = str;
    }

    public void EnableWheelSlider(bool b)
    {
        steeringWheelSlider.gameObject.SetActive(b);
    }
}
