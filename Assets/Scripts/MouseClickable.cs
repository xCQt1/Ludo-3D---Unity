using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class MouseClickable : MonoBehaviour    // parent class for all clickable gameobjects (currently number generator and pieces)
{
    public Outline outline;

    protected void Awake() {
        outline = GetComponentInChildren<Outline>();
        if (outline is null) Debug.LogError("Outline ist null");
        outline.enabled = false;
    }

    private void Update() {
        if (outline.enabled) SetOutlineColor(DetermineColor());
    }
    
    public void OnHover() {     // called when cursor starts touching the object
        SetOutlineColor(DetermineColor());
        outline.enabled = true;
        OnHoverBegin();
    }

    public void OnHoverExit() {     // called when cursor ends touching the object
        outline.enabled = false;
        OnHoverStop();
    }

    public void SetOutlineColor(Color color) => outline.OutlineColor = color;

    abstract public void OnClick();     // when clicked on object
    abstract protected void OnHoverBegin();     // abstract class to be implemented in children for OnHover()
    abstract protected void OnHoverStop();      // abstract class for OnHoverExit()
    abstract protected Color DetermineColor();
}
