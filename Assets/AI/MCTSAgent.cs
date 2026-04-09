using System.Collections.Generic;
using UnityEngine;

public class MCTSAgent
{
    private int iterations;    // MCTS 반복 횟수
    private int samplingCount; // IS-MCTS 샘플링 횟수

    public MCTSAgent(int iterations = 50, int samplingCount = 20)
    {
        this.iterations = iterations;
        this.samplingCount = samplingCount;
    }

    // 최선의 행동 반환
    public MCTSAction GetBestAction(GameState rootState)
    {
        Dictionary<MCTSActionType, float> actionScores =
            new Dictionary<MCTSActionType, float>();

        Dictionary<int, float[]> cardScores = new Dictionary<int, float[]>();

        // IS-MCTS : 샘플링 반복
        for (int s = 0; s < samplingCount; s++)
        {
            // 상대 손패 샘플링
            GameState sampledState = SampleOpponentHand(rootState);

            // 이 샘플 상태로 MCTS 실행
            MCTSNode root = new MCTSNode(sampledState, null, null);

            for (int i = 0; i < iterations; i++)
            {
                MCTSNode node = Select(root);
                MCTSNode expanded = Expand(node);
                float result = Simulate(expanded.state);
                Backpropagate(expanded, result);
            }

            // 결과 누적
            foreach (MCTSNode child in root.children)
            {
                int key = child.action.handIndex;
                if (!cardScores.ContainsKey(key))
                    cardScores[key] = new float[] { 0f, 0 };

                cardScores[key][0] += child.WinRate;
                cardScores[key][1] += 1;
            }
        }

        // 평균 승률 가장 높은 행동 선택
        MCTSAction bestAction = null;
        float bestScore = float.MinValue;

        foreach (var pair in cardScores)
        {
            float avgScore = pair.Value[0] / pair.Value[1];
            if (avgScore > bestScore)
            {
                bestScore = avgScore;

                // 해당 handIndex의 행동 찾기
                foreach (MCTSAction action in rootState.GetLegalActions())
                {
                    if (action.handIndex == pair.Key)
                    {
                        bestAction = action;
                        break;
                    }
                }
            }
        }

        // 행동 못 찾으면 턴 종료
        if (bestAction == null)
            bestAction = new MCTSAction(MCTSActionType.EndTurn, CardId.Hit, -1);

        return bestAction;
    }

    // Selection : UCB1으로 탐색할 노드 선택
    private MCTSNode Select(MCTSNode node)
    {
        while (!node.IsLeaf || node.IsFullyExpanded)
        {
            if (!node.IsFullyExpanded)
                return node;
            else
                node = node.BestChild();

            if (node == null) break;
        }
        return node;
    }

    // Expansion : 새 자식 노드 추가
    private MCTSNode Expand(MCTSNode node)
    {
        if (node.untriedActions.Count == 0) return node;

        int rand = Random.Range(0, node.untriedActions.Count);
        MCTSAction action = node.untriedActions[rand];
        node.untriedActions.RemoveAt(rand);

        GameState newState = ApplyAction(new GameState(node.state), action);
        MCTSNode child = new MCTSNode(newState, action, node);
        node.children.Add(child);

        return child;
    }

    // Simulation : 반이닝 끝까지 랜덤 롤아웃
    private float Simulate(GameState state)
    {
        GameState sim = new GameState(state);
        int maxSteps = 20;

        for (int step = 0; step < maxSteps; step++)
        {
            if (sim.attackerOut >= 3 || sim.attackerHand.Count == 0)
                break;

            List<MCTSAction> actions = sim.GetLegalActions();
            if (actions.Count == 0) break;

            int rand = Random.Range(0, actions.Count);
            sim = ApplyAction(sim, actions[rand]);
        }

        return Evaluate(sim);
    }

    // Backpropagation : 결과 역전파
    private void Backpropagate(MCTSNode node, float value)
    {
        while (node != null)
        {
            node.Update(value);
            node = node.parent;
        }
    }

    // 평가 함수
    private float Evaluate(GameState state)
    {
        float score = 0f;

        // 득점 차이
        score += (state.attackerScore - state.defenderScore) * 2f;

        // 베이스 주자
        if (state.attackerFirst) score += 0.5f;
        if (state.attackerSecond) score += 0.8f;
        if (state.attackerThird) score += 1.2f;

        // 아웃카운트 페널티
        score -= state.attackerOut * 1.5f;

        // 손패 수
        score += state.attackerHand.Count * 0.1f;

        // -1 ~ 1로 정규화
        return Mathf.Clamp(score / 10f, -1f, 1f);
    }

