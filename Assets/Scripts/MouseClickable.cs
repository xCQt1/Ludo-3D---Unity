using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class MouseClickable : MonoBehaviour
{
    public Outline outline;
    private Color outlineColor;

    protected void Awake() {
        outline = GetComponentInChildren<Outline>();
        if (outline is null) Debug.LogError("Outline ist null");
        outline.enabled = false;
    }

    private void Update() {
        if (outline.enabled) SetOutlineColor(DetermineColor());
    }
    
    public void OnHover() {
        SetOutlineColor(DetermineColor());
        outline.enabled = true;
        OnHoverBegin();
    }

    public void OnHoverExit() {
        outline.enabled = false;
        OnHoverStop();
    }

    public void SetOutlineColor(Color color) => outline.OutlineColor = color;

    abstract public void OnClick();
    abstract protected void OnHoverBegin();
    abstract protected void OnHoverStop();
    abstract protected Color DetermineColor();
}
