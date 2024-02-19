using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
                }
                if (hit.collider.CompareTag("coinBlock"))
                {
                    coins++;
                    Vector3 block = hit.collider.gameObject.transform.position;
                    Instantiate(coinPrefab, block + Vector3.up+Vector3.back, Quaternion.identity);
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
}
