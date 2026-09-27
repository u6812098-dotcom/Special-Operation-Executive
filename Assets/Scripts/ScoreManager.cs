using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static int currentScore = 0;
    public TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Start()
    {
        currentScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (scoreText != null)
        {
            // Reads the static score from the previous scene
            scoreText.text = "Kills: " + ScoreManager.currentScore.ToString();
        }
    }
}
