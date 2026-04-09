using System.Collections.Generic;
using UnityEngine;

public class MCTSNode
{
    public GameState state;
    public MCTSAction action;       // 이 노드에 오기까지 한 행동
    public MCTSNode parent;
    public List<MCTSNode> children;

    public int visitCount;
    public float totalValue;

    // 아직 시도 안 한 행동 목록
    public List<MCTSAction> untriedActions;

    private const float C = 1.41f; // 탐색 상수

    public MCTSNode(GameState state, MCTSAction action, MCTSNode parent)
    {
        this.state = state;
        this.action = action;
        this.parent = parent;
        children = new List<MCTSNode>();
        visitCount = 0;
        totalValue = 0f;
        untriedActions = state.GetLegalActions();
    }

    // 승률
    public float WinRate => visitCount == 0 ? 0f : totalValue / visitCount;

    // 모든 행동이 탐색됐는지
    public bool IsFullyExpanded => untriedActions.Count == 0;

    // 리프 노드인지
    public bool IsLeaf => children.Count == 0;

    // UCB1 점수
    public float UCB1(float parentVisit)
    {
        if (visitCount == 0) return float.MaxValue;
        return WinRate + C * Mathf.Sqrt(Mathf.Log(parentVisit) / visitCount);
    }

    // UCB1 기준 최선 자식 반환
    public MCTSNode BestChild()
    {
        MCTSNode best = null;
        float bestScore = float.MinValue;

        foreach (MCTSNode child in children)
        {
            float score = child.UCB1(visitCount);
            if (score > bestScore)
            {
                bestScore = score;
                best = child;
            }
        }

        return best;
    }

    // 가장 많이 방문한 자식 반환 (최종 행동 선택 시)
    public MCTSNode MostVisitedChild()
    {
        MCTSNode best = null;
        int bestVisit = -1;

        foreach (MCTSNode child in children)
        {
            if (child.visitCount > bestVisit)
            {
                bestVisit = child.visitCount;
                best = child;
            }
        }

        return best;
    }

    // 역전파
    public void Update(float value)
    {
        visitCount++;
        totalValue += value;
    }
}