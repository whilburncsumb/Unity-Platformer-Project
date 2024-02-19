using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class demologic : MonoBehaviour
{
    public GameObject package;
    public GameObject parachute;
    public float parachuteDrag = 5f;
    public float deploymentHeight;
    public float landingHeight = 1;
    public float openDuration = 0.5f;

    private float originalDrag;
    // Start is called before the first frame update
    void Start()
    {
        originalDrag = package.GetComponent<Rigidbody>().drag;
        parachute.SetActive(true);
        StartCoroutine(ExpandParachute());
    }

    IEnumerator ExpandParachute()
    {
        parachute.transform.localScale = Vector3.zero;
        float timeElapsed = 0f;
        float duration = 1.5f;
        while (timeElapsed < duration)
        {
            float newScale = timeElapsed / duration;
            parachute.transform.localScale = new Vector3(newScale, newScale, newScale);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Debug.Log("About to open...");
        // yield return new WaitForSecondsRealtime(1f);
        // Debug.Log("Its open!");
        // yield return new WaitForSecondsRealtime(1f);
        // Debug.Log("We landed!");
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.DrawRay(package.transform.position,Vector3.down * deploymentHeight,Color.red);
        //     Debug.Break();
        // }
        // RaycastHit hitInfo;
        // if (Physics.Raycast(package.transform.position, Vector3.down, out hitInfo, deploymentHeight))
        // {
        //     package.GetComponent<Rigidbody>().drag = parachuteDrag;
        //     Debug.DrawRay(package.transform.position,Vector3.down * deploymentHeight,Color.red);
        //     parachute.SetActive(true);
        //     if (hitInfo.distance < landingHeight)
        //     {
        //         // Debug.Log("landed");
        //         parachute.SetActive(false);
        //     }
        //     
        // }
        // else
        // {
        //     package.GetComponent<Rigidbody>().drag = originalDrag;
        //     parachute.SetActive(false);
        //     Debug.DrawRay(package.transform.position,Vector3.down * deploymentHeight,Color.cyan);
        // }
        
    }

    
}
