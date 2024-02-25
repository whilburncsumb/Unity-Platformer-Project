using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoombaScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float moveSpeed = 2.5f;
    public float raycastDistance = .01f;
    public LayerMask groundLayer;
    private float yVelocity = 0;
    private Collider col;
    private bool movingRight = false;

    void Start()
    {
        col = GetComponent<Collider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FlipSpritePeriodically());
        // groundCheckSize = new Vector3(0.7f, .05f, 0.7f);
        // bounds = col.bounds.extents;
    }


    IEnumerator FlipSpritePeriodically()
    {
        while (true)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            yield return new WaitForSeconds(0.25f);
        }
    }
    
    void Update()
    {
        // Check for obstacles to the left and right
        bool obstacleToLeft = CheckObstacle(Vector2.left,.1f);
        bool obstacleToRight = CheckObstacle(Vector2.right,.1f);

        // Reverse direction if an obstacle is detected
        if (obstacleToLeft || obstacleToRight)
        {
            movingRight = !movingRight;
        }

        // Move the character based on the current direction
        Vector3 movement = (movingRight) ? Vector3.right : Vector3.left;
        transform.Translate(movement * moveSpeed * Time.deltaTime);
        ApplyGravity();
    }
    
    bool CheckObstacle(Vector3 direction, float distanceFromEdge)
    {
        // Get the BoxCollider component attached to the GameObject
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        // Adjust the raycastOrigin based on the BoxCollider's size and center
        Vector3 raycastOrigin = transform.position + boxCollider.center + direction 
            * (boxCollider.size.x*transform.localScale.x) / 2f;

        // Cast a ray to check for obstacles in the specified direction
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(raycastOrigin, direction, out hit, raycastDistance, groundLayer);

        // Draw the raycast for visualization
        Color rayColor = hitSomething ? Color.red : Color.green;
        Debug.DrawRay(raycastOrigin, direction * raycastDistance, rayColor);

        // Return true if an obstacle is detected
        return hitSomething;
    }


    
    void ApplyGravity()
    {
        // Check if there's nothing beneath the character
        // Vector3 groundCheckBoxPosition = transform.position - Vector3.up * (groundCheckSize.y / 2f);
        // Vector3 groundCheckBoxPosition = transform.position - Vector3.up*2f;
        // bool grounded = Physics.OverlapBox(groundCheckBoxPosition, groundCheckSize / 2f, Quaternion.identity, groundLayer).Length > 0;
        // DebugDrawGroundCheckBox(groundCheckBoxPosition, groundCheckSize, grounded);
        bool grounded = CheckObstacle(Vector2.down,.1f);
        if (!grounded)
        {
            yVelocity += Physics.gravity.y * Time.deltaTime;
            yVelocity = Mathf.Clamp(yVelocity, -15, 15);

            // Calculate the displacement based on the current velocity
            float displacement = yVelocity * Time.deltaTime;
            transform.Translate(Vector3.up * displacement);
        }
        else
        {
            yVelocity = 0;
        }
    }
}
