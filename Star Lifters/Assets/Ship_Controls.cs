using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Controls : MonoBehaviour
{
    public bool inUse = false;
    public Interactable intScript;
    public GameObject player;
    public Transform drivePosition;
    public GameObject ship;

    private void Start()
    {
       intScript =  this.GetComponentInChildren<Interactable>();
    }

    private void OnMouseEnter()
    {
        intScript.isFocused = true;
    }
    private void OnMouseExit()
    {
        intScript.isFocused = false;
    }

    public void ToggleControl()
    {
        if (!inUse)
        {
            inUse = true;
            Debug.Log("Player is taking control.");
            player.transform.position = drivePosition.position;
            player.GetComponent<SFPSC_PlayerMovement>().ChangeMovementMode(1);
            player.GetComponent<SFPSC_PlayerMovement>().AssignShip(ship);
        }
        else 
        if (inUse)
        {
            inUse = false;
            Debug.Log("Player is leaving control..");
            
            player.GetComponent<SFPSC_PlayerMovement>().ChangeMovementMode(0);
            player.GetComponent<SFPSC_PlayerMovement>().RemoveShip();
        }
    }
}
