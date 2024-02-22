using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelParser : MonoBehaviour
{
    public string filename;
    public GameObject rockPrefab;
    public GameObject brickPrefab;
    public GameObject questionBoxPrefab;
    public GameObject stonePrefab;
    public GameObject notePrefab;
    public Transform environmentRoot;

    // --------------------------------------------------------------------------
    void Start()
    {
        LoadLevel();
    }

    // --------------------------------------------------------------------------
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadLevel();
        }
    }

    // --------------------------------------------------------------------------
    private void LoadLevel()
    {
        string fileToParse = $"{Application.dataPath}{"/Resources/"}{filename}.txt";
        Debug.Log($"Loading level file: {fileToParse}");

        Stack<string> levelRows = new Stack<string>();

        // Get each line of text representing blocks in our level
        using (StreamReader sr = new StreamReader(fileToParse))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                levelRows.Push(line);
            }

            sr.Close();
        }

        int row = 0;

        // Go through the rows from bottom to top
        while (levelRows.Count > 0)
        {
            string currentLine = levelRows.Pop();

            char[] letters = currentLine.ToCharArray();
            for (int column = 0; column < letters.Length; column++)
            {
                var letter = letters[column];
                // Todo - Instantiate a new GameObject that matches the type specified by letter
                // Todo - Position the new GameObject at the appropriate location by using row and column
                // Todo - Parent the new GameObject under levelRoot
                // if (letter == 'x')
                // {
                //     Vector3 newPos = new Vector3(column+.5f,row+.5f,0f); 
                //     Instantiate(rockPrefab, newPos,Quaternion.identity,environmentRoot);
                // }

                switch (letter)
                {
                    case 'x':
                    {
                        Vector3 newPos = new Vector3(column+.5f,row+.5f,0f); 
                        Instantiate(rockPrefab, newPos,Quaternion.identity,environmentRoot);
                        break;
                    }
                    case 'b':
                    {
                        Vector3 newPos = new Vector3(column+.5f,row+.5f,0f); 
                        Instantiate(brickPrefab, newPos,Quaternion.identity,environmentRoot);
                        break;
                    }
                    case '?':
                    {
                        Vector3 newPos = new Vector3(column+.5f,row+.5f,0f); 
                        Instantiate(questionBoxPrefab, newPos,Quaternion.identity,environmentRoot);
                        break;
                    }
                    case 's':
                    {
                        Vector3 newPos = new Vector3(column+.5f,row+.5f,0f); 
                        Instantiate(stonePrefab, newPos,Quaternion.identity,environmentRoot);
                        break;
                    }
                    case 'n':
                    {
                        Vector3 newPos = new Vector3(column+.5f,row+.5f,0f); 
                        Instantiate(notePrefab, newPos,Quaternion.identity,environmentRoot);
                        break;
                    }
                }
            }

            row++;
        }
    }

    // --------------------------------------------------------------------------
    private void ReloadLevel()
    {
        foreach (Transform child in environmentRoot)
        {
           Destroy(child.gameObject);
        }
        LoadLevel();
    }
}
