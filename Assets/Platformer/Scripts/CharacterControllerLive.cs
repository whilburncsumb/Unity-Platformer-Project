using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
    public LayerMask enemyLayer;
    private Vector3 groundCheckSize;
    public float coyoteTimeThreshhold;
    public float bufferTimeThreshhold;
    public float maxFallSpeed;
    private AudioSource sound;
    public Image youDied;
    public Image youWon;
    public AudioClip yay;
    public GameObject fireExplosion;
    public GameObject coinPrefab;
    public GameObject debrisPrefab;
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
        if (!canMove)
        {
            return;
        }
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
        if (!canMove)
        {
            return 0f;
        }
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
            if (grounded || !goombaStomp())
            {
                Die();
            }
        }

        if (other.gameObject.CompareTag("flagpoleTag"))
        {
            manager.incrementScore(10000);
            Win();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("breakableBrick") && hitBlocks() && !grounded)
        {
            SpawnDebris(other);
            manager.incrementScore(100);
        } else if (other.gameObject.CompareTag("coinBlock") && hitBlocks() && !grounded)
        {
            manager.addCoin();
            manager.incrementScore(100);
            Instantiate(coinPrefab, other.transform.position+Vector3.up, Quaternion.identity);
        }
    }

    private void SpawnDebris(Collision other)
    {
        Destroy(other.gameObject);
        float rotationSpeed = 20;
        float debrisForce = 4;
        for (int i = 0; i < 4; i++)//Spawn 4 brick chunks
        {
            GameObject debris = Instantiate(debrisPrefab, other.transform.position, Quaternion.identity);
                
            // Apply initial direction and random rotation to debris
            Vector3 direction = Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(-45f, 45f), Random.Range(0f, 360f)) * Vector3.up;
            debris.GetComponent<Rigidbody>().velocity = direction * debrisForce;
                
            // Apply random rotation and spin to debris
            debris.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * rotationSpeed;
        }
    }

    private bool goombaStomp()
    {
        Debug.Log("Checking for a stomp...");
        Vector3 groundCheckBoxPosition = transform.position - Vector3.up * (groundCheckSize.y / 2f);
        bool hit = false;
        Collider[] colliders = Physics.OverlapBox(groundCheckBoxPosition, groundCheckSize / 2f, Quaternion.identity, enemyLayer);
        foreach (Collider collider in colliders)
        {
            Debug.Log("Killing!");
            Instantiate(fireExplosion, collider.transform.position,Quaternion.identity);
            manager.incrementScore(300);
            manager.Explode();
            Destroy(collider.gameObject);
            hit = true;
        }
    
        // Return false if no enemies were stomped
        return hit;
    }

    private bool hitBlocks()
    {
        Vector3 ceilingCheckBoxPosition = transform.position + Vector3.up * ((groundCheckSize.y / 2f) + col.bounds.size.y);
        // Perform overlap box check to see if the ceiling check box overlaps with any objects above
        DebugDrawGroundCheckBox(ceilingCheckBoxPosition, groundCheckSize, Color.yellow);
        return Physics.OverlapBox(ceilingCheckBoxPosition, groundCheckSize / 2f, Quaternion.identity, groundLayer).Length > 0;
    }

    public void Win()
    {
        Debug.Log("YOU WIN!");
        manager.StartFadeIn(youWon);
        sound.clip = yay;
        sound.Play();
    }

    public void Die()
    {
        Debug.Log("YOU LOSE!");
        manager.StartFadeIn(youDied);
        Instantiate(fireExplosion, transform.position,Quaternion.identity);
        canMove = false;
        manager.Explode();
        Object.Destroy(this.gameObject);
    }
    
    void GroundCheck()
    {
        bool oldGrounded = grounded;
        // Calculate the position of the ground check box
        Vector3 groundCheckBoxPosition = transform.position - Vector3.up * (groundCheckSize.y / 2f);
        // Perform overlap box check to see if the ground check box overlaps with any ground objects
        grounded = Physics.OverlapBox(groundCheckBoxPosition, groundCheckSize / 2f, Quaternion.identity, groundLayer).Length > 0;
        // Define the color based on grounded state
        Color color = grounded ? Color.green : Color.red;
        DebugDrawGroundCheckBox(groundCheckBoxPosition, groundCheckSize, color);
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

    void DebugDrawGroundCheckBox(Vector3 center, Vector3 size, Color color)
    {
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
