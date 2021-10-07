using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public int id { get; set; } // index in interactables array, set automatically

    public Renderer[] highlightRenderers;

    public enum Type { NonSpecific, Rope, Wheel };
    public Type type { get; }

    public abstract Vector3 GetClosestPoint(Vector3 fromPosition, out float distance);

    public abstract float GetHandStartFactor();
    public abstract void GetHandStartFactors(out float leftHand, out float rightHand, float handStartFactor);

    public abstract Vector3 GetTargetBodyPosition(float leftHandStartFactor, float rightHandStartFactor);
    public abstract void GetHandPositions(out Vector3 leftHand, out Vector3 rightHand, float leftHandFactor, float rightHandFactor);

    public virtual void OnHighlighted() { }

    public virtual void OnStartedInteracting() { }
    public virtual void OnEndedInteracting() { }

    public void Highlight()
    {
        Facepunch.Highlight.ClearAll();

        foreach (var renderer in highlightRenderers)
        {
            Facepunch.Highlight.AddRenderer(renderer);
        }

        Facepunch.Highlight.Rebuild();

        OnHighlighted();
    }
}
