using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameOverMenu : MonoBehaviour
{

    public Text winText;
    public Text scoreText;

    private void OnEnable()
    {
        winText.text = string.Format("Player {0} won the game!", PlayerScore.winningPlayer);
        scoreText.text = string.Format("The score was {0} - {1}", PlayerScore.player1Score, PlayerScore.player2Score);
    }

    public void BackButton()
    {
        PlayerScore.ResetScore();
        SceneManager.LoadScene(0);
    }




}
