using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

    //public static int score = 0;
	public static int highScore=0;
    public static int score = 0;
    [SerializeField] private Text scoreText = null;
    [SerializeField] private Text addedScoreText = null;
	[SerializeField] private Text highScoreText =null;

    private Animator scoreAnim = null;
    private const string SHOW_TEXT = "showText";




    private void Awake()
    {
        scoreAnim = addedScoreText.GetComponent<Animator>();
        if (highScoreText != null)
        {
            highScore = PlayerPrefs.GetInt("highscore", 0);
            highScoreText.text = "HIGH: " + string.Format("{0:#,#}", highScore);
        }
    }

    private void Start()
    {
        HelixDetector.PastPieces += PieceExceeded;
        scoreText.text = score.ToString();

    }

    public static void resetScore()
    {
        score = 0;
    }

    private void PieceExceeded(int quantity)
    {
        int addedScore = 0;

        switch (quantity)
        {
            case 1:
                addedScore += 10;
                break;
            case 2:
                addedScore += 30;
                break;
            case 3:
                addedScore += 70;
                break;
            case 4:
                addedScore += 150;
                break;
            default:
                if(quantity <= 0)
                {
                    Debug.LogError("Value of quantity is less than or equal to zero: " + quantity);
                }
                else if(quantity >= 5)
                {
                    addedScore += 300;
                }
                break;
        }
        score += addedScore;
        addedScoreText.text = "+" + addedScore.ToString();
        scoreAnim.SetTrigger(SHOW_TEXT);
        scoreText.text = score.ToString();
        updateHighScore(score);
    }

    private void updateHighScore(int score)
    {
        if (score>highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highscore", highScore);
            highScoreText.text = "HIGH: " + string.Format("{0:#,#}", highScore);
        }
    }

    private void OnDestroy()
    {
        HelixDetector.PastPieces -= PieceExceeded;
       // score = 0;
    }
}
