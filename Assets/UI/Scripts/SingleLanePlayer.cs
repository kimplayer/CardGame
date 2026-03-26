using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using Unity.Properties;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleLanePlayer : MonoBehaviour
{
    public GameObject cardPrefab;
    public int handPositionY;
    public int battlePositionY;

    public Text scoreText;
    public Text outText;
    public Text baseText;

    private SingleLaneElement singleLaneElement;
    private bool opponent;

    private void Awake()
    {
        singleLaneElement = new SingleLaneElement();
    }

    public void Initialize(bool isOpponent)
    {
        opponent = isOpponent;

        singleLaneElement.score = 0;
        singleLaneElement.ResetOutCount();
        singleLaneElement.ResetBases();
        singleLaneElement.ClearHand();

        singleLaneElement.ResetDeck();
        singleLaneElement.ShuffleDeck();

        DrawStartHand();
        RefreshAllUI();
    }

    public void DrawStartHand()
    {
        singleLaneElement.DrawCards(5);
        RefreshHandUI();
    }

    public void DrawTurnCards()
    {
        singleLaneElement.DrawCards(3);
        RefreshHandUI();
    }

    public void ClickCard()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;

        string selectedCardName = EventSystem.current.currentSelectedGameObject.name;
        singleLaneElement.selectedCard = selectedCardName;
        Debug.Log(selectedCardName + " Selected");
    }

    public bool CheckSelectedCard()
    {
        return !string.IsNullOrEmpty(singleLaneElement.selectedCard);
    }

    public int GetSelectedCardType()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return -1;

        Transform tf = transform.Find(singleLaneElement.selectedCard);
        if (tf == null) return -1;

        Card card = tf.GetComponent<Card>();
        if (card == null) return -1;

        return card.cardType;
    }

    public void MoveSelectedCardToBattleZone()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return;

        Transform tf = transform.Find(singleLaneElement.selectedCard);
        if (tf != null)
        {
            tf.localPosition = new Vector2(0, battlePositionY);
        }
    }

    public int UseSelectedCard()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return -1;

        int cardType = GetSelectedCardType();

        Transform tf = transform.Find(singleLaneElement.selectedCard);
        if (tf != null)
        {
            Destroy(tf.gameObject);
        }

        singleLaneElement.RemoveSelectedCardFromHand();
        return cardType;
    }

    public void AISelectCard()
    {
        List<int> keys = new List<int>(singleLaneElement.handCard.Keys);
        if (keys.Count == 0)
        {
            singleLaneElement.selectedCard = "";
            return;
        }

        int rand = Random.Range(0, keys.Count);
        int selected = keys[rand];
        singleLaneElement.selectedCard = "Card_" + selected;
    }

    public int GetHandCount()
    {
        return singleLaneElement.handCard.Count;
    }

    public void AddScore(int value)
    {
        singleLaneElement.score += value;
        UpdateScoreUI();
    }

    public int GetScore()
    {
        return singleLaneElement.score;
    }

    public void AddOut()
    {
        singleLaneElement.outCount++;
        UpdateOutUI();
    }

    public int GetOutCount()
    {
        return singleLaneElement.outCount;
    }

    public void ResetOutCount()
    {
        singleLaneElement.ResetOutCount();
        UpdateOutUI();
    }

    public void ResetBases()
    {
        singleLaneElement.ResetBases();
        UpdateBaseUI();
    }

    public void RefreshAllUI()
    {
        UpdateScoreUI();
        UpdateOutUI();
        UpdateBaseUI();
        RefreshHandUI();
    }


    public void ApplyBattingResult(int cardType)
    {
        switch (cardType)
        {
            case 0:
                AddOut();
                break;

            case 1:
                AdvanceRunners(1);
                break;

            case 2:
                AdvanceRunners(2);
                break;

            case 3:
                AdvanceRunners(3);
                break;

            case 4:
                HomeRun();
                break;
        }

        UpdateBaseUI();
        UpdateScoreUI();
        UpdateOutUI();
    }

    private void AdvanceRunners(int hitValue)
    {
        int runs = 0;

        bool oldFirst = singleLaneElement.firstBase;
        bool oldSecond = singleLaneElement.secondBase;
        bool oldThird = singleLaneElement.thirdBase;

        singleLaneElement.ResetBases();

        // 3루 주자
        if (oldThird)
        {
            if (3 + hitValue >= 4) runs++;
            else SetBase(3 + hitValue);
        }

        // 2루 주자
        if (oldSecond)
        {
            if (2 + hitValue >= 4) runs++;
            else SetBase(2 + hitValue);
        }

        // 1루 주자
        if (oldFirst)
        {
            if (1 + hitValue >= 4) runs++;
            else SetBase(1 + hitValue);
        }

        // 타자 주자
        if (hitValue >= 4)
        {
            runs++;
        }
        else
        {
            SetBase(hitValue);
        }

        AddScore(runs);
    }

    private void HomeRun()
    {
        int runs = 1; // 타자

        if (singleLaneElement.firstBase) runs++;
        if (singleLaneElement.secondBase) runs++;
        if (singleLaneElement.thirdBase) runs++;

        singleLaneElement.ResetBases();
        AddScore(runs);
    }

    private void SetBase(int baseNum)
    {
        switch (baseNum)
        {
            case 1:
                singleLaneElement.firstBase = true;
                break;
            case 2:
                singleLaneElement.secondBase = true;
                break;
            case 3:
                singleLaneElement.thirdBase = true;
                break;
        }
    }

    public void RefreshHandUI()
    {
        ClearHandObjectsOnly();

        Vector2 position = new Vector2(0, handPositionY);
        int positionX = -3 * 250;

        foreach (var item in singleLaneElement.handCard)
        {
            GameObject temp = Instantiate(cardPrefab, transform);

            positionX += 250;
            position.x = positionX;
            temp.transform.localPosition = position;

            Card cardComp = temp.GetComponent<Card>();
            Button buttonComp = temp.GetComponent<Button>();

            if (cardComp != null)
            {
                cardComp.cardType = item.Value;
            }

            Transform textTf = temp.transform.Find("Text");
            if (textTf != null)
            {
                Text txt = textTf.GetComponent<Text>();
                if (txt != null)
                {
                    txt.text = GetCardName(item.Value);
                }
            }

            string cardName = "Card_" + item.Key;
            temp.name = cardName;

            if (buttonComp != null)
            {
                buttonComp.onClick.RemoveAllListeners();
                buttonComp.onClick.AddListener(ClickCard);

                if (opponent)
                {
                    buttonComp.interactable = false;
                }
            }

            if (cardComp != null)
            {
                cardComp.SetInfo();
            }
        }
    }

    private string GetCardName(int cardType)
    {
        switch (cardType)
        {
            case 0: return "OUT";
            case 1: return "1B";
            case 2: return "2B";
            case 3: return "3B";
            case 4: return "HR";
            default: return "?";
        }
    }

    private void ClearHandObjectsOnly()
    {
        List<GameObject> deleteList = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Card_"))
            {
                deleteList.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in deleteList)
        {
            Destroy(obj);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "점수 : " + singleLaneElement.score;
        }
    }

    private void UpdateOutUI()
    {
        if (outText != null)
        {
            outText.text = "아웃 : " + singleLaneElement.outCount;
        }
    }

    private void UpdateBaseUI()
    {
        if (baseText != null)
        {
            string first = singleLaneElement.firstBase ? "●" : "○";
            string second = singleLaneElement.secondBase ? "●" : "○";
            string third = singleLaneElement.thirdBase ? "●" : "○";

            baseText.text = "1루:" + first + "  2루:" + second + "  3루:" + third;
        }
    }
}