    // 상대 손패 샘플링 (IS-MCTS 핵심)
    private GameState SampleOpponentHand(GameState state)
    {
        GameState sampled = new GameState(state);

        // 이미 알려진 카드들 제외한 후보 덱 구성
        List<CardId> candidateDeck = BuildCandidateDeck(state);

        // 후보 덱에서 랜덤으로 상대 손패 채우기
        int handSize = Random.Range(3, 8);
        sampled.defenderHand.Clear();

        for (int i = 0; i < handSize && candidateDeck.Count > 0; i++)
        {
            int rand = Random.Range(0, candidateDeck.Count);
            sampled.defenderHand.Add(candidateDeck[rand]);
            candidateDeck.RemoveAt(rand);
        }

        return sampled;
    }

    // 후보 덱 구성 (버린 카드, 세트존 카드 제외)
    private List<CardId> BuildCandidateDeck(GameState state)
    {
        // 기본 덱 전체
        List<CardId> full = new List<CardId>
        {
            CardId.Hit, CardId.Hit, CardId.Hit, CardId.Hit,
            CardId.Double, CardId.Double, CardId.Double,
            CardId.Triple, CardId.Triple,
            CardId.HomeRun, CardId.HomeRun,
            CardId.Steal, CardId.Steal,
            CardId.Bunt, CardId.Bunt,
            CardId.GreatCatch, CardId.GreatCatch,
            CardId.DoublePlay, CardId.DoublePlay,
            CardId.TriplePlay,
            CardId.LookingStrikeOut, CardId.LookingStrikeOut,
            CardId.SwingStrikeOut, CardId.SwingStrikeOut,
            CardId.Dazzle, CardId.Dazzle,
            CardId.BadBounce, CardId.BadBounce,
            CardId.PinchHitter, CardId.PinchHitter,
            CardId.PinchRunner, CardId.PinchRunner,
            CardId.PitcherChange,
            CardId.DefensiveSub
        };

        // 버린 카드 제외
        foreach (CardId c in state.defenderDiscard)
            full.Remove(c);

        // 세트존 카드 제외
        foreach (CardId c in state.defenderSetZone)
            full.Remove(c);

        return full;
    }

    // 행동 적용 후 새 상태 반환
    private GameState ApplyAction(GameState state, MCTSAction action)
    {
        if (action.actionType == MCTSActionType.EndTurn)
            return state;

        if (action.handIndex < 0 || action.handIndex >= state.attackerHand.Count)
            return state;

        CardId card = state.attackerHand[action.handIndex];
        state.attackerHand.RemoveAt(action.handIndex);
        state.attackerDiscard.Add(card);

        if (action.actionType == MCTSActionType.SetCard)
        {
            state.attackerSetZone.Add(card);
            return state;
        }

        // 공격/드로우 카드 효과 적용
        ApplyCardEffect(state, card);
        return state;
    }

    // 카드 효과 적용 (시뮬레이션용 간소화 버전)
    private void ApplyCardEffect(GameState state, CardId card)
    {
        // 수비 발동 체크
        bool blocked = TryDefend(state, card);
        if (blocked) return;

        switch (card)
        {
            case CardId.Hit: AdvanceRunners(state, 1, true); break;
            case CardId.Double: AdvanceRunners(state, 2, true); break;
            case CardId.Triple: AdvanceRunners(state, 3, true); break;
            case CardId.HomeRun: HomeRun(state); break;
            case CardId.Steal: AdvanceRunners(state, 1, false); break;
            case CardId.Bunt: ApplyBunt(state); break;

            case CardId.PinchHitter:
                DrawSimulated(state, 3); break;
            case CardId.PinchRunner:
                DrawSimulated(state, 2); break;
            case CardId.PitcherChange:
                RemoveDefenderHand(state, 2); break;
            case CardId.DefensiveSub:
                RemoveDefenderHand(state, 1); break;
        }
    }

