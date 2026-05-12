using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10)]
public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public SingleLaneGame game;
    public TutorialUI ui;

    [Header("튜토리얼 종료 후 이동할 씬")]
    public string nextSceneName = "DeckBuild";

    private TutorialStep[] steps;
    private int currentStep = 0;
    private bool tutorialReady = false;
    private bool waitingForAction = false;
    private bool pendingAdvance = false;

    private void Start()
    {
        SetupSteps();
        StartCoroutine(BeginTutorial());
    }

    // ── 단계 정의 ─────────────────────────────────────

    private void SetupSteps()
    {
        steps = new TutorialStep[]
        {
            // ── 인트로 ─────────────────────────────────
            new TutorialStep
            {
                message = "안녕하세요!\n야구 카드 게임 튜토리얼입니다.\n카드를 드래그해 드롭존에 올리면 사용할 수 있어요.",
                actionType = TutorialActionType.WaitNext
            },

            // ── 공격 카드 ───────────────────────────────
            new TutorialStep
            {
                message = "[공격 카드]\n안타/2루타/3루타/홈런은 타자와 주자를 진루시킵니다.\n도루는 타자 없이 주자만 1루 진루,\n번트는 1루 주자를 2루로 이동합니다.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "안타 카드를 사용해보세요!\n타자가 1루에 출루합니다.",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Hit
            },
            new TutorialStep
            {
                message = "1루에 주자가 생겼어요!\n번트로 1루 주자를 2루로 보내봅시다.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "번트 카드를 사용해보세요!\n1루 주자가 2루로 이동합니다. (타자는 출루 없음)",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Bunt
            },
            new TutorialStep
            {
                message = "주자가 2루로 이동했어요!\n이번엔 도루로 3루까지 보내봅시다.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "도루 카드를 사용해보세요!\n모든 주자가 1루씩 진루합니다. (타자는 출루 없음)",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Steal
            },
            new TutorialStep
            {
                message = "주자가 3루에 있어요!\n2루타를 치면 3루 주자가 홈으로 들어와 득점합니다.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "2루타 카드를 사용해보세요!\n모든 주자가 2루씩 진루합니다.",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Double
            },
            new TutorialStep
            {
                message = "득점! 3루타는 3루씩, 홈런은 타자 포함 전원 홈으로 들어옵니다.\n이제 홈런을 쳐봅시다!",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "홈런 카드를 사용해보세요!\n모든 주자와 타자가 홈으로 들어옵니다.",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.HomeRun
            },
            new TutorialStep
            {
                message = "홈런! 모두 득점했습니다!\n공격 카드 6종을 모두 배웠어요.",
                actionType = TutorialActionType.WaitNext
            },

            // ── 수비 카드 ───────────────────────────────
            new TutorialStep
            {
                message = "[수비 카드]\n수비 카드는 드래그하면 세트존에 배치됩니다.\n상대가 공격 카드를 사용할 때 자동으로 발동해요.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "호수비 카드를 세트해보세요!\n상대 안타/2루타/3루타를 막고 아웃 +1 합니다.",
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.GreatCatch
            },
            new TutorialStep
            {
                message = "더블플레이는 주자가 있을 때 상대 안타/2루타를 막고 아웃 +2,\n삼중살은 주자 2명 이상일 때 안타를 막고 아웃 +3 합니다.\n루킹삼진/헛스윙삼진도 호수비와 효과가 같아요.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "더블플레이 카드를 세트해보세요!",
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.DoublePlay
            },

            // ── 트랩 카드 ───────────────────────────────
            new TutorialStep
            {
                message = "[트랩 카드]\n눈부심: 내 공격이 수비에 막히지 않으면 모든 주자 추가 1루 진루\n불규칙 바운드: 상대 수비 카드 발동을 취소합니다.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "눈부심 카드를 세트해보세요!",
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.Dazzle
            },

            // ── 드로우 카드 ─────────────────────────────
            new TutorialStep
            {
                message = "[드로우 카드]\n대타: 카드 3장 드로우\n대주자: 카드 2장 드로우\n투수교체: 상대 패 2장 제거\n대수비: 상대 패 1장 제거",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "대타 카드를 사용해보세요!\n카드를 3장 드로우합니다.",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.PinchHitter
            },
            new TutorialStep
            {
                message = "카드 3장을 드로우했어요!\n이번엔 투수교체로 상대 패를 줄여봅시다.",
                actionType = TutorialActionType.WaitNext
            },
            new TutorialStep
            {
                message = "투수교체 카드를 사용해보세요!\n상대 손패에서 2장을 랜덤으로 제거합니다.",
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.PitcherChange
            },
            new TutorialStep
            {
                message = "상대 패를 제거했어요!\n이제 모든 기본 카드를 배웠습니다.",
                actionType = TutorialActionType.WaitNext
            },

            // ── 마무리 ─────────────────────────────────
            new TutorialStep
            {
                message = "턴 종료 버튼을 눌러보세요.\n공격/수비를 마쳤으면 턴을 넘깁니다.",
                actionType = TutorialActionType.PressEndTurn
            },
            new TutorialStep
            {
                message = "튜토리얼 완료!\n이제 실제 게임을 즐겨보세요!",
                actionType = TutorialActionType.WaitNext
            },
        };
    }

    // ── 시작 ──────────────────────────────────────────

    private IEnumerator BeginTutorial()
    {
        yield return new WaitForSeconds(0.5f);
        game.ClickCoinHeads();
        // 손패 교체는 OnHalfInningCardsDrawn에서 처리
    }

    // SingleLaneGame의 StartHalfInningRoutine에서 드로우 직후 호출
    public void OnHalfInningCardsDrawn(SingleLanePlayer batter, bool isPlayerBatting)
    {
        if (!isPlayerBatting || tutorialReady) return;

        batter.SetTutorialHand(new List<CardId>
        {
            CardId.Hit, CardId.Bunt, CardId.Steal, CardId.Double, CardId.HomeRun,
            CardId.GreatCatch, CardId.DoublePlay, CardId.Dazzle,
            CardId.PinchHitter, CardId.PitcherChange,
        });

        tutorialReady = true;
        StartCoroutine(StartTutorialSteps());
    }

    private IEnumerator StartTutorialSteps()
    {
        yield return null; // StartHalfInningRoutine 완료 대기
        RunStep(currentStep);
    }

    // ── 단계 실행 ─────────────────────────────────────

    private void RunStep(int index)
    {
        if (index >= steps.Length)
        {
            FinishTutorial();
            return;
        }

        TutorialStep step = steps[index];
        bool showNext = step.actionType == TutorialActionType.WaitNext;

        ui.Show(step.message, showNext);

        if (showNext)
        {
            ui.HideArrow();
            game.SetButtonsPublic(false);
            ui.SetNextCallback(AdvanceStep);
            waitingForAction = false;
        }
        else
        {
            waitingForAction = true;
            pendingAdvance = false;

            if (step.actionType == TutorialActionType.DragAttackCard ||
                step.actionType == TutorialActionType.DragSetCard)
            {
                ui.ShowArrow(() => FindCardTransform(step.requiredCard));
            }
            else if (step.actionType == TutorialActionType.PressEndTurn)
            {
                game.SetButtonsPublic(true);
                ui.ShowArrow(() => game.endTurnButton?.transform);
            }
        }
    }

    private Transform FindCardTransform(CardId cardId)
    {
        foreach (Transform child in game.me.transform)
        {
            if (!child.name.StartsWith("Card_")) continue;
            Card card = child.GetComponent<Card>();
            if (card != null && card.cardId == cardId)
                return child;
        }
        return null;
    }

    private void AdvanceStep()
    {
        waitingForAction = false;
        pendingAdvance = false;
        currentStep++;
        StartCoroutine(NextStepDelay());
    }

    private IEnumerator NextStepDelay()
    {
        yield return new WaitForSeconds(0.3f);
        RunStep(currentStep);
    }

    private void FinishTutorial()
    {
        ui.Hide();
        if (DeckData.Instance != null)
            DeckData.Instance.useCustomDeck = false;
        SceneManager.LoadScene(nextSceneName);
    }

    // ── SingleLaneGame에서 호출 ───────────────────────

    public bool ValidateAction(CardId cardId, CardCategory category)
    {
        if (!tutorialReady || !waitingForAction || pendingAdvance) return false;

        TutorialStep step = steps[currentStep];
        bool isDragSet = category == CardCategory.Defense || category == CardCategory.Trap;

        if (step.actionType == TutorialActionType.DragAttackCard)
        {
            if (isDragSet || cardId != step.requiredCard) return false;
            pendingAdvance = true;
            ui.HideArrow();
            return true;
        }

        if (step.actionType == TutorialActionType.DragSetCard)
        {
            if (!isDragSet || cardId != step.requiredCard) return false;
            ui.HideArrow();
            AdvanceStep();
            return true;
        }

        return false;
    }

    public bool ValidateEndTurn()
    {
        if (!tutorialReady)      { Debug.Log("[Tutorial] ValidateEndTurn 실패: tutorialReady=false"); return false; }
        if (!waitingForAction)   { Debug.Log("[Tutorial] ValidateEndTurn 실패: waitingForAction=false"); return false; }

        TutorialStep step = steps[currentStep];
        if (step.actionType != TutorialActionType.PressEndTurn)
        {
            Debug.Log($"[Tutorial] ValidateEndTurn 실패: 현재 단계={step.actionType}");
            return false;
        }

        ui.HideArrow();
        AdvanceStep();
        return true;
    }

    public void OnPlayerActionComplete()
    {
        if (!pendingAdvance) return;
        AdvanceStep();
    }
}
