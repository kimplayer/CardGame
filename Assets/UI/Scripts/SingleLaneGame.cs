using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleLaneGame : MonoBehaviour
{
    public SingleLanePlayer me;
    public SingleLanePlayer you;
    public Text resultText;
    public GameObject resultPanel;

    public GameObject coinPanel;
    public Text coinResultText;

    public Button endTurnButton;

    public ScrollRect logScrollRect;
    public Text logText;

    private int inning = 1;
    private bool isTop = true;
    private bool gameOver = false;
    private bool playerIsFirst = true;
    private bool isPlayerBatting = true;

    private MCTSAgent mctsAgent;

    private List<string> allLogs = new List<string>();
    private const int MAX_LOG_LINES = 100;

    [Header("숨길 UI 목록")]
    public GameObject[] hideOnCoinPanel; // Inspector에서 숨길 오브젝트들 연결

    [Header("스코어보드")]
    public ScoreBoard scoreBoard;

    [Header("재시작 버튼")]
    public Button restartButton;   // 덱 다시 고르기
    public Button playAgainButton; // 같은 덱으로 시작


    private void Awake()
    {
        // 가로 방향 강제 고정
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    private void Start()
    {
        mctsAgent = new MCTSAgent(iterations: 50, samplingCount: 20);

        gameOver = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        // 재시작 버튼 연결
        if (restartButton != null)
            restartButton.onClick.AddListener(ClickRestart);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(ClickPlayAgain);

        me.Initialize(false);
        you.Initialize(true);

        if (coinPanel != null)
            coinPanel.SetActive(true);

        SetHideUI(false);
        UpdateFieldUIVisibility();
        WriteLog("게임 시작. 동전 앞/뒤를 선택하세요.");
        UpdateGameUI();
        SetButtons(false);
    }

    public void ClickCoinHeads()
    {
        DecideBattingOrder(true);
    }

    public void ClickCoinTails()
    {
        DecideBattingOrder(false);
    }

    private void DecideBattingOrder(bool playerChoiceIsHeads)
    {
        bool coinIsHeads = Random.Range(0, 2) == 0;

        if (coinResultText != null)
            coinResultText.text = coinIsHeads ? "동전 결과 : 앞면" : "동전 결과 : 뒷면";

        playerIsFirst = (playerChoiceIsHeads == coinIsHeads);

        inning = 1;
        isTop = true;

        SetCurrentBattingSide();

        if (coinPanel != null)
            coinPanel.SetActive(false);

        SetHideUI(true);

        // 동전 결과 확정 후 스코어보드 초기화
        // 선공 여부를 알고 나서 초기화해야 행 순서가 맞음
        if (scoreBoard != null)
            scoreBoard.InitScoreBoard(playerIsFirst);

        UpdateFieldUIVisibility();

        WriteLog(playerIsFirst ? "동전 승리! 선공입니다." : "동전 패배! 후공입니다.");

        StartHalfInning();
    }

    private void SetCurrentBattingSide()
    {
        isPlayerBatting = isTop ? playerIsFirst : !playerIsFirst;
    }

    private void StartHalfInning()
    {
        if (gameOver) return;
        StartCoroutine(StartHalfInningRoutine());
    }

    private IEnumerator StartHalfInningRoutine()
    {
        if (gameOver) yield break;

        SingleLanePlayer batter = GetCurrentBatter();
        batter.ResetOutCount();
        batter.ResetBases();

        // 드로우 전 한 프레임 대기
        yield return null;

        // 카드 드로우
        List<string> discarded = batter.DrawTurnCards();

        // 드로우 후 UI 갱신 대기
        yield return null;

        UpdateFieldUIVisibility();

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

        // 스코어보드 갱신
        if (scoreBoard != null)
            scoreBoard.OnHalfInningStart(inning, isTop, isPlayerBatting);

        // 드로우 후 손패 확인
        if (batter.GetHandCount() <= 0)
        {
            WriteLog("손패가 없어 반이닝 종료.");
            EndHalfInning();
            yield break;
        }

        if (isPlayerBatting)
            SetButtons(true);
        else
        {
            SetButtons(false);
            // AI 턴도 약간 대기 후 시작
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(AITurnRoutine());
        }
    }

    // 점수 변경 시 스코어보드 갱신
    public void RefreshScoreBoard()
    {
        if (scoreBoard == null) return;

        scoreBoard.OnScoreChanged(
            inning, isTop,
            me.GetScore(),
            you.GetScore(),
            me.GetScore(),
            you.GetScore()
        );
    }

    // 드래그앤드랍으로 카드 드랍됐을 때
    public bool OnCardDropped(string cardKey, CardCategory category)
    {
        // 상대 턴이거나 게임 종료면 실패 반환
        if (gameOver || !isPlayerBatting) return false;

        me.SetSelectedCardByKey(ParseKey(cardKey));

        if (category == CardCategory.Defense || category == CardCategory.Trap)
        {
            CardId selectedId = me.GetSelectedCardId();
            me.SetSelectedCard();
            WriteLog($"플레이어가 {me.GetCardName(selectedId)} 카드를 세트했다.");
        }
        else
        {
            SetButtons(false);
            StartCoroutine(ProcessUseCard(me, you, true));
        }

        return true; // 성공 반환
    }

    // 턴 종료 버튼
    public void ClickEndTurn()
    {
        if (gameOver || !isPlayerBatting) return;

        WriteLog("플레이어가 턴을 종료했다.");
        EndHalfInning();
    }

    // AI 턴 루틴
    private IEnumerator AITurnRoutine()
    {
        // 시작 전 대기 (드로우 완료 보장)
        yield return new WaitForSeconds(0.8f);

        // 손패 다시 확인
        SingleLanePlayer batter = GetCurrentBatter();

        if (batter.GetHandCount() <= 0)
        {
            WriteLog("상대 손패 없음. 반이닝 종료.");
            EndHalfInning();
            yield break;
        }

        while (!gameOver && !isPlayerBatting)
        {
            batter = GetCurrentBatter();

            if (batter.GetHandCount() <= 0 || batter.GetOutCount() >= 3)
            {
                WriteLog("상대 턴 종료.");
                EndHalfInning();
                yield break;
            }

            GameState currentState = BuildGameState(batter, me);
            MCTSAction action = mctsAgent.GetBestAction(currentState);

            if (action.actionType == MCTSActionType.EndTurn)
            {
                WriteLog("상대가 턴을 종료했다.");
                EndHalfInning();
                yield break;
            }

            ApplyMCTSAction(batter, action);

            yield return new WaitForSeconds(0.7f);
        }
    }

    private GameState BuildGameState(SingleLanePlayer attacker,
                                     SingleLanePlayer defender)
    {
        GameState state = new GameState();

        foreach (var pair in attacker.GetHandCardDict())
            state.attackerHand.Add(pair.Value);

        foreach (var pair in attacker.GetSetCardDict())
            state.attackerSetZone.Add(pair.Value);

        state.attackerFirst = attacker.HasRunnerOnFirst();
        state.attackerSecond = attacker.HasRunnerOnSecond();
        state.attackerThird = attacker.HasRunnerOnThird();
        state.attackerOut = attacker.GetOutCount();
        state.attackerScore = attacker.GetScore();

        foreach (var pair in defender.GetSetCardDict())
            state.defenderSetZone.Add(pair.Value);

        state.defenderScore = defender.GetScore();

        return state;
    }

    private void ApplyMCTSAction(SingleLanePlayer batter, MCTSAction action)
    {
        List<int> keys = new List<int>(batter.GetHandCardDict().Keys);
        if (action.handIndex < 0 || action.handIndex >= keys.Count) return;

        int key = keys[action.handIndex];
        batter.SetSelectedCardByKey(key);

        if (action.actionType == MCTSActionType.SetCard)
        {
            batter.SetSelectedCard();
            WriteLog("상대가 세트 카드를 1장 배치했다.");
        }
        else
        {
            StartCoroutine(ProcessUseCard(batter, me, false));
        }
    }

    private IEnumerator ProcessUseCard(SingleLanePlayer attacker,
                                   SingleLanePlayer defender,
                                   bool isPlayerAction)
    {
        yield return new WaitForSeconds(0.5f);

        CardId cardId = attacker.UseSelectedCard();
        CardCategory category = attacker.GetCardCategory(cardId);

        string actor = isPlayerAction ? "플레이어" : "상대";
        string cardName = attacker.GetCardName(cardId);

        if (category == CardCategory.Attack)
        {
            WriteLog($"{actor}가 {cardName} 카드를 사용했다.");

            bool blocked = defender.TryActivateDefenseOrTrap(
                               cardId, attacker, out string activatedName);

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
                WriteLog($"수비 발동! {activatedName} 때문에 " +
                         $"{actor}의 {cardName} 카드가 취소되었다.");
            }
        }
        else if (category == CardCategory.Draw)
        {
            WriteLog($"{actor}가 {cardName} 카드를 사용했다.");
            string result = attacker.ApplyDrawCard(cardId, defender);
            WriteLog(result);
        }

        UpdateGameUI();

        if (scoreBoard != null)
            scoreBoard.OnScoreChanged(
                inning, isTop,
                me.GetScore(), you.GetScore(),
                me.GetScore(), you.GetScore()
            );

        // 핵심 수정 부분
        if (attacker.GetOutCount() >= 3)
        {
            yield return new WaitForSeconds(0.5f);
            WriteLog($"{actor}의 반이닝 종료. (3아웃)");
            EndHalfInning();
            yield break;
        }

        if (attacker.GetHandCount() <= 0)
        {
            yield return new WaitForSeconds(0.5f);
            WriteLog($"{actor}의 반이닝 종료. (손패 없음)");
            EndHalfInning();
            yield break;
        }

        // 플레이어 턴이면 버튼 활성화
        if (isPlayerAction)
            SetButtons(true);
        // AI 턴이면 AITurnRoutine의 while이 처리
    }

    private bool isEndingHalfInning = false;

    private void EndHalfInning()
    {
        if (gameOver) return;

        // 이미 종료 처리 중이면 무시
        if (isEndingHalfInning) return;
        isEndingHalfInning = true;

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
                isEndingHalfInning = false;
                FinishGame();
                return;
            }

            inning++;
            isTop = true;
        }

        if (CheckGameOverByRule())
        {
            isEndingHalfInning = false;
            FinishGame();
            return;
        }

        SetCurrentBattingSide();
        UpdateFieldUIVisibility();
        UpdateGameUI();

        // 스코어보드 갱신
        if (scoreBoard != null)
            scoreBoard.OnHalfInningEnd(
                inning, isTop,
                me.GetScore(),
                you.GetScore()
            );

        isEndingHalfInning = false;
        StartHalfInning();
    }

    private bool CheckWalkOff()
    {
        if (inning < 9) return false;
        if (isTop) return false;

        return playerIsFirst
            ? you.GetScore() > me.GetScore()
            : me.GetScore() > you.GetScore();
    }

    private bool CheckGameOverByRule()
    {
        if (inning < 9) return false;
        if (inning == 9 && !isTop) return false;

        if (inning == 10 && isTop)
            return me.GetScore() != you.GetScore();

        if (inning <= 12) return false;
        return true;
    }

    private void FinishGame()
    {
        gameOver = true;
        SetButtons(false);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        // 재시작 버튼 활성화
        if (restartButton != null) restartButton.gameObject.SetActive(true);
        if (playAgainButton != null) playAgainButton.gameObject.SetActive(true);

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

    private SingleLanePlayer GetCurrentBatter()
    {
        return isPlayerBatting ? me : you;
    }

    private void UpdateGameUI()
    {
        
    }

    private void SetButtons(bool active)
    {
        if (endTurnButton != null)
            endTurnButton.interactable = active;
    }

    private void AddInningDivider(string inningLabel)
    {
        WriteLog($"========= {inningLabel} =========");
    }

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

    private IEnumerator ScrollLogToBottom()
    {
        // Canvas 레이아웃 재계산 강제 실행
        if (logText != null)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
                logText.rectTransform);

        // 3프레임 대기 (레이아웃 완전히 반영될 때까지)
        yield return null;
        yield return null;
        yield return null;

        if (logScrollRect != null)
            logScrollRect.verticalNormalizedPosition = 0f;
    }

    private void UpdateFieldUIVisibility()
    {
        me.SetFieldUIVisible(isPlayerBatting);
        you.SetFieldUIVisible(!isPlayerBatting);
    }

    private int ParseKey(string cardKey)
    {
        string[] split = cardKey.Split('_');
        int key = 0;
        if (split.Length >= 2)
            int.TryParse(split[1], out key);
        return key;
    }
    private void SetHideUI(bool visible)
    {
        if (hideOnCoinPanel == null) return;
        foreach (GameObject obj in hideOnCoinPanel)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

    // 덱 다시 고르기 → DeckBuild 씬으로
    public void ClickRestart()
    {
        // 커스텀 덱 초기화
        if (DeckData.Instance != null)
        {
            DeckData.Instance.playerDeck.Clear();
            DeckData.Instance.useCustomDeck = false;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuild");
    }

    // 같은 덱으로 바로 시작 → SingleLane 씬 재시작
    public void ClickPlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager
            .LoadScene("SingleLane");
    }
}