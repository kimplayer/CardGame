using System.Collections.Generic;
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
    [TextArea(2, 5)] public string message;
    public TutorialActionType actionType;
    public CardId requiredCard;
    public string instruction;          // 액션 단계에서 표시할 안내 텍스트

    // 단계 시작 시 손패 교체 (null이면 유지)
    public List<CardId> sectionHand;

    // 내 베이스 설정
    public bool resetMyBases;
    public bool preSetMyRunner1, preSetMyRunner2, preSetMyRunner3;

    // 상대 상태 설정
    public bool resetOpponentState;         // 아웃카운트 + 베이스 초기화
    public bool preSetOpponentRunner1, preSetOpponentRunner2, preSetOpponentRunner3;
    public bool preSetOpponentSetCard;      // 상대 세트존에 카드 미리 배치
    public CardId opponentSetCard;
    public List<CardId> opponentHand;       // 상대 손패 설정

    // DragSetCard 완료 후 상대 공격 시뮬레이션
    public bool simulateOpponentAttack;
    public CardId opponentAttackCard;
}
