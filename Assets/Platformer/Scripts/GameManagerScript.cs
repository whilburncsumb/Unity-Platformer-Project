using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

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
    private int score;
    public CharacterControllerLive mario;
    public GameObject coinPrefab;
    public GameObject debrisPrefab;
    // Variables that control the pitch of music note blocks
    public float minY;
    public float maxY;
    public float minPitch;
    public float maxPitch;
    
    public float fadeDuration = 1f; // Duration of the fade-in animation
    public Image youDied;
    public Image youWon;

    private float elapsedTime = 0f;
    private Color targetColor = Color.clear;
    private float startTime;
    public int maxClockTime;

    private void Start()
    {
        patternSwitchTime = patternSwitchInterval;
        coins = 0;
        score = 0;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        updateHUD();
        questionBlockAnimation();
        mouseActions();
    }

    public void addCoin()
    {
        coins++;
    }

    public void incrementScore(int scorebonus)
    {
        score += scorebonus;
    }

    private void updateHUD()
    {
        int intTime = maxClockTime - (int)(Time.time - startTime);
        if (intTime <= 0)
        {
            mario.Die();
        }
        string timeStr = $"TIME \n {intTime}";
        timerText.text = timeStr;
        string coinStr = coins.ToString("D3");
        coinText.text =  "COINS\n" + coinStr;
        string scoreString = score.ToString("D6");
        scoreText.text = "MARIO\n" + scoreString;
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
    
    public void StartFadeIn(Image i)
    {
        // Enable the Image component
        i.enabled = true;

        // Start fade-in animation
        targetColor = youDied.color; // Make sure targetColor has the same color as the current color
        targetColor.a = 1f; // Set the alpha value to fully opaque

        // Start the fade-in coroutine
        StartCoroutine(FadeInCoroutine(i));
    }

    void RestartGame()
    {
        // Restart the game here
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Start();
    }
    
    IEnumerator FadeInCoroutine(Image i)
    {
        while (elapsedTime < fadeDuration)
        {
            // Calculate the current alpha value based on the elapsed time
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            // Update the color with the new alpha value
            i.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            // Wait for the next frame
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // Ensure the final color is set correctly
        i.color = targetColor;
        yield return new WaitForSeconds(3f);
        // Restart the game here
        RestartGame();
    }

    public void Explode()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.Play();
    }
}
