using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SingleLaneElement
{
    public string selectedCard;
    public Dictionary<int, int> handCard;

    //덱더미
    public List<int> deck;
    //버림더미
    public List<int> discard;

    public int score;
    public int outCount;

    public bool firstBase;
    public bool secondBase;
    public bool thirdBase;

    private int nextCardId;


    public SingleLaneElement()
    {
        selectedCard = "";
        handCard = new Dictionary<int, int>();
        deck = new List<int>();
        discard = new List<int>();

        score = 0;
        outCount = 0;

        firstBase = false;
        secondBase = false;
        thirdBase = false;

        nextCardId = 0;

        ResetDeck();
        ShuffleDeck();
    }

    // 덱 초기화
    public void ResetDeck()
    {
        deck.Clear();

        deck.AddRange(new int[] {
            0, 0, 0, 0, 0, 0, 
            1, 1, 1,
            2, 2, 2,
            3,
            4
        });
    }


    //덱 셔플
    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            int temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    // 버린 카드로 덱 초기화
    public void ResetDeckFromDiscard()
    {
        deck.Clear();
        deck.AddRange(discard);
        discard.Clear();
        ShuffleDeck();
    }

    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0 )
            {
                if (discard.Count == 0)
                {
                    ResetDeck();
                    ShuffleDeck();
                }
                else
                {
                    ResetDeckFromDiscard();
                }
            }
            if (deck.Count == 0) return; // 덱과 버린 카드 모두 없는 경우, 더 이상 카드를 뽑을 수 없음

            int drawType = deck[0];
            deck.RemoveAt(0);

            handCard.Add(nextCardId, drawType);
            nextCardId++;

        }
    }

    public int RemoveSelectedCardFromHand()
    {
        if (string.IsNullOrEmpty(selectedCard)) return -1;

        string[] split = selectedCard.Split('_');
        if (split.Length < 2) return 01;

        int key;
        if (!int.TryParse(split[1], out key)) return -1;
        if (!handCard.ContainsKey(key)) return -1;

        int cardType = handCard[key];
        handCard.Remove(key);
        discard.Add(cardType);
        selectedCard = "";

        return cardType;
    }

    // 손에서 카드 털기
    public void ClearHand()
    {
        handCard.Clear();
        selectedCard = "";
    }

    public void ResetBases()
    {
        firstBase = false;
        secondBase = false;
        thirdBase = false;
    }
    public void ResetOutCount()
    {
        outCount = 0;
    }
}