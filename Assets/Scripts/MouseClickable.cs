using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class MouseClickable : MonoBehaviour
{
    public Outline outline;
    public Color outlineColor;

    protected void Awake() {
        outline = GetComponentInChildren<Outline>();
        outline.enabled = false;
        if (outline is null) Debug.LogError("Outline ist null");
    }

    private void Update() {
        if (outline.enabled) SetOutlineColor(DetermineColor());
    }
    
    public void OnHover() {
        SetOutlineColor(DetermineColor());
        outline.enabled = true;
    }

    public void OnHoverExit() {
        outline.enabled = false;
    }

    public void SetOutlineColor(Color color) => outline.OutlineColor = color;

    abstract public void OnClick();
    abstract protected Color DetermineColor();
}
