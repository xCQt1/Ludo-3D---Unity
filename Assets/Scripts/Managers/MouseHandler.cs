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
    void Update()   // gets the selected object and determines, whether it is being clicked on
    {
        if (GameHandler.Instance.gameState != GameState.GAME) return;
        
        GetSelectedObj();
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            lastObj?.OnClick();
        }
    }

    private void GetSelectedObj() {     // determines currently with the cursor selected object
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {     // raycast from camera "through" cursor
            
            GameObject go = hit.collider.gameObject;

            MouseClickable currentObj;
            currentObj = go.GetComponent<Piece>();
            currentObj ??= go.GetComponent<NumberGenerator>();

            if (currentObj is not null && currentObj != lastObj) {  // case that a new object is selected and different from last one
                lastObj?.OnHoverExit();

                lastObj = currentObj;
                lastObj.OnHover();

            } else if (currentObj is null && lastObj is not null) { // if mouse unselected last object
                lastObj.OnHoverExit();
                lastObj = null;
            }
        }        
    }
}
