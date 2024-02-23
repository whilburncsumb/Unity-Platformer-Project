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
    private Rigidbody rbody;

    private float halfH;
    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();
        halfH = col.bounds.extents.y + 0.03f;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalMovement = Input.GetAxis("Horizontal");
        // rbody.velocity *= Math.Abs(horizontalMovement);
        
        
        rbody.velocity += Vector3.right *horizontalMovement *Time.deltaTime *speed;
        if (Math.Abs(rbody.velocity.x) > maxSpeed)
        {
            // rbody.velocity = rbody.velocity.normalized * maxSpeed;
            Vector3 newV = rbody.velocity;
            newV.x = Mathf.Clamp(newV.x,-maxSpeed, maxSpeed);
            rbody.velocity = newV;
        }

        
        Vector3 startpoint = transform.position;
        Vector3 endpoint = startpoint + Vector3.down * halfH;
        grounded = (Physics.Raycast(startpoint, Vector3.down, halfH));
        Color lineColor = (grounded) ? Color.red : Color.blue;
        Debug.DrawLine(startpoint,endpoint,lineColor,0f,false);
        
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rbody.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
        } else if (!grounded && Input.GetKey(KeyCode.Space))
        {
            if (rbody.velocity.y > 0)
            {
                rbody.AddForce(Vector3.up*jumpBoost,ForceMode.Force);
            }
            
        }

        if (Math.Abs(horizontalMovement)<0.5f)
        {
            // rbody.velocity *= Math.Abs(horizontalMovement);
            Vector3 newV = rbody.velocity;
            newV.x *= 1f - Time.deltaTime;
            rbody.velocity = newV;
        }
        float yaw = (rbody.velocity.x>0) ? 90:-90;
        // transform.rotation.eulerAngles = new Vector3(0f,yaw,0f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        float speedy = rbody.velocity.x;
        Animator anim = GetComponent<Animator>();
        anim.SetFloat("Speed",Mathf.Abs(speedy));
        anim.SetBool("In Air",!grounded);
    }
}
