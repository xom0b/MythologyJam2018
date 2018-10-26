using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
{
    public ScoreboardPoint playerOneScore;
    public ScoreboardPoint playerTwoScore;

    public static ScoreboardManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out ScoreboardManager manager)
    {
        manager = instance;
        return (manager != null);
    }

    public void AddPoint(int player)
    {
        switch(player)
        {
            case 0:
                playerOneScore.SetScore(playerOneScore.score + 1);
                break;
            case 1:
                playerTwoScore.SetScore(playerTwoScore.score + 1);
                break;
        }
    }

    public void SetScore(int player, int score)
    {
        switch (player)
        {
            case 0:
                playerOneScore.SetScore(score);
                break;
            case 1:
                playerTwoScore.SetScore(score);
                break;
        }
    }
}
