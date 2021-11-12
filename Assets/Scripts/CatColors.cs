using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CatColors", menuName = "ScriptableObjects/Cat Colors", order = 1)]
public class CatColors : ScriptableObject
{
    public Color[] eyeColors;
    public Color[] furColors;
    public Color[] pantsColors;
    public Color[] jacketColors;

    [System.Serializable]
    public class LiveryCombination {
        public Color accent = Color.red;
        public Color baseColor = Color.green;
        public Color detail = Color.blue;
    }

    public LiveryCombination[] liveries;
}
