using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerLive : MonoBehaviour
{
    public float speed = 50f;
    public float maxSpeed = 15f;
    public float jumpForce = 10f;
    public float jumpBoost = .3f;
    public bool grounded;
    public bool jumping;
    public bool coyote;
    public bool jumpBuffered;
    private Rigidbody rbody;
    public Camera camera;
    private Animator anim;
    private Collider col;
    public LayerMask groundLayer; // Layer mask for the ground objects
    private Vector3 groundCheckSize; // Size of the ground check box
    private float coyoteTimer;
    public float coyoteTimeThreshhold;
    public float bufferTimeThreshhold;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        //If using Capsule collider
        // halfH = col.bounds.extents.y + 0.03f;
        //If using box collider
        groundCheckSize = new Vector3(0.7f, .05f, 0.7f);
        jumpBuffered = false;
    }

    // Update is called once per frame
    void Update()
    {
        var horizontalMovement = HorizontalMovement();
        GroundCheck();
        Jump();
        BodyRotation(horizontalMovement);
        animate();
        moveCamera();
    }

    private void BodyRotation(float horizontalMovement)
    {
        //Slow down Mario
        if (Math.Abs(horizontalMovement)<0.5f)
        {
            // rbody.velocity *= Math.Abs(horizontalMovement);
            Vector3 newV = rbody.velocity;
            newV.x *= 1f - Time.deltaTime;
            rbody.velocity = newV;
        }
        //Set model rotation
        float yaw = (rbody.velocity.x>0) ? 90:-90;
        // transform.rotation.eulerAngles = new Vector3(0f,yaw,0f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (grounded || coyote))
        {
            //reset y momentum for coyote jump
            Vector3 newV = rbody.velocity;
            newV.y = 0f;
            rbody.velocity = newV;
            rbody.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
            jumping = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && jumping)
        {
            jumpBuffered = true;
            StartCoroutine(disableJumpBuffer());
        }
        else if (!grounded && Input.GetKey(KeyCode.Space))
        {
            if (rbody.velocity.y > 0)
            {
                rbody.AddForce(Vector3.up*jumpBoost,ForceMode.Force);
            }
            
        }
    }

    private void animate()
    {
        float ySpeed = rbody.velocity.x;
        anim.SetFloat("Speed",Mathf.Abs(ySpeed));
        anim.SetBool("In Air",!grounded);
    }
    
    private float HorizontalMovement()
    {
        float horizontalMovement = Input.GetAxis("Horizontal");
        
        rbody.velocity += Vector3.right * (horizontalMovement * Time.deltaTime * speed);
        if (Math.Abs(rbody.velocity.x) > maxSpeed)
        {
            Vector3 newV = rbody.velocity;
            newV.x = Mathf.Clamp(newV.x,-maxSpeed, maxSpeed);
            rbody.velocity = newV;
        }

        if (horizontalMovement < 0.1f)
        {
            //slow down?
        }

        return horizontalMovement;
    }


    private void moveCamera()
    {
        var camPosition = camera.transform.position;
        camera.transform.SetPositionAndRotation
            (new Vector3(transform.position.x,camPosition.y,camPosition.z),Quaternion.identity);
    }

    private void determineIfGrounded()
    {
        //Capsule Collider
        // Vector3 startpoint = transform.position;
        // Vector3 endpoint = startpoint + Vector3.down * halfH;
        // grounded = (Physics.Raycast(startpoint, Vector3.down, halfH));
        // Color lineColor = (grounded) ? Color.red : Color.blue;
        // Debug.DrawLine(startpoint,endpoint,lineColor,0f,false);
        
        //Box collider
        // Get the center of the character's bounds
        Vector3 center = col.bounds.center;
        // Get the extents of the character's bounds
        Vector3 extents = col.bounds.extents;
        // Calculate the bottom point of the character's bounds
        Vector3 bottomPoint = center - new Vector3(0, extents.y, 0);
        // Shoot a raycast downwards from the bottom of the box collider
        RaycastHit hit;
        float raycastDistance = 2f; // Distance of the raycast
        grounded = Physics.Raycast(bottomPoint, -transform.up, out hit, raycastDistance);
        // Debug draw the raycast
        Debug.DrawRay(bottomPoint, -transform.up * raycastDistance, grounded ? Color.green : Color.red);
    }
    
    void GroundCheck()
    {
        bool oldGrounded = grounded;
        // Calculate the position of the ground check box
        Vector3 groundCheckBoxPosition = transform.position - Vector3.up * (groundCheckSize.y / 2f);
        // Perform overlap box check to see if the ground check box overlaps with any ground objects
        grounded = Physics.OverlapBox(groundCheckBoxPosition, groundCheckSize / 2f, Quaternion.identity, groundLayer).Length > 0;
        DebugDrawGroundCheckBox(groundCheckBoxPosition, groundCheckSize, grounded);
        if (grounded)
        {
            transform.position += Vector3.up * groundCheckSize.y/50f;
            coyote = true;
        }

        if (oldGrounded && !grounded) //leaving ground
        {
            StartCoroutine(disableCoyote());
        } else if (!oldGrounded && grounded) //hitting ground
        {
            jumping = false;
            if (jumpBuffered)
            {
                Jump();
                Debug.Log("BUFFERED JUMP GO!");
            }
        }
    }

    void DebugDrawGroundCheckBox(Vector3 center, Vector3 size, bool grounded)
    {
        // Define the color based on grounded state
        Color color = grounded ? Color.green : Color.red;

        // Draw the ground check box                                      
        Debug.DrawRay(center + new Vector3(-size.x / 2f,  -size.y / 2f,0f), Vector3.right * size.x, color);
        Debug.DrawRay(center + new Vector3(-size.x / 2f,  -size.y / 2f,0f), Vector3.up * size.y, color);
        Debug.DrawRay(center + new Vector3(-size.x / 2f,  size.y / 2f,0f), Vector3.right * size.x, color);
        Debug.DrawRay(center + new Vector3(size.x / 2f,  -size.y / 2f,0f), Vector3.up * size.y, color);
    }

    IEnumerator disableCoyote()
    {
        // Debug.Log("Getting ready to change coyote value...");
        yield return new WaitForSeconds(coyoteTimeThreshhold);
        // Debug.Log("Coyote is off now");
        coyote = false;
    }
    
    IEnumerator disableJumpBuffer()
    {
        yield return new WaitForSeconds(bufferTimeThreshhold);
        jumpBuffered = false;
    }
}
