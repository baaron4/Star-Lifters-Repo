using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public float curAccel = 0f;
    public float changeInStageSpeed = 10f;
    public bool invertVDirection = true;
    public Rigidbody rb;
    private Vector3 inputForce;
    public Transform shipInterior;
    public Quaternion steeredDirection;

    public void adjustAccel(float increaseAmount)
    {
        if(invertVDirection)
            curAccel += -increaseAmount;
        else 
            curAccel += increaseAmount;
    }
    public void adjustRotation(Quaternion rot)
    {
        //TODO clamp direction amount
        steeredDirection = rot;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Thrust
        inputForce = (transform.forward).normalized * curAccel;
        rb.velocity = Vector3.Lerp(rb.velocity, inputForce, changeInStageSpeed * Time.fixedDeltaTime);

        //Steering
        this.transform.localRotation =  steeredDirection * this.transform.localRotation;
        //Lock Interior to exterior
        shipInterior.transform.position = this.transform.position + 5* Vector3.up;
        shipInterior.transform.rotation = this.transform.rotation;

    }
}
