using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleLaneGame : MonoBehaviour
{
    public SingleLanePlayer me;
    public SingleLanePlayer you;

    [Header("게임 UI")]
    public Text inningText;
    public Text turnText;
    public Text resultText;
    public GameObject resultPanel;

    [Header("동전 선택 UI")]
    public GameObject coinPanel;
    public Text coinResultText;

    [Header("버튼")]
    public Button useCardButton;
    public Button endTurnButton;

    private int inning = 1;
    private bool isTop = true; // true = 초 / false = 말
    private bool gameOver = false;

    // true면 플레이어가 선공(초)
    private bool playerIsFirst = true;

    // 현재 공격 플레이어가 나인지
    private bool isPlayerBatting = true;

    private void Start()
    {
        gameOver = false;
        resultPanel.SetActive(false);

        me.Initialize(false);
        you.Initialize(true);

        if (coinPanel != null)
        {
            coinPanel.SetActive(true);
        }

        UpdateGameUI();
        SetButtons(false);
    }

    // 동전 앞면 선택
    public void ClickCoinHeads()
    {
        DecideBattingOrder(true);
    }

    // 동전 뒷면 선택
    public void ClickCoinTails()
    {
        DecideBattingOrder(false);
    }

    private void DecideBattingOrder(bool playerChoiceIsHeads)
    {
        bool coinIsHeads = Random.Range(0, 2) == 0;

        if (coinResultText != null)
        {
            coinResultText.text = coinIsHeads ? "동전 결과 : 앞면" : "동전 결과 : 뒷면";
        }

        bool playerWin = (playerChoiceIsHeads == coinIsHeads);

        // 이기면 선공, 지면 후공
        playerIsFirst = playerWin;

        inning = 1;
        isTop = true;

        SetCurrentBattingSide();

        if (coinPanel != null)
        {
            coinPanel.SetActive(false);
        }

        UpdateGameUI();
        StartHalfInning();
    }

    private void SetCurrentBattingSide()
    {
        if (isTop)
        {
            isPlayerBatting = playerIsFirst;
        }
        else
        {
            isPlayerBatting = !playerIsFirst;
        }
    }

    private void StartHalfInning()
    {
        if (gameOver) return;

        SingleLanePlayer batter = GetCurrentBatter();
        batter.ResetOutCount();
        batter.ResetBases();
        batter.DrawTurnCards();

        UpdateGameUI();

        if (isPlayerBatting)
        {
            SetButtons(true);
        }
        else
        {
            SetButtons(false);
            StartCoroutine(AITurnRoutine());
        }
    }

    public void ClickUseSelectedCard()
    {
        if (gameOver) return;
        if (!isPlayerBatting) return;
        if (!me.CheckSelectedCard()) return;

        StartCoroutine(ProcessCardUse(me));
    }

    public void ClickEndTurn()
    {
        if (gameOver) return;
        if (!isPlayerBatting) return;

        EndHalfInning();
    }

    private IEnumerator AITurnRoutine()
    {
        yield return new WaitForSeconds(0.7f);

        while (!gameOver && !isPlayerBatting)
        {
            SingleLanePlayer batter = GetCurrentBatter();

            if (batter.GetHandCount() <= 0 || batter.GetOutCount() >= 3)
            {
                EndHalfInning();
                yield break;
            }

            batter.AISelectCard();
            yield return StartCoroutine(ProcessCardUse(batter));
            yield return new WaitForSeconds(0.7f);
        }
    }

    private IEnumerator ProcessCardUse(SingleLanePlayer batter)
    {
        batter.MoveSelectedCardToBattleZone();
        yield return new WaitForSeconds(0.3f);

        int cardType = batter.UseSelectedCard();
        batter.ApplyBattingResult(cardType);

        UpdateGameUI();

        if (batter.GetHandCount() <= 0 || batter.GetOutCount() >= 3)
        {
            yield return new WaitForSeconds(0.5f);
            EndHalfInning();
        }
        else if (isPlayerBatting)
        {
            SetButtons(true);
        }
    }

    private void EndHalfInning()
    {
        if (gameOver) return;

        SetButtons(false);

        // 초 -> 말
        if (isTop)
        {
            isTop = false;
        }
        // 말 종료 -> 다음 이닝 초
        else
        {
            if (CheckWalkOff())
            {
                FinishGame();
                return;
            }

            inning++;
            isTop = true;
        }

        if (CheckGameOverByRule())
        {
            FinishGame();
            return;
        }

        SetCurrentBattingSide();
        UpdateGameUI();
        StartHalfInning();
    }

    private bool CheckWalkOff()
    {
        // 9회말 이후 말 공격에서 후공 팀이 앞서면 바로 끝
        if (inning < 9) return false;
        if (isTop) return false;

        if (playerIsFirst)
        {
            // 플레이어가 선공이면 AI가 후공
            return you.GetScore() > me.GetScore();
        }
        else
        {
            // 플레이어가 후공이면 플레이어가 후공
            return me.GetScore() > you.GetScore();
        }
    }

    private bool CheckGameOverByRule()
    {
        // 9회 이전 종료 없음
        if (inning < 9) return false;

        // 9회초 끝났다고 종료 아님. 말까지 가야 함.
        if (inning == 9 && isTop == false) return false;

        // 9회말 종료 후
        if (inning == 10 && isTop == true)
        {
            if (me.GetScore() != you.GetScore()) return true;
            return false;
        }

        // 연장 10~12회
        if (inning <= 12) return false;

        // 12회말 종료 후 다음 13회초로 넘어가려는 시점이면 종료
        return true;
    }

    private void FinishGame()
    {
        gameOver = true;
        SetButtons(false);

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            if (me.GetScore() > you.GetScore())
            {
                resultText.text = "승리!";
            }
            else if (me.GetScore() < you.GetScore())
            {
                resultText.text = "패배!";
            }
            else
            {
                resultText.text = "무승부!";
            }
        }

        UpdateGameUI();
    }

    private SingleLanePlayer GetCurrentBatter()
    {
        return isPlayerBatting ? me : you;
    }

    private void UpdateGameUI()
    {
        if (inningText != null)
        {
            string half = isTop ? "초" : "말";
            inningText.text = inning + "회" + half;
        }

        if (turnText != null)
        {
            turnText.text = isPlayerBatting ? "내 공격" : "상대 공격";
        }
    }

    private void SetButtons(bool active)
    {
        if (useCardButton != null)
            useCardButton.interactable = active;

        if (endTurnButton != null)
            endTurnButton.interactable = active;
    }
}