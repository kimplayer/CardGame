using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// 게임 전체 흐름(이닝, 초말, 동전 선택, 턴 진행, 게임 종료)을 관리하는 스크립트

public class SingleLaneGame : MonoBehaviour
{
    public SingleLanePlayer me;
    public SingleLanePlayer you;

    public Text inningText;
    public Text turnText;
    public Text resultText;
    public GameObject resultPanel;

    public GameObject coinPanel;
    public Text coinResultText;

    public Button useCardButton;
    public Button setCardButton;
    public Button endTurnButton;

    public ScrollRect logScrollRect; // 스크롤 로그창
    public Text logText;             // Content 안의 로그 텍스트

    private int inning = 1;
    private bool isTop = true;
    private bool gameOver = false;

    private bool playerIsFirst = true;
    private bool isPlayerBatting = true;

    private List<string> allLogs = new List<string>();
    private const int MAX_LOG_LINES = 100; // 최대 로그 줄 수

    // 게임 시작
    private void Start()
    {
        gameOver = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        me.Initialize(false);
        you.Initialize(true);

        if (coinPanel != null)
            coinPanel.SetActive(true);

        WriteLog("게임 시작. 동전 앞/뒤를 선택하세요.");
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

    // 선공/후공 결정
    private void DecideBattingOrder(bool playerChoiceIsHeads)
    {
        bool coinIsHeads = Random.Range(0, 2) == 0;

        if (coinResultText != null)
            coinResultText.text = coinIsHeads ? "동전 결과 : 앞면" : "동전 결과 : 뒷면";

        bool playerWin = (playerChoiceIsHeads == coinIsHeads);
        playerIsFirst = playerWin;

        inning = 1;
        isTop = true;

        SetCurrentBattingSide();

        if (coinPanel != null)
            coinPanel.SetActive(false);

        if (playerIsFirst)
            WriteLog("동전 승리! 선공입니다.");
        else
            WriteLog("동전 패배! 후공입니다.");

        StartHalfInning();
    }

    // 현재 초/말에 따라 공격 주체 설정
    private void SetCurrentBattingSide()
    {
        if (isTop)
            isPlayerBatting = playerIsFirst;
        else
            isPlayerBatting = !playerIsFirst;
    }

    // 반이닝 시작
    private void StartHalfInning()
    {
        if (gameOver) return;

        SingleLanePlayer batter = GetCurrentBatter();
        batter.ResetOutCount();
        batter.ResetBases();

        List<string> discarded = batter.DrawTurnCards();

        string sideText = isTop ? "초" : "말";
        string batterText = isPlayerBatting ? "내 공격" : "상대 공격";

        AddInningDivider($"{inning}회{sideText}");
        WriteLog($"{inning}회{sideText} 시작 - {batterText}");

        if (discarded.Count > 0)
        {
            if (isPlayerBatting)
                WriteLog("손패 초과로 랜덤 버림: " + string.Join(", ", discarded));
            else
                WriteLog("상대 손패 초과로 카드가 자동 정리되었다.");
        }

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

    // 플레이어 카드 사용 버튼
    public void ClickUseSelectedCard()
    {
        if (gameOver || !isPlayerBatting) return;
        if (!me.CheckSelectedCard()) return;

        CardId selectedId = me.GetSelectedCardId();
        CardCategory category = me.GetCardCategory(selectedId);

        if (category == CardCategory.Defense || category == CardCategory.Trap)
        {
            WriteLog("수비/함정 카드는 세트해야 합니다.");
            return;
        }

        StartCoroutine(ProcessUseCard(me, you, true));
    }

    // 플레이어 카드 세트 버튼
    public void ClickSetSelectedCard()
    {
        if (gameOver || !isPlayerBatting) return;
        if (!me.CheckSelectedCard()) return;

        CardId selectedId = me.GetSelectedCardId();
        CardCategory category = me.GetCardCategory(selectedId);

        if (category == CardCategory.Defense || category == CardCategory.Trap)
        {
            me.SetSelectedCard();
            WriteLog($"플레이어가 {me.GetCardName(selectedId)} 카드를 세트했다.");
        }
        else
        {
            WriteLog("이 카드는 바로 사용 카드입니다.");
        }
    }

    // 플레이어 턴 종료 버튼
    public void ClickEndTurn()
    {
        if (gameOver || !isPlayerBatting) return;

        WriteLog("플레이어가 턴을 종료했다.");
        EndHalfInning();
    }

    // AI 턴 루틴
    private IEnumerator AITurnRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        while (!gameOver && !isPlayerBatting)
        {
            SingleLanePlayer batter = GetCurrentBatter();

            if (batter.GetHandCount() <= 0 || batter.GetOutCount() >= 3)
            {
                WriteLog("상대 턴 종료.");
                EndHalfInning();
                yield break;
            }

            batter.AISelectPlayableCard(true);

            CardId selected = batter.GetSelectedCardId();
            CardCategory category = batter.GetCardCategory(selected);

            if (category == CardCategory.Defense || category == CardCategory.Trap)
            {
                batter.SetSelectedCard();
                WriteLog("상대가 세트 카드를 1장 배치했다.");
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return StartCoroutine(ProcessUseCard(batter, me, false));
                yield return new WaitForSeconds(0.7f);
            }
        }
    }

    // 카드 사용 처리
    private IEnumerator ProcessUseCard(SingleLanePlayer attacker, SingleLanePlayer defender, bool isPlayerAction)
    {
        yield return new WaitForSeconds(0.2f);

        CardId cardId = attacker.UseSelectedCard();
        CardCategory category = attacker.GetCardCategory(cardId);

        string actor = isPlayerAction ? "플레이어" : "상대";
        string cardName = attacker.GetCardName(cardId);

        if (category == CardCategory.Attack)
        {
            WriteLog($"{actor}가 {cardName} 카드를 사용했다.");

            bool blocked = defender.TryActivateDefenseOrTrap(cardId, attacker, out string activatedName);

            if (!blocked)
            {
                attacker.ApplyAttackCard(cardId);

                if (activatedName == "눈부심")
                    WriteLog($"{actor}의 공격 성공! 눈부심 발동으로 추가 진루.");
                else
                    WriteLog($"{actor}의 {cardName} 효과가 적용되었다.");
            }
            else
            {
                WriteLog($"수비 발동! {activatedName} 때문에 {actor}의 {cardName} 카드가 취소되었다.");
            }
        }
        else if (category == CardCategory.Draw)
        {
            WriteLog($"{actor}가 {cardName} 카드를 사용했다.");
            string result = attacker.ApplyDrawCard(cardId, defender);
            WriteLog(result);
        }

        UpdateGameUI();

        if (attacker.GetHandCount() <= 0 || attacker.GetOutCount() >= 3)
        {
            yield return new WaitForSeconds(0.5f);
            WriteLog($"{actor}의 반이닝 종료.");
            EndHalfInning();
        }
        else if (isPlayerBatting)
        {
            SetButtons(true);
        }
    }

    // 반이닝 종료
    private void EndHalfInning()
    {
        if (gameOver) return;

        SetButtons(false);

        if (isTop)
        {
            isTop = false;
        }
        else
        {
            if (CheckWalkOff())
            {
                WriteLog("끝내기 발생!");
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

    // 끝내기 판정
    private bool CheckWalkOff()
    {
        if (inning < 9) return false;
        if (isTop) return false;

        if (playerIsFirst)
            return you.GetScore() > me.GetScore();
        else
            return me.GetScore() > you.GetScore();
    }

    // 경기 종료 규칙 판정
    private bool CheckGameOverByRule()
    {
        if (inning < 9) return false;
        if (inning == 9 && isTop == false) return false;

        if (inning == 10 && isTop == true)
        {
            if (me.GetScore() != you.GetScore()) return true;
            return false;
        }

        if (inning <= 12) return false;
        return true;
    }

    // 경기 종료 처리
    private void FinishGame()
    {
        gameOver = true;
        SetButtons(false);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            if (me.GetScore() > you.GetScore())
            {
                resultText.text = "승리!";
                WriteLog("게임 종료 - 플레이어 승리!");
            }
            else if (me.GetScore() < you.GetScore())
            {
                resultText.text = "패배!";
                WriteLog("게임 종료 - 플레이어 패배!");
            }
            else
            {
                resultText.text = "무승부!";
                WriteLog("게임 종료 - 무승부!");
            }
        }

        UpdateGameUI();
    }

    // 현재 공격 플레이어 반환
    private SingleLanePlayer GetCurrentBatter()
    {
        return isPlayerBatting ? me : you;
    }

    // UI 갱신
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

    // 버튼 활성화/비활성화
    private void SetButtons(bool active)
    {
        if (useCardButton != null) useCardButton.interactable = active;
        if (setCardButton != null) setCardButton.interactable = active;
        if (endTurnButton != null) endTurnButton.interactable = active;
    }

    // 이닝 구분선 추가
    private void AddInningDivider(string inningLabel)
    {
        WriteLog($"========= {inningLabel} =========");
    }

    // 로그 추가 + 100줄 초과 시 오래된 로그 삭제
    private void WriteLog(string message)
    {
        allLogs.Add(message);

        while (allLogs.Count > MAX_LOG_LINES)
            allLogs.RemoveAt(0);

        if (logText != null)
            logText.text = string.Join("\n", allLogs);

        Debug.Log(message);

        StartCoroutine(ScrollLogToBottom());
    }

    // 로그 추가 후 스크롤을 맨 아래로 이동
    private IEnumerator ScrollLogToBottom()
    {
        yield return null;
        yield return null;

        if (logScrollRect != null)
            logScrollRect.verticalNormalizedPosition = 0f;
    }
}