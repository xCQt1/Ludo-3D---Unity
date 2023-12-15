using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class MouseClickable : MonoBehaviour    // parent class for all clickable gameobjects (currently number generator and pieces)
{
    [HideInInspector] public Outline Outline;

    protected void Awake() {
        Outline = GetComponentInChildren<Outline>();
        if (Outline is null) Debug.LogError("Outline ist null");
        Outline.enabled = false;
    }

    private void Update() {
        if (Outline.enabled) SetOutlineColor(DetermineColor());
    }
    
    public void OnHover() {     // called when cursor starts touching the object
        SetOutlineColor(DetermineColor());
        Outline.enabled = true;
        OnHoverBegin();
    }

    public void OnHoverExit() {     // called when cursor ends touching the object
        Outline.enabled = false;
        OnHoverStop();
    }

    public void SetOutlineColor(Color color) => Outline.OutlineColor = color;

    abstract public void OnClick();     // when clicked on object
    abstract protected void OnHoverBegin();     // abstract class to be implemented in children for OnHover()
    abstract protected void OnHoverStop();      // abstract class for OnHoverExit()
    abstract protected Color DetermineColor();
}
