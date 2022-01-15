using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerScore
{
    // Start is called before the first frame update
    public static float player1Score = 0f;
    public static float player2Score = 0f;

    public static int winningPlayer = 0;

    public static int scoreToWin = 10;

    public static void ResetScore()
    {
        player1Score = 0f;
        player2Score = 0f;
        winningPlayer = 0;
    }


    // check if one of the players won the game
    public static bool CheckPlayerWon()
    {
        if (player1Score >= scoreToWin)
        {
            winningPlayer = 1;
        
        }
        else if (player2Score >= scoreToWin)
        {
            winningPlayer = 2;
        }
        else
        {
            winningPlayer = 0;
        }
        return winningPlayer != 0;

    }
}
