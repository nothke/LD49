using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

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

    public TMP_Text speedText;
    public TMP_Text shipIdText;

    public TMP_Text interactionText;

    public UnityEngine.UI.Slider steeringWheelSlider;

    float prevSpeed;
    float acceleration;

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
            acceleration = (speed - prevSpeed) / Time.deltaTime * acceleartionScale;
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
        }

        if (steeringWheelSlider.gameObject.activeInHierarchy)
        {
            steeringWheelSlider.value = -ship.RudderAngleNormalized;
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
