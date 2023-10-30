using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    private MouseClickable lastObj;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        GetSelectedObj();
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            lastObj?.OnClick();
        }
    }

    private void GetSelectedObj() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            
            GameObject go = hit.collider.gameObject;

            MouseClickable currentObj;
            currentObj = go.GetComponent<Piece>();
            currentObj ??= go.GetComponent<NumberGenerator>();

            if (currentObj is not null && currentObj != lastObj) {
                lastObj?.OnHoverExit();

                lastObj = currentObj;
                lastObj.OnHover();

            } else if (currentObj is null && lastObj is not null) {
                lastObj.OnHoverExit();
                lastObj = null;
            }
        }        
    }
}
