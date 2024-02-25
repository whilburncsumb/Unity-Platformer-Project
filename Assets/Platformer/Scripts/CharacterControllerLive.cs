using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterControllerLive : MonoBehaviour
{
    public float speed = 50f;
    public float maxWalkSpeed;
    public float maxSpeed = 15f;
    public float jumpForce = 10f;
    public float jumpBoost = .3f;
    public float jumpHaltFactor;
    public float horizontalSlowdownFactor;
    private bool grounded;
    private bool jumping;
    private bool coyote;
    private bool jumpBuffered;
    private Rigidbody rbody;
    public new Camera camera;
    private Animator anim;
    private Collider col;
    public GameManagerScript manager;
    public LayerMask groundLayer; // Layer mask for the ground objects
    private Vector3 groundCheckSize; // Size of the ground check box
    public float coyoteTimeThreshhold;
    public float bufferTimeThreshhold;
    public float maxFallSpeed;
    private AudioSource sound;
    public Image youDied;
    public Image youWon;
    public AudioClip explosion;
    public AudioClip yay;
    private bool canMove = true;
    
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
        coyote = false;
        sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        var horizontalMovement = HorizontalMovement();
        GroundCheck();
        Jump();
        animate();
        moveCamera();
    }

    private void FixedUpdate()
    {
        var horizontalMovement = HorizontalMovement();
        BodyRotation(horizontalMovement);
        slowDown();
    }

    private void BodyRotation(float horizontalMovement)
    {
        if(rbody.velocity.x==0){return;}
        //Set model rotation
        float yaw = (rbody.velocity.x>0) ? 90:-90;
        // transform.rotation.eulerAngles = new Vector3(0f,yaw,0f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (grounded || coyote))
        {
            jumping = true;
            ApplyJumpForce();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && jumping)
        {
            jumpBuffered = true;
            StartCoroutine(disableJumpBuffer());
        }
        // else if (!grounded && Input.GetKey(KeyCode.Space))
        // {
        //     if (rbody.velocity.y > 0)
        //     {
        //         rbody.AddForce(Vector3.up*jumpBoost,ForceMode.Force);
        //     }
        // }
        //Clamp fall speed
        if (!grounded && rbody.velocity.y < maxFallSpeed)
        {
            rbody.velocity = new Vector3(rbody.velocity.x, maxFallSpeed, 0f);
            // Debug.Log("CLAMPING V SPEED");
        }
    }

    private void ApplyJumpForce()
    {
        //Reset y velocity
        Vector3 newV = rbody.velocity;
        newV.y = 0f;
        rbody.velocity = newV;
        rbody.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
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
        Vector3 newV = rbody.velocity;
        if (Input.GetKey(KeyCode.LeftShift))//running
        {
            newV.x = Mathf.Clamp(newV.x,-maxSpeed, maxSpeed);
        }
        else //walking
        {
            newV.x = Mathf.Clamp(newV.x,-maxWalkSpeed, maxWalkSpeed);
        }

        // Apply the new velocity
        rbody.velocity = newV;

        return horizontalMovement;
    }

    void slowDown()
    {
        Vector3 newV = rbody.velocity;
        //Slow horizontaly
        float horizontalMovement = Input.GetAxis("Horizontal");
        // Slow down gradually when not holding a direction
        if (newV.x != 0f && Mathf.Abs(horizontalMovement) < 0.5f)
        {
            newV.x *= horizontalSlowdownFactor;
            if (newV.x < 0.01f && newV.x > -0.01f)
            {
                newV.x = 0;
            }
            // Debug.Log("Slowing down! X velocity: " + newV.x);
        }
        
        //Slow vertically
        if (jumping && !Input.GetKey(KeyCode.Space) && rbody.velocity.y > 0f && rbody.velocity.y < (jumpForce*.75f))
        {
            //If they let go of space during the upward portion of a jump
            // Debug.Log("slowing down jump...");
            newV.y *= jumpHaltFactor;
            rbody.velocity = newV;
        }
        
        rbody.velocity = newV;
    }


    private void moveCamera()
    {
        var camPosition = camera.transform.position;
        camera.transform.SetPositionAndRotation
            (new Vector3(transform.position.x,camPosition.y,camPosition.z),Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemyTag"))
        {
            Debug.Log("YOU LOSE!");
            manager.StartFadeIn(youDied);
            sound.clip = explosion;
            sound.Play();
            
        }

        if (other.gameObject.CompareTag("flagpoleTag"))
        {
            Debug.Log("YOU WIN!");
            manager.StartFadeIn(youWon);
            sound.clip = yay;
            sound.Play();
        }
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
            coyote = true;
        }

        if (oldGrounded && !grounded) //leaving ground
        {
            StartCoroutine(disableCoyote());
        } else if (!oldGrounded && grounded) //hitting ground
        {
            transform.position += Vector3.up * groundCheckSize.y/50f;
            jumping = false;
            if (jumpBuffered)
            {
                ApplyJumpForce();
                jumpBuffered = false;
                // Debug.Log("BUFFERED JUMP GO!");
            }
        }
        rbody.useGravity = !grounded;
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
        yield return new WaitForSeconds(coyoteTimeThreshhold);
        coyote = false;
    }
    
    IEnumerator disableJumpBuffer()
    {
        yield return new WaitForSeconds(bufferTimeThreshhold);
        jumpBuffered = false;
    }
}
