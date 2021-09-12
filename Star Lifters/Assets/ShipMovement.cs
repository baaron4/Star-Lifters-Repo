using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public float curAccel = 0f;
    public float changeInStageSpeed = 10f;
    public Rigidbody rb;
    private Vector3 inputForce;

    public void adjustAccel(int increaseAmount)
    {
        curAccel += increaseAmount;
    }
    public void adjustRotation(Quaternion rot)
    {

    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        inputForce = (transform.forward).normalized * curAccel;
        rb.velocity = Vector3.Lerp(rb.velocity, inputForce, changeInStageSpeed * Time.fixedDeltaTime);
       
    }
}
