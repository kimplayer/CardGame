using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;


// 플레이어 1명의 실제 게임 데이터(손패, 세트카드, 점수, 베이스 상태 등)를 저장하는 클래스
// MonoBehaviour가 아니라 데이터만 관리하는 용도
public class SingleLaneElement
{
    public string selectedCard;
    public string selectedSetCard;

    public Dictionary<int, CardId> handCard;
    public Dictionary<int, CardId> setCard;

    public List<CardId> deck;
    public List<CardId> discard;

    public int score;
    public int outCount;

    public bool firstBase;
    public bool secondBase;
    public bool thirdBase;

    private int nextCardId;

    public SingleLaneElement()
    {
        selectedCard = "";
        selectedSetCard = "";

        handCard = new Dictionary<int, CardId>();
        setCard = new Dictionary<int, CardId>();
        deck = new List<CardId>();
        discard = new List<CardId>();

        score = 0;
        outCount = 0;

        firstBase = false;
        secondBase = false;
        thirdBase = false;

        nextCardId = 0;

        ResetDeck();
        ShuffleDeck();
    }

    // 기본 덱 재구성
    public void ResetDeck()
    {
        deck.Clear();

        deck.AddRange(new CardId[]
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
        });
    }

    // 커스텀 덱 설정 (덱빌딩 씬에서 넘어온 덱 사용)
    public void SetCustomDeck(List<CardId> customDeck)
    {
        deck.Clear();
        deck.AddRange(customDeck);
        discard.Clear();
        ShuffleDeck();
    }

    // 셔플
    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            CardId temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    // 버린 카드로 덱 재구성
    public void ResetDeckFromDiscard()
    {
        deck.Clear();
        deck.AddRange(discard);
        discard.Clear();
        ShuffleDeck();
    }

    // count 만큼 드로우
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                if (discard.Count > 0)
                    ResetDeckFromDiscard();
                else
                {
                    ResetDeck();
                    ShuffleDeck();
                }
            }

            if (deck.Count == 0) return;

            CardId drawCard = deck[0];
            deck.RemoveAt(0);

            handCard.Add(nextCardId, drawCard);
            nextCardId++;
        }
    }

    // 선택한 손패 카드 사용 처리
    public CardId RemoveSelectedCardFromHand()
    {
        if (string.IsNullOrEmpty(selectedCard)) return CardId.Hit;

        string[] split = selectedCard.Split('_');
        if (split.Length < 2) return CardId.Hit;

        int key;
        if (!int.TryParse(split[1], out key)) return CardId.Hit;
        if (!handCard.ContainsKey(key)) return CardId.Hit;

        CardId card = handCard[key];
        handCard.Remove(key);
        discard.Add(card);
        selectedCard = "";

        return card;
    }

    // 선택한 손패 카드를 세트존으로 이동
    public bool SetSelectedCardFromHand()
    {
        if (string.IsNullOrEmpty(selectedCard)) return false;

        string[] split = selectedCard.Split('_');
        if (split.Length < 2) return false;

        int key;
        if (!int.TryParse(split[1], out key)) return false;
        if (!handCard.ContainsKey(key)) return false;

        CardId card = handCard[key];
        handCard.Remove(key);
        setCard.Add(key, card);
        selectedCard = "";

        return true;
    }

    // 세트존 카드 제거
    public bool RemoveSetCard(int key, out CardId card)
    {
        card = CardId.Hit;
        if (!setCard.ContainsKey(key)) return false;

        card = setCard[key];
        setCard.Remove(key);
        discard.Add(card);
        return true;
    }

    // 베이스 초기화
    public void ResetBases()
    {
        firstBase = false;
        secondBase = false;
        thirdBase = false;
    }

    // 아웃카운트 초기화
    public void ResetOutCount()
    {
        outCount = 0;
    }

    // 베이스 위 주자 수 반환
    public int RunnerCount()
    {
        int count = 0;
        if (firstBase) count++;
        if (secondBase) count++;
        if (thirdBase) count++;
        return count;
    }
}