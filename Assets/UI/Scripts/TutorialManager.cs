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
    private bool isAdvancing = false;

    private void Start()
    {
        SetupSteps();
        StartCoroutine(BeginTutorial());
    }

    // ── 단계 정의 ─────────────────────────────────────

    private void SetupSteps()
    {
        var attackHand = new List<CardId>
        {
            CardId.Hit, CardId.Bunt, CardId.Steal,
            CardId.Double, CardId.Triple, CardId.HomeRun,
        };
        var defenseHand = new List<CardId>
        {
            CardId.GreatCatch, CardId.DoublePlay, CardId.TriplePlay,
            CardId.LookingStrikeOut, CardId.SwingStrikeOut,
        };
        var trapHand = new List<CardId>
        {
            CardId.Dazzle, CardId.BadBounce, CardId.Hit, CardId.Double,
        };
        var drawHand = new List<CardId>
        {
            CardId.PinchHitter, CardId.PinchRunner,
            CardId.PitcherChange, CardId.DefensiveSub,
        };
        var opponentCards = new List<CardId>
        {
            CardId.Hit, CardId.Double, CardId.Triple,
            CardId.HomeRun, CardId.GreatCatch, CardId.Steal,
        };

        steps = new TutorialStep[]
        {
            // ── 인트로 ─────────────────────────────────
            new TutorialStep
            {
                message = "안녕하세요!\n야구 카드 게임 튜토리얼입니다.\n카드를 드래그해 드롭존에 올리면 사용할 수 있어요.",
                actionType = TutorialActionType.WaitNext,
            },

            // ── 공격 카드 섹션 ───────────────────────────
            new TutorialStep
            {
                message = "[공격 카드 섹션]\n6종의 공격 카드를 배웁니다.\n카드를 드래그해 드롭존에 올려보세요.",
                actionType = TutorialActionType.WaitNext,
                sectionHand = attackHand,
                resetMyBases = true,
            },
            new TutorialStep
            {
                message = "안타 — 타자가 1루에 출루합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Hit,
                instruction = "안타 카드를 드래그해 드롭존에 올려보세요",
            },
            new TutorialStep
            {
                message = "번트 — 1루 주자를 2루로 이동합니다.\n(타자 출루 없음)",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Bunt,
                instruction = "번트 카드를 드래그해 드롭존에 올려보세요",
            },
            new TutorialStep
            {
                message = "도루 — 모든 주자가 1루씩 진루합니다.\n(타자 출루 없음)",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Steal,
                instruction = "도루 카드를 드래그해 드롭존에 올려보세요",
            },
            new TutorialStep
            {
                message = "2루타 — 모든 주자가 2루씩 진루합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Double,
                instruction = "2루타 카드를 드래그해 드롭존에 올려보세요",
            },
            new TutorialStep
            {
                message = "3루타 — 모든 주자가 3루씩 진루합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Triple,
                instruction = "3루타 카드를 드래그해 드롭존에 올려보세요",
            },
            new TutorialStep
            {
                message = "홈런 — 타자와 모든 주자가 홈으로 들어옵니다!",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.HomeRun,
                instruction = "홈런 카드를 드래그해 드롭존에 올려보세요",
            },

            // ── 수비 카드 섹션 ───────────────────────────
            new TutorialStep
            {
                message = "[수비 카드 섹션]\n수비 카드는 세트존에 놓아두면\n상대 공격 시 자동으로 발동합니다.",
                actionType = TutorialActionType.WaitNext,
                sectionHand = defenseHand,
                resetMyBases = true,
                resetOpponentState = true,
            },
            new TutorialStep
            {
                message = "호수비 — 상대 안타/2루타/3루타를 막고 아웃 +1",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.GreatCatch,
                instruction = "호수비를 세트존에 드래그해보세요\n(상대가 안타를 사용합니다)",
                simulateOpponentAttack = true,
                opponentAttackCard = CardId.Hit,
            },
            new TutorialStep
            {
                message = "호수비 발동! 상대 안타를 막았습니다. 아웃 +1\n\n더블플레이 — 1루 주자가 있을 때 상대 안타/2루타를 막고 아웃 +2",
                actionType = TutorialActionType.WaitNext,
                resetOpponentState = true,
                preSetOpponentRunner1 = true,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.DoublePlay,
                instruction = "더블플레이를 세트존에 드래그해보세요\n(1루 주자 있음, 상대가 안타를 사용합니다)",
                simulateOpponentAttack = true,
                opponentAttackCard = CardId.Hit,
            },
            new TutorialStep
            {
                message = "더블플레이 발동! 아웃 +2\n\n삼중살 — 주자 2명 이상일 때 상대 안타를 막고 아웃 +3",
                actionType = TutorialActionType.WaitNext,
                resetOpponentState = true,
                preSetOpponentRunner1 = true,
                preSetOpponentRunner2 = true,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.TriplePlay,
                instruction = "삼중살을 세트존에 드래그해보세요\n(주자 2명 있음, 상대가 안타를 사용합니다)",
                simulateOpponentAttack = true,
                opponentAttackCard = CardId.Hit,
            },
            new TutorialStep
            {
                message = "삼중살 발동! 아웃 +3\n\n루킹삼진 — 호수비와 동일한 효과 (아웃 +1)",
                actionType = TutorialActionType.WaitNext,
                resetOpponentState = true,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.LookingStrikeOut,
                instruction = "루킹삼진을 세트존에 드래그해보세요",
                simulateOpponentAttack = true,
                opponentAttackCard = CardId.Hit,
            },
            new TutorialStep
            {
                message = "루킹삼진 발동! 아웃 +1\n\n헛스윙삼진 — 루킹삼진과 동일한 효과",
                actionType = TutorialActionType.WaitNext,
                resetOpponentState = true,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.SwingStrikeOut,
                instruction = "헛스윙삼진을 세트존에 드래그해보세요",
                simulateOpponentAttack = true,
                opponentAttackCard = CardId.Hit,
            },
            new TutorialStep
            {
                message = "헛스윙삼진 발동! 아웃 +1\n수비 카드 5종 완료!",
                actionType = TutorialActionType.WaitNext,
            },

            // ── 함정 카드 섹션 ───────────────────────────
            new TutorialStep
            {
                message = "[함정 카드 섹션]\n함정 카드도 세트존에 놓으면\n조건 충족 시 자동으로 발동합니다.",
                actionType = TutorialActionType.WaitNext,
                sectionHand = trapHand,
                resetMyBases = true,
                resetOpponentState = true,
            },
            new TutorialStep
            {
                message = "눈부심 — 내 공격이 수비에 막히지 않으면\n모든 주자가 추가로 1루 진루합니다.",
                actionType = TutorialActionType.WaitNext,
                preSetMyRunner1 = true,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.Dazzle,
                instruction = "눈부심을 세트존에 드래그해보세요",
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Hit,
                instruction = "안타로 공격해보세요! (눈부심이 발동합니다)",
            },
            new TutorialStep
            {
                message = "눈부심 발동! 공격이 막히지 않아\n주자들이 추가로 1루 더 진루했습니다.\n\n불규칙 바운드 — 상대 수비 카드 발동을 취소합니다.",
                actionType = TutorialActionType.WaitNext,
                resetMyBases = true,
                preSetOpponentSetCard = true,
                opponentSetCard = CardId.GreatCatch,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragSetCard,
                requiredCard = CardId.BadBounce,
                instruction = "불규칙 바운드를 세트존에 드래그해보세요\n(상대 세트존에 호수비가 있습니다)",
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.Double,
                instruction = "2루타로 공격해보세요! (불규칙 바운드 발동)",
            },
            new TutorialStep
            {
                message = "불규칙 바운드 발동! 상대 호수비가 취소되어\n공격이 성공했습니다.\n함정 카드 2종 완료!",
                actionType = TutorialActionType.WaitNext,
            },

            // ── 드로우 카드 섹션 ─────────────────────────
            new TutorialStep
            {
                message = "[드로우 카드 섹션]\n사용하면 즉시 효과가 발동합니다.",
                actionType = TutorialActionType.WaitNext,
                sectionHand = drawHand,
                resetMyBases = true,
                resetOpponentState = true,
                opponentHand = opponentCards,
            },
            new TutorialStep
            {
                message = "대타 — 카드를 3장 드로우합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.PinchHitter,
                instruction = "대타 카드를 사용해보세요",
            },
            new TutorialStep
            {
                message = "카드 3장을 드로우했습니다!\n\n대주자 — 카드를 2장 드로우합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.PinchRunner,
                instruction = "대주자 카드를 사용해보세요",
            },
            new TutorialStep
            {
                message = "카드 2장을 드로우했습니다!\n\n투수교체 — 상대 손패에서 2장을 랜덤 제거합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.PitcherChange,
                instruction = "투수교체 카드를 사용해보세요",
            },
            new TutorialStep
            {
                message = "상대 손패 2장을 제거했습니다!\n\n대수비 — 상대 손패에서 1장을 랜덤 제거합니다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.DragAttackCard,
                requiredCard = CardId.DefensiveSub,
                instruction = "대수비 카드를 사용해보세요",
            },
            new TutorialStep
            {
                message = "상대 손패 1장을 제거했습니다!\n드로우 카드 4종 완료!\n\n모든 카드를 배웠습니다!",
                actionType = TutorialActionType.WaitNext,
            },

            // ── 마무리 ─────────────────────────────────
            new TutorialStep
            {
                message = "마지막으로 턴 종료 버튼을 눌러봅시다.",
                actionType = TutorialActionType.WaitNext,
            },
            new TutorialStep
            {
                actionType = TutorialActionType.PressEndTurn,
                instruction = "턴 종료 버튼을 눌러보세요",
            },
            new TutorialStep
            {
                message = "튜토리얼 완료!\n이제 실제 게임을 즐겨보세요!",
                actionType = TutorialActionType.WaitNext,
            },
        };
    }

    // ── 시작 ──────────────────────────────────────────

    private IEnumerator BeginTutorial()
    {
        yield return new WaitForSeconds(0.5f);
        game.StartTutorialAsFirst();
        // 손패 교체는 OnHalfInningCardsDrawn에서 처리
    }

    // SingleLaneGame의 StartHalfInningRoutine에서 드로우 직후 호출
    public void OnHalfInningCardsDrawn(SingleLanePlayer batter, bool isPlayerBatting)
    {
        if (!isPlayerBatting || tutorialReady) return;

        // 첫 단계의 sectionHand로 초기화
        batter.SetTutorialHand(new List<CardId>
        {
            CardId.Hit, CardId.Bunt, CardId.Steal,
            CardId.Double, CardId.Triple, CardId.HomeRun,
        });

        tutorialReady = true;
        StartCoroutine(StartTutorialSteps());
    }

    private IEnumerator StartTutorialSteps()
    {
        yield return null;
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
        ApplyStepPreSetup(step);

        if (step.actionType == TutorialActionType.WaitNext)
        {
            ui.Show(step.message, true);
            ui.HideArrow();
            ui.HideInstruction();
            game.SetButtonsPublic(false);
            ui.SetNextCallback(AdvanceStep);
            waitingForAction = false;
        }
        else
        {
            ui.Hide();
            waitingForAction = true;
            pendingAdvance = false;

            if (!string.IsNullOrEmpty(step.instruction))
                ui.ShowInstruction(step.instruction);

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

    private void ApplyStepPreSetup(TutorialStep step)
    {
        if (step.sectionHand != null && step.sectionHand.Count > 0)
            game.me.SetTutorialHand(step.sectionHand);

        if (step.resetMyBases)
            game.me.ResetBases();
        if (step.preSetMyRunner1 || step.preSetMyRunner2 || step.preSetMyRunner3)
            game.TutorialSetMyBases(step.preSetMyRunner1, step.preSetMyRunner2, step.preSetMyRunner3);

        if (step.resetOpponentState)
            game.TutorialResetOpponentState();
        if (step.preSetOpponentRunner1 || step.preSetOpponentRunner2 || step.preSetOpponentRunner3)
            game.TutorialSetOpponentBases(step.preSetOpponentRunner1, step.preSetOpponentRunner2, step.preSetOpponentRunner3);

        if (step.preSetOpponentSetCard)
            game.TutorialSetOpponentSetCard(step.opponentSetCard);

        if (step.opponentHand != null && step.opponentHand.Count > 0)
            game.TutorialSetOpponentHand(step.opponentHand);
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
        if (isAdvancing) return;
        isAdvancing = true;
        waitingForAction = false;
        pendingAdvance = false;
        currentStep++;
        StartCoroutine(NextStepDelay());
    }

    private IEnumerator NextStepDelay()
    {
        yield return new WaitForSeconds(0.3f);
        isAdvancing = false;
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
            ui.HideInstruction();
            return true;
        }

        if (step.actionType == TutorialActionType.DragSetCard)
        {
            if (!isDragSet || cardId != step.requiredCard) return false;
            ui.HideArrow();
            ui.HideInstruction();

            if (step.simulateOpponentAttack)
            {
                pendingAdvance = true;
                waitingForAction = false;
                game.TutorialStartOpponentAttackSimulation(step.opponentAttackCard);
            }
            else
            {
                AdvanceStep();
            }
            return true;
        }

        return false;
    }

    public bool ValidateEndTurn()
    {
        if (!tutorialReady) return false;
        if (!waitingForAction) return false;

        TutorialStep step = steps[currentStep];
        if (step.actionType != TutorialActionType.PressEndTurn) return false;

        ui.HideArrow();
        ui.HideInstruction();
        AdvanceStep();
        return true;
    }

    public void OnPlayerActionComplete()
    {
        if (!pendingAdvance) return;
        AdvanceStep();
    }

    public void OnOpponentAttackSimulated()
    {
        if (!pendingAdvance) return;
        AdvanceStep();
    }
}
