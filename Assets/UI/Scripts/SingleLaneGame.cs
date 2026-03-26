using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleLaneGame : MonoBehaviour
{
    public SingleLanePlayer me;
    public SingleLanePlayer you;

    public Text inningText;
    public Text turnText;
    public Text gameOverText;
    public GameObject gameOverPannel;
    public Button endTurnButton;

    private int inning;
    private bool isPlayerTurn;
    private bool gameOver;

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        inning = 1;
        isPlayerTurn = true;

        me.Initialize(false);
        you.Initialize(true);

        UpdateGameUI();
        StartTurn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTurn()
    {
        if(gameOver) return;

        SingleLanePlayer current = isPlayerTurn ? me : you;
        current.DrawTurnCards();

        UpdateGameUI();

        if(!isPlayerTurn)
        {
            StartCoroutine(AITurnRoutine());
        }
    }
    // 턴 종료 클릭
    public void ClickEndTurn()
    {
        if (gameOver || !isPlayerTurn) return;
        EndCurrentTurn();
    }

    public void ClickUseSelectedCard()
    {
        if(gameOver || !isPlayerTurn) return;
        if(!me.CheckSelectedCard()) return;

        StartCoroutine(ProcessCardUse(me));
    }

  
    // 전투
    private IEnumerator AITurnRoutine()
    {
        yield return new WaitForSeconds(0.7f);

        while (!gameOver && !isPlayerTurn)
        {
            if (you.GetHandCount() <= 0 || you.GetOutCount() >=3)
            {
                EndCurrentTurn();
                yield break;
            }

            you.AISelectCard();
            yield return StartCoroutine(ProcessCardUse(you));
            yield return new WaitForSeconds(0.7f);
        }
    }

    private IEnumerator ProcessCardUse(SingleLanePlayer currentPlayer)
    {
        currentPlayer.MoveSelectedCardToBattleZone();
        yield return new WaitForSeconds(0.3f);

        int cardType = currentPlayer.UseSelectedCard();

        ApplyCardResult(currentPlayer, cardType);
        UpdateGameUI();

        if(CheckTurnEnd(currentPlayer))
        {
            yield return new WaitForSeconds(0.5f);
            EndCurrentTurn();
        }
    }

    private void ApplyCardResult(SingleLanePlayer player, int cardType)
    {
        switch (cardType)
        {
            case 0:
                player.AddOut();
                break;
            case 1:
                player.AddScore(1);
                break;
            case 2:
                player.AddScore(2);
                break;
            case 3:
                player.AddScore(3);
                break;
            case 4:
                player.AddScore(4);
                break;
        }
    }

    private bool CheckTurnEnd(SingleLanePlayer player)
    {
        return player.GetHandCount() <= 0 || player.GetOutCount() >= 3;
    }

    private void EndCurrentTurn()
    {
        SingleLanePlayer current = isPlayerTurn ? me : you;
        current.ResetOutCount();

        if(!isPlayerTurn)
        {
            inning++;
        }

        if(CheckGameOver())
        {
            return;
        }

        isPlayerTurn = !isPlayerTurn;
        UpdateGameUI();
        StartTurn();
    }

    private bool CheckGameOver()
    {
        if (inning < 10) return false;

        if(inning>=10 && inning <12)
        {
            if(me.GetScore() != you.GetScore())
            {
                FinishGame();
                return true;
            }

            if(inning > 12)
            {
                FinishGame();
                return true;
            }

            return false;
        }

        FinishGame();
        return true;
    }

    private void FinishGame()
    {
        gameOver = true;

        if(gameOverPannel != null) gameOverPannel.SetActive(true);

        if(gameOverText != null)
        {
            if (me.GetScore() > you.GetScore()) 
                gameOverText.text = "승리!";
            else if (me.GetScore() < you.GetScore())
                gameOverText.text = "패배!";
            else
                gameOverText.text = "무승부!";
        }

        UpdateGameUI();
    }

    private void UpdateGameUI()
    {
        if(inningText != null)
        { 
            inningText.text = inning <= 9 ? inning + "회" : inning + "회(연장)";
        }
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "내 턴" : "상대 턴";
        }
        if (endTurnButton != null)
        {
            endTurnButton.interactable = isPlayerTurn && !gameOver;
        }
    }
}
