using System.Collections.Generic;
using UnityEngine;

public class MCTSAgent
{
    private int iterations;
    private int samplingCount;

    // ML 추론기 (없으면 순수 MCTS로 동작)
    private MLPolicyInference mlInference;

    public MCTSAgent(int iterations = 50, int samplingCount = 20,
                     MLPolicyInference ml = null)
    {
        this.iterations = iterations;
        this.samplingCount = samplingCount;
        this.mlInference = ml;
    }

    // 최선의 행동 반환
    public MCTSAction GetBestAction(GameState rootState)
    {
        Dictionary<int, float[]> cardScores = new Dictionary<int, float[]>();

        for (int s = 0; s < samplingCount; s++)
        {
            GameState sampledState = SampleOpponentHand(rootState);

            // ML Policy 가져오기
            float[] policy = null;
            if (mlInference != null)
                policy = mlInference.GetPolicy(sampledState.ToInputVector());

            MCTSNode root = new MCTSNode(sampledState, null, null);

            // ML Policy로 루트 자식 노드 Prior 설정
            if (policy != null)
                InitRootWithPolicy(root, policy);

            for (int i = 0; i < iterations; i++)
            {
                MCTSNode node = Select(root);
                MCTSNode expanded = Expand(node, policy);

                // ML Value로 Simulation 대체 or 보조
                float result;
                if (mlInference != null)
                    result = HybridEvaluate(expanded.state);
                else
                    result = Simulate(expanded.state);

                Backpropagate(expanded, result);
            }

            foreach (MCTSNode child in root.children)
            {
                int key = child.action.handIndex;
                if (!cardScores.ContainsKey(key))
                    cardScores[key] = new float[] { 0f, 0f };

                cardScores[key][0] += child.WinRate;
                cardScores[key][1] += 1f;
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

        if (bestAction == null)
            bestAction = new MCTSAction(MCTSActionType.EndTurn, CardId.Hit, -1);

        return bestAction;
    }

    // 루트 노드 자식들에 ML Prior 초기화
    private void InitRootWithPolicy(MCTSNode root, float[] policy)
    {
        List<MCTSAction> actions = root.state.GetLegalActions();

        for (int i = 0; i < actions.Count; i++)
        {
            int idx = Mathf.Min(i, policy.Length - 1);
            float prior = policy[idx];

            GameState newState = ApplyAction(new GameState(root.state), actions[i]);
            MCTSNode child = new MCTSNode(newState, actions[i], root, prior);

            root.children.Add(child);
        }

        root.untriedActions.Clear();
    }

    // ML Value + Simulation 혼합 평가
    private float HybridEvaluate(GameState state)
    {
        // ML Value 예측
        float mlValue = mlInference.GetValue(state.ToInputVector());

        // 짧은 롤아웃
        float simValue = ShortSimulate(state, 5);

        // 7:3 비율로 혼합 (ML 신뢰도가 높을수록 ML 비중 높이기)
        return 0.7f * mlValue + 0.3f * simValue;
    }

    // 짧은 롤아웃 (ML 보조용)
    private float ShortSimulate(GameState state, int steps)
    {
        GameState sim = new GameState(state);

        for (int i = 0; i < steps; i++)
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

    // Selection
    private MCTSNode Select(MCTSNode node)
    {
        while (!node.IsLeaf || node.IsFullyExpanded)
        {
            if (!node.IsFullyExpanded)
                return node;

            MCTSNode best = node.BestChild();
            if (best == null) break;
            node = best;
        }
        return node;
    }

    // Expansion - ML Policy Prior 반영
    private MCTSNode Expand(MCTSNode node, float[] policy)
    {
        if (node.untriedActions.Count == 0) return node;

        MCTSAction action;

        if (policy != null)
        {
            // Policy 확률 기반으로 유망한 행동 우선 선택
            action = SelectByPolicy(node.untriedActions, policy);
        }
        else
        {
            // ML 없으면 랜덤 선택
            int rand = Random.Range(0, node.untriedActions.Count);
            action = node.untriedActions[rand];
        }

        node.untriedActions.Remove(action);

        // Prior 값 계산
        float prior = 1f / Mathf.Max(1, node.untriedActions.Count + 1);
        if (policy != null)
        {
            int idx = node.state.attackerHand.IndexOf(action.cardId);
            if (idx >= 0 && idx < policy.Length)
                prior = policy[idx];
        }

        GameState newState = ApplyAction(new GameState(node.state), action);
        MCTSNode child = new MCTSNode(newState, action, node, prior);
        node.children.Add(child);

        return child;
    }

    // Policy 확률 기반 행동 선택
    private MCTSAction SelectByPolicy(List<MCTSAction> actions, float[] policy)
    {
        float total = 0f;
        List<float> weights = new List<float>();

        for (int i = 0; i < actions.Count; i++)
        {
            int idx = Mathf.Min(actions[i].handIndex >= 0
                                     ? actions[i].handIndex : 0,
                                     policy.Length - 1);
            float weight = policy[idx];
            weights.Add(weight);
            total += weight;
        }

        // 가중치 기반 랜덤 선택
        float rand = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < actions.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
                return actions[i];
        }

        return actions[actions.Count - 1];
    }

    // Simulation (ML 없을 때 사용)
    private float Simulate(GameState state)
    {
        GameState sim = new GameState(state);
        int maxStep = 20;

        for (int i = 0; i < maxStep; i++)
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

    // Backpropagation
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

        score += (state.attackerScore - state.defenderScore) * 2f;

        if (state.attackerFirst) score += 0.5f;
        if (state.attackerSecond) score += 0.8f;
        if (state.attackerThird) score += 1.2f;

        score -= state.attackerOut * 1.5f;
        score += state.attackerHand.Count * 0.1f;

        return Mathf.Clamp(score / 10f, -1f, 1f);
    }

    // 상대 손패 샘플링
    private GameState SampleOpponentHand(GameState state)
    {
        GameState sampled = new GameState(state);
        List<CardId> candidateDeck = BuildCandidateDeck(state);

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

    // 후보 덱 구성
    private List<CardId> BuildCandidateDeck(GameState state)
    {
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

        foreach (CardId c in state.defenderDiscard) full.Remove(c);
        foreach (CardId c in state.defenderSetZone) full.Remove(c);

        return full;
    }

    // 행동 적용
    private GameState ApplyAction(GameState state, MCTSAction action)
    {
        if (action.actionType == MCTSActionType.EndTurn) return state;

        if (action.handIndex < 0 ||
            action.handIndex >= state.attackerHand.Count) return state;

        CardId card = state.attackerHand[action.handIndex];
        state.attackerHand.RemoveAt(action.handIndex);
        state.attackerDiscard.Add(card);

        if (action.actionType == MCTSActionType.SetCard)
        {
            state.attackerSetZone.Add(card);
            return state;
        }

        ApplyCardEffect(state, card);
        return state;
    }

    // 카드 효과 (시뮬레이션용)
    private void ApplyCardEffect(GameState state, CardId card)
    {
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
            case CardId.PinchHitter: DrawSimulated(state, 3); break;
            case CardId.PinchRunner: DrawSimulated(state, 2); break;
            case CardId.PitcherChange: RemoveDefenderHand(state, 2); break;
            case CardId.DefensiveSub: RemoveDefenderHand(state, 1); break;
        }
    }

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

    private void SetBase(GameState state, int num)
    {
        if (num == 1) state.attackerFirst = true;
        if (num == 2) state.attackerSecond = true;
        if (num == 3) state.attackerThird = true;
    }

    private void DrawSimulated(GameState state, int count)
    {
        for (int i = 0; i < count; i++)
            state.attackerHand.Add(CardId.Hit);
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

    private bool HasRunner(GameState state) =>
        state.attackerFirst || state.attackerSecond || state.attackerThird;

    private int RunnerCount(GameState state)
    {
        int c = 0;
        if (state.attackerFirst) c++;
        if (state.attackerSecond) c++;
        if (state.attackerThird) c++;
        return c;
    }
}