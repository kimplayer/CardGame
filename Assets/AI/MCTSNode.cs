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

    private const float C = 1.41f;

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

    public float WinRate => visitCount == 0 ? 0f : totalValue / visitCount;
    public bool IsFullyExpanded => untriedActions.Count == 0;
    public bool IsLeaf => children.Count == 0;

    // UCB1
    public float UCB1(float parentVisit)
    {
        if (visitCount == 0) return float.MaxValue;
        return WinRate + C * Mathf.Sqrt(Mathf.Log(parentVisit) / visitCount);
    }

    // UCB1 기준 최선 자식
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

    // 가장 많이 방문한 자식
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