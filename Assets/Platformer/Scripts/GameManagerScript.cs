using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManagerScript : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public Material questionBlock;
    private float patternSwitchTime;
    public float patternSwitchInterval;
    private int coins;
    public Camera mainCamera;
    public float cameraSpeed;
    public GameObject coinPrefab;
    public GameObject debrisPrefab;
    // Variables that control the pitch of music note blocks
    public float minY;
    public float maxY;
    public float minPitch;
    public float maxPitch;
    

    private void Start()
    {
        patternSwitchTime = patternSwitchInterval;
        coins = 0;
    }

    // Update is called once per frame
    void Update()
    {
        updateHUD();
        questionBlockAnimation();
        mouseActions();
        moveCamera();
    }

    private void updateHUD()
    {
        int intTime = 400 - (int)Time.realtimeSinceStartup;
        string timeStr = $"TIME \n {intTime}";
        timerText.text = timeStr;
        string coinStr = $" \n {coins.ToString()}";
        coinText.text = coinStr;
    }

    private void mouseActions()
    {
        //Detect mouseclicks on destroyable bricks
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the mouse position into the scene
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits any objects
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object has a specific tag or layer
                if (hit.collider.CompareTag("breakableBrick") || hit.collider.gameObject.layer == LayerMask.NameToLayer("breakableBrick"))
                {
                    Destroy(hit.collider.gameObject);
                    spawnDebris(hit);
                }
                else if (hit.collider.CompareTag("coinBlock"))
                {
                    coins++;
                    Vector3 block = hit.collider.gameObject.transform.position;
                    Instantiate(coinPrefab, block + Vector3.up+Vector3.back, Quaternion.identity);
                }
                else if(hit.collider.CompareTag("noteBlock"))
                {
                    AudioSource block = hit.collider.gameObject.GetComponent<AudioSource>();
                    // Get the current y-position of the block
                    float yPos = hit.collider.gameObject.transform.position.y%8;
                    
                    // Calculate the pitch based on the y-position
                    float normalizedY = Mathf.InverseLerp(minY, maxY, yPos);
                    // float pitch = Mathf.Lerp(minPitch, maxPitch, normalizedY);
                    float pitch = Mathf.Lerp(minPitch, maxPitch, normalizedY);
                    block.pitch = pitch;
                    block.Play();
                    Debug.Log("Playing note in pitch: " + pitch);
                }
            }
        }
    }

    private void questionBlockAnimation()
    {
        patternSwitchTime -= Time.deltaTime;
        if (patternSwitchTime < 0)
        {
            patternSwitchTime = patternSwitchInterval;
            float x = questionBlock.mainTextureOffset.x;
            float y = questionBlock.mainTextureOffset.y + .2f;
            if (y >= 1f)
            {
                y = 0f;
            }
            questionBlock.mainTextureOffset = new Vector2(x,y);
        }
    }

    private void moveCamera()
    {
        float input = Input.GetAxis("Horizontal");
        float movement = input * cameraSpeed * Time.deltaTime;

        // Move the camera horizontally
        mainCamera.transform.Translate(new Vector3(movement, 0f, 0f));
    }

    private void spawnDebris(RaycastHit hit)
    {
        float rotationSpeed = 20;
        float debrisForce = 4;
        for (int i = 0; i < 4; i++)//Spawn 4 brick chunks
        {
            GameObject debris = Instantiate(debrisPrefab, hit.point, Quaternion.identity);
    
            // Apply initial direction and random rotation to debris
            Vector3 direction = Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(-45f, 45f), Random.Range(0f, 360f)) * Vector3.up;
            debris.GetComponent<Rigidbody>().velocity = direction * debrisForce;
    
            // Apply random rotation and spin to debris
            debris.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * rotationSpeed;
        }
    }
}
