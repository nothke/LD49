using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Utils;

public class ShipLivery : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("bodyRenderers")]
    public Renderer[] hullRenderers;
    public Renderer[] sailRenderers;
    public Renderer[] mastRenderers;
    public CatColors colors;

    [HideInInspector]
    public Color red;
    [HideInInspector]
    public Color green;
    [HideInInspector]
    public Color blue;

    public Texture texture;
    public TMPro.TMP_Text printableTemplateText;

    public Vector2[] sailTextPositions;
    public float sailTextScale = 0.1f;

    public string textToWrite = "001";

    public void ApplyLivery(int colorCombination, int sail, int hull)
    {
        colorCombination = colorCombination % colors.liveryColorCombinations.Length; // in case
        sail = sail % colors.sailLiveries.Length; // in case
        hull = hull % colors.hullLiveryTextures.Length; // in case

        CatColors.LiveryCombination liveryColor = colors.liveryColorCombinations[colorCombination];

        AssignLivery(
            liveryColor.accent, liveryColor.baseColor, liveryColor.detail, 
            colors.hullLiveryTextures[hull], colors.sailLiveries[sail].sailTexture,
            colors.sailLiveries[sail].numberPosition,
            colors.sailLiveries[sail].numberColor == CatColors.Livery.ColorKind.black? Color.black :
            (colors.sailLiveries[sail].numberColor == CatColors.Livery.ColorKind.baseColor? Color.red :
            (colors.sailLiveries[sail].numberColor == CatColors.Livery.ColorKind.accent? Color.green : Color.blue)));
    }

    void AssignLivery(Color r, Color g, Color b, Texture2D hull, Texture2D sail, Vector3 numberPosition, Color numberColor)
    {
        red = r;
        green = g;
        blue = b;

        // Print text on the texture
        {
            Debug.Assert(printableTemplateText, "Template text not set", this);

            RenderTexture prevActive = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(sail.width, sail.height);
            RenderTexture.active = rt;
            Graphics.Blit(sail, rt);

            printableTemplateText.text = textToWrite;
            printableTemplateText.color = numberColor;
            printableTemplateText.ForceMeshUpdate(true, true);

            rt.BeginOrthoRendering();

            sailTextPositions[0] = numberPosition;
            numberPosition.x = 1 - numberPosition.x;
            sailTextPositions[1] = numberPosition;

            for (int i = 0; i < sailTextPositions.Length; i++)
            {
                rt.DrawTMPText(printableTemplateText, sailTextPositions[i], sailTextScale);
            }

            rt.EndRendering();

            texture = rt.ConvertToTexture2D();
            texture.name = "SAIL-clone";
            //Debug.Log("Set tex");

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = prevActive;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_ColorR", r);
        block.SetColor("_ColorG", g);
        block.SetColor("_ColorB", b);

        block.SetTexture("_MainTex", texture);
        foreach (Renderer rend in sailRenderers)
        {
            rend.SetPropertyBlock(block);
        }
        block.SetTexture("_MainTex", hull);
        foreach (Renderer rend in hullRenderers)
        {
            rend.SetPropertyBlock(block);
        }

        MaterialPropertyBlock mastBlock = new MaterialPropertyBlock();
        mastBlock.SetColor("_Color", Color.white);
        foreach (Renderer rend in mastRenderers)
        {
            rend.SetPropertyBlock(mastBlock);
        }
    }

    private void OnDestroy()
    {
        // The texture is an asset so it won't be automatically disposed
        if (texture)
            Destroy(texture);
    }
}
