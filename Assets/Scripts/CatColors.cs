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
}
