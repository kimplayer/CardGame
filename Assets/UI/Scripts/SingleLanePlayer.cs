using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleLanePlayer : MonoBehaviour
{
    public GameObject card;
    public GameObject cardPrefab;
    public int handPositionY;
    public int battlePositionY;

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
        singleLaneElement.outCount = 0;
        singleLaneElement.ClearHand();

        CreateStartHand();
        UpdateScoreUI();
        UpdateOutUI();
    }

    public void CreateStartHand()
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

        Transform cardTf = transform.Find(singleLaneElement.selectedCard);
        if (cardTf == null) return -1;

        Card cardComp = cardTf.GetComponent<Card>();
        if (cardComp == null) return -1;

        return cardComp.cardType;
    }

    public void MoveSelectedCardToBattleZone()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return;

        Transform selectedTf = transform.Find(singleLaneElement.selectedCard);
        if (selectedTf == null) return;

        selectedTf.localPosition = new Vector2(0, battlePositionY);
    }

    public int UseSelectedCard()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return -1;

        int cardType = GetSelectedCardType();

        Transform selectedTf = transform.Find(singleLaneElement.selectedCard);
        if (selectedTf != null)
        {
            Destroy(selectedTf.gameObject);
        }

        singleLaneElement.RemoveSelectedCardFromHand();
        return cardType;
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
        singleLaneElement.outCount = 0;
        UpdateOutUI();
    }

    public int GetHandCount()
    {
        return singleLaneElement.handCard.Count;
    }

    public List<int> GetRemainCards()
    {
        return new List<int>(singleLaneElement.handCard.Keys);
    }

    public void AISelectCard()
    {
        List<int> cardKeys = new List<int>(singleLaneElement.handCard.Keys);
        if (cardKeys.Count == 0)
        {
            singleLaneElement.selectedCard = "";
            return;
        }

        int randomNum = Random.Range(0, cardKeys.Count);
        int selectedCard = cardKeys[randomNum];
        singleLaneElement.selectedCard = "Card_" + selectedCard;
    }

    public void RefreshHandUI()
    {
        ClearHandObjectsOnly();

        Vector2 position = new Vector2(0, handPositionY);
        int positionX = -3 * 250;

        foreach (var item in singleLaneElement.handCard)
        {
            GameObject temp = Object.Instantiate(cardPrefab, transform);

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
                    txt.text = item.Value.ToString();
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
            Object.Destroy(obj);
        }
    }

    private void UpdateScoreUI()
    {
        Transform scoreTf = transform.Find("Score");
        if (scoreTf != null)
        {
            Text txt = scoreTf.GetComponent<Text>();
            if (txt != null)
            {
                txt.text = "점수 : " + singleLaneElement.score;
            }
        }
    }

    private void UpdateOutUI()
    {
        Transform outTf = transform.Find("Out");
        if (outTf != null)
        {
            Text txt = outTf.GetComponent<Text>();
            if (txt != null)
            {
                txt.text = "아웃 : " + singleLaneElement.outCount;
            }
        }
    }
}