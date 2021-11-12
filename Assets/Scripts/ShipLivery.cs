using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLivery : MonoBehaviour
{
    public Renderer[] liveryRenderers;
    public CatColors colors;

    [HideInInspector]
    public Color red;
    [HideInInspector]
    public Color green;
    [HideInInspector]
    public Color blue;

    // Start is called before the first frame update

    public void ApplyLivery(int which)
    {
        int w = which % colors.liveries.Length; // in case
        CatColors.LiveryCombination livery = colors.liveries[w];
        ApplyColors(livery.accent, livery.baseColor, livery.detail);
    }

    void ApplyColors(Color r, Color g, Color b)
    {
        red = r;
        green = g;
        blue = b;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_ColorR", r);
        block.SetColor("_ColorG", g);
        block.SetColor("_ColorB", b);
        foreach (Renderer rend in liveryRenderers)
        {
            rend.SetPropertyBlock(block);
        }
    }
}