    // 시뮬레이션용 수비 발동
    private bool TryDefend(GameState state, CardId attackCard)
    {
        for (int i = 0; i < state.defenderSetZone.Count; i++)
        {
            CardId def = state.defenderSetZone[i];

            bool isHitType = attackCard == CardId.Hit ||
                             attackCard == CardId.Double ||
                             attackCard == CardId.Triple;
            bool isHitOrDouble = attackCard == CardId.Hit ||
                                 attackCard == CardId.Double;

            bool canActivate = false;

            switch (def)
            {
                case CardId.GreatCatch:
                case CardId.LookingStrikeOut:
                case CardId.SwingStrikeOut:
                    canActivate = isHitType; break;

                case CardId.DoublePlay:
                    canActivate = HasRunner(state) && isHitOrDouble; break;

                case CardId.TriplePlay:
                    canActivate = RunnerCount(state) >= 2 &&
                                  attackCard == CardId.Hit; break;
            }

            if (canActivate)
            {
                // 불규칙 바운드 체크
                bool badBounce = TryBadBounce(state);
                if (!badBounce)
                {
                    ApplyDefenseEffect(state, def);
                    state.defenderSetZone.RemoveAt(i);
                    return true;
                }
                else
                {
                    state.defenderSetZone.RemoveAt(i);
                    return false;
                }
            }
        }

        // 눈부심 체크
        TryDazzle(state);
        return false;
    }

    private bool TryBadBounce(GameState state)
    {
        for (int i = 0; i < state.attackerSetZone.Count; i++)
        {
            if (state.attackerSetZone[i] == CardId.BadBounce)
            {
                state.attackerSetZone.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    private void TryDazzle(GameState state)
    {
        for (int i = 0; i < state.attackerSetZone.Count; i++)
        {
            if (state.attackerSetZone[i] == CardId.Dazzle)
            {
                AdvanceRunners(state, 1, false);
                state.attackerSetZone.RemoveAt(i);
                return;
            }
        }
    }

    private void ApplyDefenseEffect(GameState state, CardId def)
    {
        switch (def)
        {
            case CardId.GreatCatch:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                state.attackerOut += 1; break;

            case CardId.DoublePlay:
                state.attackerOut += 2;
                RemoveFrontRunner(state); break;

            case CardId.TriplePlay:
                state.attackerOut += 2;
                RemoveFrontRunner(state);
                RemoveFrontRunner(state); break;
        }
    }

    // 주자 진루
    private void AdvanceRunners(GameState state, int value, bool batterAdvance)
    {
        bool f = state.attackerFirst;
        bool s = state.attackerSecond;
        bool t = state.attackerThird;

        state.attackerFirst = false;
        state.attackerSecond = false;
        state.attackerThird = false;

        if (t) { if (3 + value >= 4) state.attackerScore++; else SetBase(state, 3 + value); }
        if (s) { if (2 + value >= 4) state.attackerScore++; else SetBase(state, 2 + value); }
        if (f) { if (1 + value >= 4) state.attackerScore++; else SetBase(state, 1 + value); }
        if (batterAdvance) { if (value >= 4) state.attackerScore++; else SetBase(state, value); }
    }

    private void ApplyBunt(GameState state)
    {
        if (state.attackerFirst)
        {
            state.attackerFirst = false;
            if (state.attackerSecond)
            {
                if (state.attackerThird) state.attackerScore++;
                else state.attackerThird = true;
            }
            state.attackerSecond = true;
        }
    }

    private void HomeRun(GameState state)
    {
        int runs = 1;
        if (state.attackerFirst) runs++;
        if (state.attackerSecond) runs++;
        if (state.attackerThird) runs++;

        state.attackerFirst = false;
        state.attackerSecond = false;
        state.attackerThird = false;
        state.attackerScore += runs;
    }

    private void SetBase(GameState state, int baseNum)
    {
        if (baseNum == 1) state.attackerFirst = true;
        if (baseNum == 2) state.attackerSecond = true;
        if (baseNum == 3) state.attackerThird = true;
    }

    private void DrawSimulated(GameState state, int count)
    {
        for (int i = 0; i < count; i++)
            state.attackerHand.Add(CardId.Hit); // 간소화 - 실제론 덱에서 드로우
    }

    private void RemoveDefenderHand(GameState state, int count)
    {
        for (int i = 0; i < count && state.defenderHand.Count > 0; i++)
        {
            int rand = Random.Range(0, state.defenderHand.Count);
            state.defenderHand.RemoveAt(rand);
        }
    }

    private void RemoveFrontRunner(GameState state)
    {
        if (state.attackerThird) { state.attackerThird = false; return; }
        if (state.attackerSecond) { state.attackerSecond = false; return; }
        if (state.attackerFirst) { state.attackerFirst = false; }
    }

    private bool HasRunner(GameState state)
    {
        return state.attackerFirst || state.attackerSecond || state.attackerThird;
    }

    private int RunnerCount(GameState state)
    {
        int c = 0;
        if (state.attackerFirst) c++;
        if (state.attackerSecond) c++;
        if (state.attackerThird) c++;
        return c;
    }
}