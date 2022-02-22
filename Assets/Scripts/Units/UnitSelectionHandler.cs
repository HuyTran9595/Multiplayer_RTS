using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

//we don't need Network Behavior because the clients choose the unit
//then, the Clients signify to the server which unit to move
//we can move evrything at once in the server anyway
//there is a unit selection handler in the scene. It is unique in each client.

//This class only exists in client side, not stored on the server
public class UnitSelectionHandler : MonoBehaviour
{
    //which game obj layers can we hit?
    [SerializeField] private LayerMask layerMask = new LayerMask();


    private Camera mainCamera;
    public List<Unit> SelectedUnits { get; } = new List<Unit>();


    [SerializeField] RectTransform selectionBox = null;
    private Vector2 selectionBoxStartPosition; 
    private RTSPlayer player = null; //will get the player FROM the network
    

    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();


        if (selectionBox is null)
        {
            Debug.LogError("Missing selection box image in " + name);
        }


        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }


    private void AuthorityHandleUnitDespawned(Unit obj)
    {
        SelectedUnits.Remove(obj);
    }




    private void Update()
    {
        //try to get the player until get it
        if(player == null)
        {
            //identity = our player game object OF the connection
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }


        //when we first press left button = start selection
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        //when we release left button = no selection
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        //as long as we keep pressing, update selection box
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    /// <summary>
    /// Start a new selection box
    /// </summary>
    private void StartSelectionArea()
    {
        //if we are holding Shift, we add the selected units to the current list
        //So if we aren't, we clear the list

        if (!Keyboard.current.shiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }
        



        //now start a new one
        selectionBox.gameObject.SetActive(true);
        selectionBoxStartPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    /// <summary>
    /// update selection box
    /// </summary>
    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        //so now, we have the StartPosition and CurrentPosition of the box
        //now we make the box

        float width = mousePosition.x - selectionBoxStartPosition.x;
        float height = mousePosition.y - selectionBoxStartPosition.y;

        //Work out the math for this
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        //anchored position = the center of the rectangle
        selectionBox.anchoredPosition = selectionBoxStartPosition +
            new Vector2(width / 2, height / 2);


    }

    /// <summary>
    /// clear selection box
    /// </summary>
    private void ClearSelectionArea()
    {
        selectionBox.gameObject.SetActive(false);

        //the code below is for 1 single unit selection
        //still use them: when the player click and release in the same frame 
        // -> the box size is very small
        //we set the default value of box is width = height = 0 -> size = 0
        if(selectionBox.sizeDelta.magnitude == 0)
        {
            // just check for 1 unit
            //therefore, Ray
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            //if ray cast doesn't hit anything, return
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return;
            }

            //if we don't hit a unit, do nothing
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit))
            {
                return;
            }

            //now we have a Unit
            //if we don't have authority over that unit, do nothing
            if (!unit.hasAuthority)
            {
                return;
            }

            //now we have a unit, and it's our units
            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }

        //now we handle multi selecting

        //these are screen space
        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit))
            {
                continue;
            }
            
            //cast world space to screen space
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if(screenPosition.x > min.x && screenPosition.x < max.x &&
               screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
}
