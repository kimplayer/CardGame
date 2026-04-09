using System.Collections.Generic;
using UnityEngine;

public class MCTSNode
{
    public GameState state;
    public MCTSAction action;
    public MCTSNode parent;
    public List<MCTSNode> children;

    public int visitCount;
    public float totalValue;

    public List<MCTSAction> untriedActions;

    // ML Policy 확률값 (이 노드에 오기까지의 행동 확률)
    public float priorProbability;

    private const float C = 1.41f;
    private const float C_PUT = 1.0f;  // PUCT 상수 (ML 가중치 강도)

    public MCTSNode(GameState state, MCTSAction action,
                    MCTSNode parent, float prior = 1.0f)
    {
        this.state = state;
        this.action = action;
        this.parent = parent;
        this.priorProbability = prior;

        children = new List<MCTSNode>();
        visitCount = 0;
        totalValue = 0f;
        untriedActions = state.GetLegalActions();
    }

    public float WinRate => visitCount == 0 ? 0f : totalValue / visitCount;

    public bool IsFullyExpanded => untriedActions.Count == 0;
    public bool IsLeaf => children.Count == 0;

    // PUCT 공식 (ML Prior 반영한 UCB1)
    // AlphaZero에서 사용하는 방식
    public float PUCT(float parentVisit)
    {
        if (visitCount == 0)
            return float.MaxValue;

        float exploitation = WinRate;
        float exploration = C_PUT * priorProbability *
                             Mathf.Sqrt(parentVisit) / (1 + visitCount);

        return exploitation + exploration;
    }

    // PUCT 기준 최선 자식 반환
    public MCTSNode BestChild()
    {
        MCTSNode best = null;
        float bestScore = float.MinValue;

        foreach (MCTSNode child in children)
        {
            float score = child.PUCT(visitCount);
            if (score > bestScore)
            {
                bestScore = score;
                best = child;
            }
        }

        return best;
    }

    // 가장 많이 방문한 자식 반환
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

    public void Update(float value)
    {
        visitCount++;
        totalValue += value;
    }
}