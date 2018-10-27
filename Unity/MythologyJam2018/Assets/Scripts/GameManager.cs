using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int pointsToWin;
    public float pointsTextTime;
    public Text winText;

    [HideInInspector]
    public int playerOneScore;
    [HideInInspector]
    public int playerTwoScore;

    [HideInInspector]
    public GameState gameState;

    public enum GameState
    {
        Playing,
        Winning
    }

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out GameManager manager)
    {
        manager = instance;
        return (manager != null);
    }

    private void Start()
    {
        winText.gameObject.SetActive(false);
    }

    public void AddPoint(int player)
    {
        switch (player)
        {
            case 0:
                playerOneScore++;
                break;
            case 1:
                playerTwoScore++;
                break;
        }

        ScoreboardManager scoreboardManager;
        if (ScoreboardManager.TryGetInstance(out scoreboardManager))
        {
            scoreboardManager.AddPoint(player);
        }

        CheckWin();
    }

    private void CheckWin()
    {
        Debug.Log("player one: " + playerOneScore);
        Debug.Log("player two: " + playerTwoScore);

        bool playerWon = false;

        if (playerOneScore >= pointsToWin)
        {
            playerWon = true;
            winText.text = "Player One Wins!";
        }
        else if (playerTwoScore >= pointsToWin)
        {
            playerWon = true;
            winText.text = "Player Two Wins!";
        }

        if (playerWon)
        {
            gameState = GameState.Winning;
            winText.gameObject.SetActive(true);
            StartCoroutine(ShowWinText());
        }
    }

    private void UpdateScoreboard(int player)
    {
        ScoreboardManager scoreboardManager;
        if (ScoreboardManager.TryGetInstance(out scoreboardManager))
        {
            scoreboardManager.AddPoint(player);
        }
    }

    private void ResetScoreboard()
    {
        ScoreboardManager scoreboardManager;
        if (ScoreboardManager.TryGetInstance(out scoreboardManager))
        {
            scoreboardManager.SetScore(0, 0);
            scoreboardManager.SetScore(1, 0);
        }
    }

    private void ResetGame()
    {
        playerOneScore = 0;
        playerTwoScore = 0;
        ResetScoreboard();

        PlayerManager playerManager;
        if (PlayerManager.TryGetInstance(out playerManager))
        {
            playerManager.ResetPlayers();
        }

        winText.gameObject.SetActive(false);
        gameState = GameState.Playing;
    }

    private IEnumerator ShowWinText()
    {
        float deltaTime = 0f;

        while(deltaTime < pointsTextTime)
        {
            deltaTime += Time.deltaTime;
            yield return null;
        }

        ResetGame();
    }
}
