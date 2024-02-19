using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Material questionBlock;
    private float patternSwitchTime;
    public float patternSwitchInterval;

    private void Start()
    {
        patternSwitchTime = patternSwitchInterval;
    }

    // Update is called once per frame
    void Update()
    {
        int intTime = 400 - (int)Time.realtimeSinceStartup;
        string timeStr = $"Time: \n {intTime}";
        timerText.text = timeStr;
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
    
}
