using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerScore
{
    // Start is called before the first frame update
    public static float player1Score = 0f;
    public static float player2Score = 0f;

    public static void ResetScore()
    {
        player1Score = 0f;
        player2Score = 0f;
    }
}
