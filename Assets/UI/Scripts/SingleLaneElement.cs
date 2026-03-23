using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class SingleLaneElement
{
    public string selectedCard;
    public Dictionary<int, int> handCard;
    public int life;
    public List<int> startingCardList;

    public SingleLaneElement()
    {
        selectedCard = "";
        handCard = new Dictionary<int, int>();
        startingCardList = new List<int>(new int[] { 0, 1, 2, 2, 3 });
    }

    // 카드 핸드에 세팅
    public void SetHand()
    {
        for (int n = 0; n < startingCardList.Count; n++)
        {
            int card_kind = startingCardList[n];
            handCard.Add(n, card_kind);
        }
    }

    // 손에서 카드 털기
    public void ClearHand()
    {
        handCard.Clear();
    }
}