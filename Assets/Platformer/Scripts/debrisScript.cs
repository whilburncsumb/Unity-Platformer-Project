using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debrisScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyObject",1f);
    }
    void DestroyObject()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.localScale -= new Vector3(0.005f, 0.005f, 0.005f);
        if (transform.localScale.x < 0)
        {
            Destroy(gameObject);
        }
    }
}
