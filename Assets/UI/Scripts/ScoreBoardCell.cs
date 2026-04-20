using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardCell : MonoBehaviour
{
    public Text cellText;
    public Image cellBackground;

    public Color defaultColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color currentColor = new Color(0.2f, 0.4f, 0.8f, 0.8f);
    public Color myScoreColor = new Color(0.1f, 0.5f, 0.1f, 0.8f);
    public Color enemyScoreColor = new Color(0.5f, 0.1f, 0.1f, 0.8f);
    public Color totalColor = new Color(0.4f, 0.1f, 0.6f, 0.8f);

    public void SetText(string text)
    {
        if (cellText != null)
            cellText.text = text;
    }

    public void SetDefault()
    {
        if (cellBackground != null)
            cellBackground.color = defaultColor;
    }

    public void SetCurrent()
    {
        if (cellBackground != null)
            cellBackground.color = currentColor;
    }

    public void SetMyScore()
    {
        if (cellBackground != null)
            cellBackground.color = myScoreColor;
    }

    public void SetEnemyScore()
    {
        if (cellBackground != null)
            cellBackground.color = enemyScoreColor;
    }

    public void SetTotal()
    {
        if (cellBackground != null)
            cellBackground.color = totalColor;
    }
}
