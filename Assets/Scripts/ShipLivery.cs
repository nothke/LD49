using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLivery : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("bodyRenderers")]
    public Renderer[] hullRenderers;
    public Renderer[] sailRenderers;
    public CatColors colors;

    [HideInInspector]
    public Color red;
    [HideInInspector]
    public Color green;
    [HideInInspector]
    public Color blue;

    // Start is called before the first frame update

    public void ApplyLivery(int colorCombination, int sail, int hull)
    {
        colorCombination = colorCombination % colors.liveryColorCombinations.Length; // in case
        sail = sail % colors.sailLiveryTextures.Length; // in case
        hull = hull % colors.hullLiveryTextures.Length; // in case

        CatColors.LiveryCombination liveryColor = colors.liveryColorCombinations[colorCombination];
        AssignLivery(liveryColor.accent, liveryColor.baseColor, liveryColor.detail, colors.hullLiveryTextures[hull], colors.sailLiveryTextures[sail]);
    }

    void AssignLivery(Color r, Color g, Color b, Texture2D hull, Texture2D sail)
    {
        red = r;
        green = g;
        blue = b;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_ColorR", r);
        block.SetColor("_ColorG", g);
        block.SetColor("_ColorB", b);

        block.SetTexture("_MainTex", sail);
        foreach (Renderer rend in sailRenderers)
        {
            rend.SetPropertyBlock(block);
        }
        block.SetTexture("_MainTex", hull);
        foreach (Renderer rend in hullRenderers)
        {
            rend.SetPropertyBlock(block);
        }
    }
}
