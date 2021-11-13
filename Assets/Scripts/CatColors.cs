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

    [UnityEngine.Serialization.FormerlySerializedAs("liveries")]
    public LiveryCombination[] liveryColorCombinations;

    [System.Serializable]
    public class Livery
    {
        public string name;
        public Texture2D sailTexture;
        public Vector2 numberPosition = new Vector2(0.15f, 0.4f);
    }

    public Livery[] sailLiveries;

    //public Texture2D[] sailLiveryTextures;
    [UnityEngine.Serialization.FormerlySerializedAs("bodyLiveryTextures")]
    public Texture2D[] hullLiveryTextures;
}
