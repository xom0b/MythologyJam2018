using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardPoint : MonoBehaviour
{
    public int player;
    public List<Text> scoreTexts;

    public int score;

    public void SetScore(int score)
    {
        this.score = score;
        foreach(Text text in scoreTexts)
        {
            text.text = score.ToString();
        }
    }
}
