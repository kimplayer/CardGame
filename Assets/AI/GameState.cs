using System.Collections.Generic;
using UnityEngine;

// MCTS 시뮬레이션에서 사용할 게임 상태 스냅샷
// MonoBehaviour 없이 순수 데이터 클래스
public class GameState
{
    // 공격자 상태
    public List<CardId> attackerHand;
    public List<CardId> attackerSetZone;
    public List<CardId> attackerDiscard;
    public bool attackerFirst;
    public bool attackerSecond;
    public bool attackerThird;
    public int attackerOut;
    public int attackerScore;

    // 수비자 상태
    public List<CardId> defenderHand;
    public List<CardId> defenderSetZone;
    public List<CardId> defenderDiscard;
    public int defenderScore;

    // 게임 진행 상태
    public int inning;
    public bool isTop;
    public bool isPlayerBatting;

    // 깊은 복사 생성자
    public GameState(GameState other)
    {
        attackerHand = new List<CardId>(other.attackerHand);
        attackerSetZone = new List<CardId>(other.attackerSetZone);
        attackerDiscard = new List<CardId>(other.attackerDiscard);
        attackerFirst = other.attackerFirst;
        attackerSecond = other.attackerSecond;
        attackerThird = other.attackerThird;
        attackerOut = other.attackerOut;
        attackerScore = other.attackerScore;

        defenderHand = new List<CardId>(other.defenderHand);
        defenderSetZone = new List<CardId>(other.defenderSetZone);
        defenderDiscard = new List<CardId>(other.defenderDiscard);
        defenderScore = other.defenderScore;

        inning = other.inning;
        isTop = other.isTop;
        isPlayerBatting = other.isPlayerBatting;
    }

    // 기본 생성자
    public GameState()
    {
        attackerHand = new List<CardId>();
        attackerSetZone = new List<CardId>();
        attackerDiscard = new List<CardId>();
        defenderHand = new List<CardId>();
        defenderSetZone = new List<CardId>();
        defenderDiscard = new List<CardId>();
    }

    // 신경망 입력용 float 배열 변환 (총 60차원)
    public float[] ToInputVector()
    {
        float[] v = new float[60];
        int idx = 0;

        // 베이스 상황 (3)
        v[idx++] = attackerFirst ? 1f : 0f;
        v[idx++] = attackerSecond ? 1f : 0f;
        v[idx++] = attackerThird ? 1f : 0f;

        // 아웃카운트 (1)
        v[idx++] = attackerOut / 3f;

        // 이닝 / 초말 (2)
        v[idx++] = inning / 12f;
        v[idx++] = isTop ? 1f : 0f;

        // 점수 차이 (1)
        v[idx++] = Mathf.Clamp((attackerScore - defenderScore) / 10f, -1f, 1f);

        // 공격자 손패 카드 종류별 장수 (17)
        foreach (CardId id in System.Enum.GetValues(typeof(CardId)))
        {
            int count = 0;
            foreach (CardId c in attackerHand)
                if (c == id) count++;
            v[idx++] = count / 4f;
        }

        // 공격자 세트존 카드 종류별 장수 (17)
        foreach (CardId id in System.Enum.GetValues(typeof(CardId)))
        {
            int count = 0;
            foreach (CardId c in attackerSetZone)
                if (c == id) count++;
            v[idx++] = count / 4f;
        }

        // 수비자 세트존 장수만 (1) - 실제 카드는 모름
        v[idx++] = defenderSetZone.Count / 4f;

        // 수비자 버린카드 종류별 장수 (17) - 추측 근거
        foreach (CardId id in System.Enum.GetValues(typeof(CardId)))
        {
            int count = 0;
            foreach (CardId c in defenderDiscard)
                if (c == id) count++;
            v[idx++] = count / 4f;
        }

        // 공격자 손패 총 장수 (1)
        v[idx++] = attackerHand.Count / 11f;

        return v;
    }

    // 가능한 행동 목록 반환
    public List<MCTSAction> GetLegalActions()
    {
        List<MCTSAction> actions = new List<MCTSAction>();

        for (int i = 0; i < attackerHand.Count; i++)
        {
            CardId card = attackerHand[i];
            CardCategory cat = GetCategory(card);

            if (cat == CardCategory.Defense || cat == CardCategory.Trap)
                actions.Add(new MCTSAction(MCTSActionType.SetCard, card, i));
            else
                actions.Add(new MCTSAction(MCTSActionType.UseCard, card, i));
        }

        actions.Add(new MCTSAction(MCTSActionType.EndTurn, CardId.Hit, -1));
        return actions;
    }

    // 카드 분류 반환
    public static CardCategory GetCategory(CardId id)
    {
        switch (id)
        {
            case CardId.Hit:
            case CardId.Double:
            case CardId.Triple:
            case CardId.HomeRun:
            case CardId.Steal:
            case CardId.Bunt:
                return CardCategory.Attack;

            case CardId.GreatCatch:
            case CardId.DoublePlay:
            case CardId.TriplePlay:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                return CardCategory.Defense;

            case CardId.Dazzle:
            case CardId.BadBounce:
                return CardCategory.Trap;

            case CardId.PinchHitter:
            case CardId.PinchRunner:
            case CardId.PitcherChange:
            case CardId.DefensiveSub:
                return CardCategory.Draw;
        }
        return CardCategory.Attack;
    }
}

// 행동 타입
public enum MCTSActionType
{
    UseCard,
    SetCard,
    EndTurn
}

// 행동 정의
public class MCTSAction
{
    public MCTSActionType actionType;
    public CardId cardId;
    public int handIndex;

    public MCTSAction(MCTSActionType type, CardId id, int index)
    {
        actionType = type;
        cardId = id;
        handIndex = index;
    }
}