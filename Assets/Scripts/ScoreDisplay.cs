using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Start()
    {
        if (scoreText != null)
        {
            // Reads the static score from the previous scene
            scoreText.text = "Final Kills: " + ScoreManager.currentScore.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
