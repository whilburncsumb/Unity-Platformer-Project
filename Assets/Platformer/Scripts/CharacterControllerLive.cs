using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerLive : MonoBehaviour
{
    public float speed = 250f;
    public float maxSpeed = 10f;
    public float jumpForce;
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
        
        rbody.velocity += Vector3.right *horizontalMovement *Time.deltaTime *speed;
        if (rbody.velocity.x > maxSpeed)
        {
            // rbody.velocity = rbody.velocity.normalized * maxSpeed;
            Vector3 newV = rbody.velocity;
            newV.x = maxSpeed;
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
        }



        
    }
}
