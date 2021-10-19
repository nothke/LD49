using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColors : MonoBehaviour
{
    public CatColors colors;

    public MeshRenderer eyeRenderer;
    public SkinnedMeshRenderer bodyRenderer;

    public int eyeMaterialIndex, furMaterialIndex, pantsMaterialIndex, jacketMaterialIndex;

    public void SetColors(int eyeColorIt, int furColorIt, int pantsColorIt, int jacketColorIt)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        block.SetColor("_Color", colors.eyeColors[eyeColorIt]);
        eyeRenderer.SetPropertyBlock(block, eyeMaterialIndex);

        block.SetColor("_Color", colors.furColors[furColorIt]);
        bodyRenderer.SetPropertyBlock(block, furMaterialIndex);

        block.SetColor("_Color", colors.pantsColors[pantsColorIt]);
        bodyRenderer.SetPropertyBlock(block, pantsMaterialIndex);

        block.SetColor("_Color", colors.jacketColors[jacketColorIt]);
        bodyRenderer.SetPropertyBlock(block, jacketMaterialIndex);
    }
}
