/*
    ███████╗██╗██████╗  ██████╗████████╗  ██████╗ ███████╗██████╗  ██████╗ █████╗ ███╗  ██╗
    ██╔════╝██║██╔══██╗██╔════╝╚══██╔══╝  ██╔══██╗██╔════╝██╔══██╗██╔════╝██╔══██╗████╗ ██║
    █████╗  ██║██████╔╝╚█████╗    ██║     ██████╔╝█████╗  ██████╔╝╚█████╗ ██║  ██║██╔██╗██║
    ██╔══╝  ██║██╔══██╗ ╚═══██╗   ██║     ██╔═══╝ ██╔══╝  ██╔══██╗ ╚═══██╗██║  ██║██║╚████║
    ██║     ██║██║  ██║██████╔╝   ██║     ██║     ███████╗██║  ██║██████╔╝╚█████╔╝██║ ╚███║
    ╚═╝     ╚═╝╚═╝  ╚═╝╚═════╝    ╚═╝     ╚═╝     ╚══════╝╚═╝  ╚═╝╚═════╝  ╚════╝ ╚═╝  ╚══╝

    ██████╗ ██╗      █████╗ ██╗   ██╗███████╗██████╗   ███╗   ███╗ █████╗ ██╗   ██╗███████╗███╗   ███╗███████╗███╗  ██╗████████╗
    ██╔══██╗██║     ██╔══██╗╚██╗ ██╔╝██╔════╝██╔══██╗  ████╗ ████║██╔══██╗██║   ██║██╔════╝████╗ ████║██╔════╝████╗ ██║╚══██╔══╝
    ██████╔╝██║     ███████║ ╚████╔╝ █████╗  ██████╔╝  ██╔████╔██║██║  ██║╚██╗ ██╔╝█████╗  ██╔████╔██║█████╗  ██╔██╗██║   ██║   
    ██╔═══╝ ██║     ██╔══██║  ╚██╔╝  ██╔══╝  ██╔══██╗  ██║╚██╔╝██║██║  ██║ ╚████╔╝ ██╔══╝  ██║╚██╔╝██║██╔══╝  ██║╚████║   ██║   
    ██║     ███████╗██║  ██║   ██║   ███████╗██║  ██║  ██║ ╚═╝ ██║╚█████╔╝  ╚██╔╝  ███████╗██║ ╚═╝ ██║███████╗██║ ╚███║   ██║   
    ╚═╝     ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝  ╚═╝     ╚═╝ ╚════╝    ╚═╝   ╚══════╝╚═╝     ╚═╝╚══════╝╚═╝  ╚══╝   ╚═╝   

    █▄▄ █▄█   ▀█▀ █ █ █▀▀   █▀▄ █▀▀ █ █ █▀▀ █   █▀█ █▀█ █▀▀ █▀█
    █▄█  █     █  █▀█ ██▄   █▄▀ ██▄ ▀▄▀ ██▄ █▄▄ █▄█ █▀▀ ██▄ █▀▄
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I use Physics.gravity a lot instead of Vector3.up because you can point the gravity to a different direction and i want the controller to work fine
[RequireComponent(typeof(Rigidbody))]
public class SFPSC_PlayerMovement : MonoBehaviour
{
    private static Vector3 vecZero = Vector3.zero;
    private Rigidbody rb;

    private bool enableMovement = true;

    [Header("Movement properties")]
    public int movementMode = 0; //0 walk, 1 ship control
    public float walkSpeed = 8.0f;
    public float runSpeed = 12.0f;
    public float changeInStageSpeed = 10.0f; // Lerp from walk to run and backwards speed
    public float maximumPlayerSpeed = 150.0f;
    [HideInInspector] public float vInput, hInput;
    public Transform groundChecker;
    public float groundCheckerDist = 0.2f;

    [Header("Jump")]
    public float jumpForce = 500.0f;
    public float jumpCooldown = 1.0f;
    private bool jumpBlocked = false;

    private SFPSC_WallRun wallRun;
    private SFPSC_GrapplingHook grapplingHook;

    [Header("Ship properties")]
    public GameObject onboardShip;
    private ShipMovement shipMovement;
    public float artificialGravity;
    private GameObject ControlledShip;
    public GameObject playerCamera;
    public float mouseX, mouseY;
    private float sensitivity;
    public float mousemoveX, mousemoveY;



    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        TryGetWallRun();
        TryGetGrapplingHook();

        if (onboardShip != null)
            BoardShip(onboardShip);

        sensitivity = playerCamera.GetComponent<SFPSC_FPSCamera>().sensitivity;
    }

    public void TryGetWallRun()
    {
        this.TryGetComponent<SFPSC_WallRun>(out wallRun);
    }

    public void TryGetGrapplingHook()
    {
        this.TryGetComponent<SFPSC_GrapplingHook>(out grapplingHook);
    }

    private bool isGrounded = false;
    public bool IsGrounded { get { return isGrounded; } }

    private Vector3 inputForce;
    private int i = 0;
    private float prevY;
    private void FixedUpdate()
    {
        if ((wallRun != null && wallRun.IsWallRunning) || (grapplingHook != null && grapplingHook.IsGrappling))
            isGrounded = false;
        else
        {
            // I recieved several messages that there are some bugs and I found out that the ground check is not working properly
            // so I made this one. It's faster and all it needs is the velocity of the rigidbody in two frames.
            // It works pretty well!
            isGrounded = (Mathf.Abs(rb.velocity.y - prevY) < .1f) && (Physics.OverlapSphere(groundChecker.position, groundCheckerDist).Length > 1); // > 1 because it also counts the player
            prevY = rb.velocity.y;
        }

        // Input
        vInput = Input.GetAxisRaw("Vertical");
        hInput = Input.GetAxisRaw("Horizontal");

        // Clamping speed
        rb.velocity = ClampMag(rb.velocity, maximumPlayerSpeed);

        if (enableMovement) { 
            //Ground Movment
            inputForce = (transform.forward * vInput + transform.right * hInput).normalized * (Input.GetKey(SFPSC_KeyManager.Run) ? runSpeed : walkSpeed);

            if (isGrounded)
            {
                // Jump
                if (Input.GetButton("Jump") && !jumpBlocked)
                {
                    rb.AddForce(-jumpForce * rb.mass * -this.transform.up);
                    jumpBlocked = true;
                    Invoke("UnblockJump", jumpCooldown);
                }
                // Ground controller
                rb.velocity = Vector3.Lerp(rb.velocity, inputForce, changeInStageSpeed * Time.fixedDeltaTime);
            }
            else
            {
                //Ship Gravity with air control
                Vector3 movmentVector = ClampSqrMag(rb.velocity + inputForce * Time.fixedDeltaTime, rb.velocity.sqrMagnitude) + (artificialGravity * Time.fixedDeltaTime* onboardShip.transform.up) ;
                rb.velocity = movmentVector;
            }
        }

        if (movementMode == 1 && !enableMovement)
        {
            //Ship Mode
            shipMovement.adjustAccel(vInput);

            // Mouse input
            mouseX = Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            //steer
            float rotX = 0;
            float rotY = 0;
            float rotZ = 0;
            if (Input.GetButtonDown("Jump") )
            {
                mousemoveX = 0;
                mousemoveY = 0;
            }
            if (Input.GetButton("Jump"))
            {
                mousemoveX += mouseX;
                mousemoveY += mouseY;

                rotY = mousemoveX;
                rotX = mousemoveY;
            }
            else
            {
                mousemoveX = 0;
                mousemoveY = 0;
            }
            
            rotZ = hInput;
            shipMovement.adjustRotation(Quaternion.Euler(rotX, rotY, rotZ));
        }
    }

    private static Vector3 ClampSqrMag(Vector3 vec, float sqrMag)
    {
        if (vec.sqrMagnitude > sqrMag)
            vec = vec.normalized * Mathf.Sqrt(sqrMag);
        return vec;
    }

    private static Vector3 ClampMag(Vector3 vec, float maxMag)
    {
        if (vec.sqrMagnitude > maxMag * maxMag)
            vec = vec.normalized * maxMag;
        return vec;
    }

    #region Previous Ground Check
    /*private void OnCollisionStay(Collision collision)
    {
        isGrounded = false;
        Debug.Log(collision.contactCount);
        for(int i = 0; i < collision.contactCount; ++i)
        {
            if (Vector3.Dot(Vector3.up, collision.contacts[i].normal) > .2f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }*/
    #endregion

    private void UnblockJump()
    {
        jumpBlocked = false;
    }
    
    
    // Enables jumping and player movement
    public void EnableMovement()
    {
        enableMovement = true;
    }

    // Disables jumping and player movement
    public void DisableMovement()
    {
        enableMovement = false;
    }

    public void ChangeMovementMode(int mode)
    {
        movementMode = mode;
        switch (mode)
        {
            case 0:
                //Walking
                EnableMovement();
                break;
            case 1:
                //Ship Control
                DisableMovement();
                break;
        }
        
    }

    public void BoardShip(GameObject ship)
    {
        onboardShip = ship;
        artificialGravity = ship.GetComponent<Ship_Controller>().artificialGravity;
        this.transform.SetParent(ship.transform);
        playerCamera.transform.SetParent(ship.transform);
    }

    public void LeaveShip(GameObject ship)
    {
        onboardShip = null;
        artificialGravity = 0f;
    }

    public void AssignShip(GameObject ship)
    {
        ControlledShip = ship;
        shipMovement = ship.GetComponent<Ship_Controller>().shipExterior.GetComponent<ShipMovement>();
    }

    public void RemoveShip()
    {
        ControlledShip = null;
        shipMovement = null;
    }
}
