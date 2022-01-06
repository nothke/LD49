using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoundMixingDebugger : MonoBehaviour
{
    TMP_Text text;
    public UnityEngine.Audio.AudioMixer mixer;
    public string[] volumeNames;
    float[] volumes;

    KeyCode[] keyCodes = {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0,
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6,
        KeyCode.Keypad7,
        KeyCode.Keypad8,
        KeyCode.Keypad9,
        KeyCode.Keypad0
    };

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        text.text = "";

        volumes = new float[volumeNames.Length];

        for (int i = 0; i < volumeNames.Length; ++i)
        {
            if (mixer.GetFloat(volumeNames[i], out float v))
            {
                volumes[i] = v;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                PressedKey(i%10);
                return;
            }
        }

        if (currentlySoloing != -1)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                volumes[currentlySoloing] += 1f;
                mixer.SetFloat(volumeNames[currentlySoloing], volumes[currentlySoloing]);

                text.text = string.Format("[{0}] Soloing \"{1}\" {2} db", currentlySoloing, volumeNames[currentlySoloing], volumes[currentlySoloing]);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                volumes[currentlySoloing] -= 1f;
                mixer.SetFloat(volumeNames[currentlySoloing], volumes[currentlySoloing]);

                text.text = string.Format("[{0}] Soloing \"{1}\" {2} db", currentlySoloing, volumeNames[currentlySoloing], volumes[currentlySoloing]);
            }
        }
    }

    int currentlySoloing = -1;
    void PressedKey(int k)
    {
        if (currentlySoloing == k)
        {
            for (int i = 0; i < volumeNames.Length; ++i)
            {
                mixer.SetFloat(volumeNames[i], volumes[i]);
            }

            currentlySoloing = -1;
            text.text = "";
        }
        else {
            for (int i = 0; i < volumeNames.Length; ++i)
            {
                mixer.SetFloat(volumeNames[i], i == k? volumes[i] : -80f);
            }
            currentlySoloing = k;
            text.text = string.Format("[{0}] Soloing \"{1}\" {2} db", k, volumeNames[k], volumes[k]);
        }
    }
}
