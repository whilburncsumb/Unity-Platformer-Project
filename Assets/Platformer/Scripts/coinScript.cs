using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coinScript : MonoBehaviour
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

    private void FixedUpdate()
    {
        transform.Translate(new Vector3(0f, .05f, 0f));
    }
}
