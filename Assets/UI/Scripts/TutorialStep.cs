using UnityEngine;

public enum TutorialActionType
{
    WaitNext,
    DragAttackCard,
    DragSetCard,
    PressEndTurn,
}

[System.Serializable]
public class TutorialStep
{
    [TextArea(2, 5)]
    public string message;
    public TutorialActionType actionType;
    public CardId requiredCard;
}
