using System.Collections.Generic;
using UnityEngine;

public class SelfPlayGame
{
    private MCTSAgent agent1;
    private MCTSAgent agent2;

    // 게임 진행 중 수집한 데이터
    public List<SelfPlayDataEntry> collectedData = new List<SelfPlayDataEntry>();

    public SelfPlayGame(MCTSAgent a1, MCTSAgent a2)
    {
        agent1 = a1;
        agent2 = a2;
    }

    // 게임 1판 실행, 승자 반환 (1 = agent1 승, -1 = agent2 승, 0 = 무)
    public int Run()
    {
        SingleLaneElement p1 = new SingleLaneElement();
        SingleLaneElement p2 = new SingleLaneElement();

        p1.DrawCards(5);
        p2.DrawCards(5);

        int inning = 1;
        bool isTop = true;
        bool p1Bats = true; // 선공 고정 (Self Play에서는 간단하게)

        while (true)
        {
            SingleLaneElement batter = p1Bats ? p1 : p2;
            SingleLaneElement fielder = p1Bats ? p2 : p1;
            MCTSAgent currentAgent = p1Bats ? agent1 : agent2;

            batter.ResetOutCount();
            batter.ResetBases();
            batter.DrawCards(3);

            // 반이닝 진행
            RunHalfInning(batter, fielder, currentAgent, p1Bats);

            // 초 → 말 → 다음 이닝
            if (isTop)
            {
                isTop = false;
                p1Bats = !p1Bats;
            }
            else
            {
                // 끝내기 판정
                if (inning >= 9)
                {
                    if (p1.score != p2.score)
                        break;
                }

                inning++;
                isTop = true;
                p1Bats = !p1Bats;

                if (inning > 12) break;
            }
        }

        // 최종 결과
        int result = 0;
        if (p1.score > p2.score) result = 1;
        else if (p1.score < p2.score) result = -1;

        // 수집한 데이터에 outcome 반영
        foreach (var entry in collectedData)
            entry.outcome = result;

        return result;
    }

    // 반이닝 진행
    private void RunHalfInning(
        SingleLaneElement batter,
        SingleLaneElement fielder,
        MCTSAgent agent,
        bool isP1Batting)
    {
        int maxTurns = 20;

        for (int t = 0; t < maxTurns; t++)
        {
            if (batter.outCount >= 3 || batter.handCard.Count == 0)
                break;

            // 현재 상태 → GameState 변환
            GameState state = ConvertToGameState(batter, fielder);

            // 신경망 입력 벡터 저장
            float[] inputVec = state.ToInputVector();

            // MCTS로 행동 결정
            MCTSAction action = agent.GetBestAction(state);

            // 행동 인덱스 기록
            int actionIdx = action.handIndex >= 0 ? action.handIndex : 99;

            // 데이터 저장
            collectedData.Add(new SelfPlayDataEntry
            {
                stateVector = inputVec,
                actionIndex = actionIdx,
                outcome = 0f // 나중에 최종 결과로 덮어씀
            });

            // 행동 실제 적용
            ApplyActionToElement(batter, fielder, action);
        }
    }

    // SingleLaneElement → GameState 변환
    private GameState ConvertToGameState(
        SingleLaneElement batter,
        SingleLaneElement fielder)
    {
        GameState state = new GameState();

        foreach (var pair in batter.handCard)
            state.attackerHand.Add(pair.Value);

        foreach (var pair in batter.setCard)
            state.attackerSetZone.Add(pair.Value);

        foreach (var card in batter.discard)
            state.attackerDiscard.Add(card);

        state.attackerFirst = batter.firstBase;
        state.attackerSecond = batter.secondBase;
        state.attackerThird = batter.thirdBase;
        state.attackerOut = batter.outCount;
        state.attackerScore = batter.score;

        foreach (var pair in fielder.setCard)
            state.defenderSetZone.Add(pair.Value);

        foreach (var card in fielder.discard)
            state.defenderDiscard.Add(card);

        state.defenderScore = fielder.score;

        return state;
    }

    // 행동을 실제 SingleLaneElement에 적용
    private void ApplyActionToElement(
        SingleLaneElement batter,
        SingleLaneElement fielder,
        MCTSAction action)
    {
        if (action.actionType == MCTSActionType.EndTurn) return;

        List<int> keys = new List<int>(batter.handCard.Keys);
        if (action.handIndex < 0 || action.handIndex >= keys.Count) return;

        int key = keys[action.handIndex];
        CardId card = batter.handCard[key];

        batter.handCard.Remove(key);
        batter.discard.Add(card);

        if (action.actionType == MCTSActionType.SetCard)
        {
            batter.setCard.Add(key, card);
            return;
        }

        // 공격/드로우 처리 (간소화)
        CardCategory cat = GameState.GetCategory(card);
        if (cat == CardCategory.Attack)
        {
            // 실제 게임 로직 재사용 불가 (MonoBehaviour 없음)
            // 간소화된 적용
            ApplyAttackSimple(batter, fielder, card);
        }
    }

    private void ApplyAttackSimple(
        SingleLaneElement batter,
        SingleLaneElement fielder,
        CardId card)
    {
        switch (card)
        {
            case CardId.Hit:
                AdvanceSimple(batter, 1, true); break;
            case CardId.Double:
                AdvanceSimple(batter, 2, true); break;
            case CardId.Triple:
                AdvanceSimple(batter, 3, true); break;
            case CardId.HomeRun:
                int runs = 1;
                if (batter.firstBase) runs++;
                if (batter.secondBase) runs++;
                if (batter.thirdBase) runs++;
                batter.firstBase = batter.secondBase = batter.thirdBase = false;
                batter.score += runs;
                break;
            case CardId.Steal:
                AdvanceSimple(batter, 1, false); break;
            case CardId.Bunt:
                if (batter.firstBase)
                {
                    batter.firstBase = false;
                    if (batter.secondBase)
                    {
                        if (batter.thirdBase) batter.score++;
                        else batter.thirdBase = true;
                    }
                    batter.secondBase = true;
                }
                break;
        }
    }

    private void AdvanceSimple(SingleLaneElement b, int value, bool batter)
    {
        bool f = b.firstBase, s = b.secondBase, t = b.thirdBase;
        b.firstBase = b.secondBase = b.thirdBase = false;

        if (t) { if (3 + value >= 4) b.score++; else SetBase(b, 3 + value); }
        if (s) { if (2 + value >= 4) b.score++; else SetBase(b, 2 + value); }
        if (f) { if (1 + value >= 4) b.score++; else SetBase(b, 1 + value); }
        if (batter) { if (value >= 4) b.score++; else SetBase(b, value); }
    }

    private void SetBase(SingleLaneElement b, int num)
    {
        if (num == 1) b.firstBase = true;
        if (num == 2) b.secondBase = true;
        if (num == 3) b.thirdBase = true;
    }
}