using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    private MouseClickable _lastObj;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()   // gets the selected object and determines, whether it is being clicked on
    {
        if (GameHandler.Instance.GameState != GameState.GAME) return;
        
        GetSelectedObj();
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            _lastObj?.OnClick();
        }
    }

    private void GetSelectedObj() {     // determines currently with the cursor selected object
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {     // raycast from camera "through" cursor
            
            GameObject go = hit.collider.gameObject;

            MouseClickable currentObj;
            currentObj = go.GetComponent<Piece>();
            currentObj ??= go.GetComponent<NumberGenerator>();

            if (currentObj is not null && currentObj != _lastObj) {  // case that a new object is selected and different from last one
                _lastObj?.OnHoverExit();

                _lastObj = currentObj;
                _lastObj.OnHover();

            } else if (currentObj is null && _lastObj is not null) { // if mouse unselected last object
                _lastObj.OnHoverExit();
                _lastObj = null;
            }
        }        
    }
}
