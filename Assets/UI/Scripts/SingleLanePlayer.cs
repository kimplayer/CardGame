using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleLanePlayer : MonoBehaviour
{
    public GameObject[] cardPrefabs;
    public int handPositionY;
    public int setPositionY;
    public int battlePositionY;

    public Text scoreText;

    [Header("Visible UI Group")]
    public GameObject outPanelObject;
    public GameObject baseDiamondObject;

    [Header("Out Count UI")]
    public OutCountUI outCountUI;

    [Header("UI Colors")]
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.gray;

    [Header("Base Diamond")]
    public BaseDiamond baseDiamond;

    private SingleLaneElement singleLaneElement;
    private bool opponent;

    [Header("카드 뒷면")]
    public Sprite cardBackSprite;

    private const int MAX_HAND_COUNT = 11;

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
        singleLaneElement.handCard.Clear();
        singleLaneElement.setCard.Clear();
        singleLaneElement.discard.Clear();

        if (!isOpponent && DeckData.Instance != null && DeckData.Instance.useCustomDeck)
            singleLaneElement.SetCustomDeck(DeckData.Instance.playerDeck);
        else
        {
            singleLaneElement.ResetDeck();
            singleLaneElement.ShuffleDeck();
        }

        DrawStartHand();
        RefreshAllUI();
    }

    public List<string> DrawStartHand()
    {
        singleLaneElement.DrawCards(5);
        List<string> discarded = TrimHandToLimit();
        RefreshHandUI();
        return discarded;
    }

    public List<string> DrawTurnCards()
    {
        singleLaneElement.DrawCards(3);
        List<string> discarded = TrimHandToLimit();
        RefreshHandUI();
        return discarded;
    }

    public List<string> TrimHandToLimit()
    {
        List<string> discardedNames = new List<string>();

        while (singleLaneElement.handCard.Count > MAX_HAND_COUNT)
        {
            List<int> keys = new List<int>(singleLaneElement.handCard.Keys);
            if (keys.Count == 0) break;

            int rand = Random.Range(0, keys.Count);
            int key = keys[rand];
            CardId removedCard = singleLaneElement.handCard[key];

            discardedNames.Add(GetCardName(removedCard));
            singleLaneElement.handCard.Remove(key);
            singleLaneElement.discard.Add(removedCard);
        }

        return discardedNames;
    }

    public bool CheckSelectedCard()
    {
        return !string.IsNullOrEmpty(singleLaneElement.selectedCard);
    }

    public CardId GetSelectedCardId()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return CardId.Hit;

        string[] split = singleLaneElement.selectedCard.Split('_');
        int key;
        if (split.Length < 2 || !int.TryParse(split[1], out key)) return CardId.Hit;
        if (!singleLaneElement.handCard.ContainsKey(key)) return CardId.Hit;

        return singleLaneElement.handCard[key];
    }

    public CardCategory GetCardCategory(CardId id)
    {
        switch (id)
        {
            case CardId.Hit:
            case CardId.Double:
            case CardId.Triple:
            case CardId.HomeRun:
            case CardId.Steal:
            case CardId.Bunt:
                return CardCategory.Attack;

            case CardId.GreatCatch:
            case CardId.DoublePlay:
            case CardId.TriplePlay:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                return CardCategory.Defense;

            case CardId.Dazzle:
            case CardId.BadBounce:
                return CardCategory.Trap;

            case CardId.PinchHitter:
            case CardId.PinchRunner:
            case CardId.PitcherChange:
            case CardId.DefensiveSub:
                return CardCategory.Draw;
        }

        return CardCategory.Attack;
    }

    public string GetCardName(CardId id)
    {
        switch (id)
        {
            case CardId.Hit: return "안타";
            case CardId.Double: return "2루타";
            case CardId.Triple: return "3루타";
            case CardId.HomeRun: return "홈런";
            case CardId.Steal: return "도루";
            case CardId.Bunt: return "번트";
            case CardId.GreatCatch: return "호수비";
            case CardId.DoublePlay: return "더블플레이";
            case CardId.TriplePlay: return "삼중살";
            case CardId.LookingStrikeOut: return "루킹삼진";
            case CardId.SwingStrikeOut: return "헛스윙삼진";
            case CardId.Dazzle: return "눈부심";
            case CardId.BadBounce: return "불규칙 바운드";
            case CardId.PinchHitter: return "대타";
            case CardId.PinchRunner: return "대주자";
            case CardId.PitcherChange: return "투수교체";
            case CardId.DefensiveSub: return "대수비";
        }

        return "알 수 없음";
    }

    public bool SetSelectedCard()
    {
        bool result = singleLaneElement.SetSelectedCardFromHand();
        RefreshHandUI();
        RefreshSetUI();
        return result;
    }

    public CardId UseSelectedCard()
    {
        CardId id = singleLaneElement.RemoveSelectedCardFromHand();
        RefreshHandUI();
        return id;
    }

    public void AddOut(int value = 1)
    {
        singleLaneElement.outCount += value;
        UpdateOutUI();
    }

    public int GetOutCount() => singleLaneElement.outCount;
    public int GetScore() => singleLaneElement.score;
    public int GetHandCount() => singleLaneElement.handCard.Count;
    public int GetSetCount() => singleLaneElement.setCard.Count;
    public int GetRunnerCount() => singleLaneElement.RunnerCount();

    public void ResetOutCount()
    {
        singleLaneElement.ResetOutCount();

        if (outCountUI != null)
            outCountUI.ResetOutCount();
    }

    public void ResetBases()
    {
        singleLaneElement.ResetBases();

        if (baseDiamond != null)
            baseDiamond.ResetBases();
    }

    public void AddScore(int value)
    {
        singleLaneElement.score += value;
        UpdateScoreUI();
    }

    public bool HasRunnerOnFirst() => singleLaneElement.firstBase;
    public bool HasRunnerOnSecond() => singleLaneElement.secondBase;
    public bool HasRunnerOnThird() => singleLaneElement.thirdBase;

    public bool HasAnyRunner() =>
        singleLaneElement.firstBase ||
        singleLaneElement.secondBase ||
        singleLaneElement.thirdBase;

    public Dictionary<int, CardId> GetHandCardDict() => singleLaneElement.handCard;
    public Dictionary<int, CardId> GetSetCardDict() => singleLaneElement.setCard;

    public void SetSelectedCardByKey(int key)
    {
        singleLaneElement.selectedCard = "Card_" + key;
    }

    public void AdvanceAllRunnersOneBase()
    {
        bool oldFirst = singleLaneElement.firstBase;
        bool oldSecond = singleLaneElement.secondBase;
        bool oldThird = singleLaneElement.thirdBase;

        singleLaneElement.firstBase = false;
        singleLaneElement.secondBase = false;
        singleLaneElement.thirdBase = false;

        if (oldThird) AddScore(1);
        if (oldSecond) singleLaneElement.thirdBase = true;
        if (oldFirst) singleLaneElement.secondBase = true;

        UpdateBaseUI();
    }

    private int RemoveOneRunner()
    {
        if (singleLaneElement.thirdBase)
        {
            singleLaneElement.thirdBase = false;
            UpdateBaseUI();
            return 1;
        }
        if (singleLaneElement.secondBase)
        {
            singleLaneElement.secondBase = false;
            UpdateBaseUI();
            return 1;
        }
        if (singleLaneElement.firstBase)
        {
            singleLaneElement.firstBase = false;
            UpdateBaseUI();
            return 1;
        }
        return 0;
    }

    private int RemoveRunners(int count)
    {
        int removed = 0;
        for (int i = 0; i < count; i++)
            removed += RemoveOneRunner();

        UpdateBaseUI();
        return removed;
    }

    public void ApplyAttackCard(CardId cardId)
    {
        switch (cardId)
        {
            case CardId.Hit: AdvanceRunners(1, true); break;
            case CardId.Double: AdvanceRunners(2, true); break;
            case CardId.Triple: AdvanceRunners(3, true); break;
            case CardId.HomeRun: HomeRun(); break;
            case CardId.Steal: AdvanceRunners(1, false); break;
            case CardId.Bunt: ApplyBunt(); break;
        }
    }

    public string ApplyDrawCard(CardId cardId, SingleLanePlayer enemy)
    {
        switch (cardId)
        {
            case CardId.PinchHitter:
                {
                    singleLaneElement.DrawCards(3);
                    List<string> removed = TrimHandToLimit();
                    RefreshHandUI();
                    if (removed.Count > 0)
                        return "카드 3장을 드로우했다. 손패 초과로 " +
                               string.Join(", ", removed) + " 버림.";
                    return "카드 3장을 드로우했다.";
                }
            case CardId.PinchRunner:
                {
                    singleLaneElement.DrawCards(2);
                    List<string> removed = TrimHandToLimit();
                    RefreshHandUI();
                    if (removed.Count > 0)
                        return "카드 2장을 드로우했다. 손패 초과로 " +
                               string.Join(", ", removed) + " 버림.";
                    return "카드 2장을 드로우했다.";
                }
            case CardId.PitcherChange:
                enemy.RemoveRandomHandCards(2);
                return "상대 패 2장을 제거했다.";

            case CardId.DefensiveSub:
                enemy.RemoveRandomHandCards(1);
                return "상대 패 1장을 제거했다.";
        }

        return "";
    }

    public void RemoveRandomHandCards(int count)
    {
        List<int> keys = new List<int>(singleLaneElement.handCard.Keys);

        for (int i = 0; i < count; i++)
        {
            if (keys.Count == 0) break;

            int rand = Random.Range(0, keys.Count);
            int key = keys[rand];
            CardId id = singleLaneElement.handCard[key];

            singleLaneElement.handCard.Remove(key);
            singleLaneElement.discard.Add(id);
            keys.RemoveAt(rand);
        }

        RefreshHandUI();
    }

    public bool TryActivateDefenseOrTrap(CardId attackCard,
                                         SingleLanePlayer attacker,
                                         out string activatedName)
    {
        activatedName = "";
        List<int> keys = new List<int>(singleLaneElement.setCard.Keys);

        bool attackCanceled = false;
        bool defenseTriggered = false;

        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];
            CardId setId = singleLaneElement.setCard[key];

            if (CanActivateDefense(setId, attackCard, attacker))
            {
                bool canceledByBadBounce = attacker.TryActivateBadBounce();

                if (!canceledByBadBounce)
                {
                    ApplyDefenseEffect(setId, attacker);
                    RemoveSetCardByKey(key);
                    activatedName = GetCardName(setId);
                    attackCanceled = true;
                    defenseTriggered = true;
                    break;
                }
                else
                {
                    activatedName = "불규칙 바운드";
                    RemoveSetCardByKey(key);
                }
            }
        }

        if (!attackCanceled)
        {
            bool dazzled = attacker.TryActivateDazzle(attacker);
            if (dazzled) activatedName = "눈부심";
        }

        return defenseTriggered;
    }

    public bool TryActivateBadBounce()
    {
        List<int> keys = new List<int>(singleLaneElement.setCard.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];
            if (singleLaneElement.setCard[key] == CardId.BadBounce)
            {
                RemoveSetCardByKey(key);
                return true;
            }
        }

        return false;
    }

    public bool TryActivateDazzle(SingleLanePlayer attacker)
    {
        List<int> keys = new List<int>(singleLaneElement.setCard.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];
            if (singleLaneElement.setCard[key] == CardId.Dazzle)
            {
                attacker.AdvanceAllRunnersOneBase();
                RemoveSetCardByKey(key);
                return true;
            }
        }

        return false;
    }

    private bool CanActivateDefense(CardId defenseId, CardId attackCard,
                                    SingleLanePlayer attacker)
    {
        bool isHitType =
            attackCard == CardId.Hit ||
            attackCard == CardId.Double ||
            attackCard == CardId.Triple;

        bool isHitOrDouble =
            attackCard == CardId.Hit ||
            attackCard == CardId.Double;

        switch (defenseId)
        {
            case CardId.GreatCatch:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                return isHitType;

            case CardId.DoublePlay:
                return attacker.HasAnyRunner() && isHitOrDouble;

            case CardId.TriplePlay:
                return attacker.GetRunnerCount() >= 2 &&
                       attackCard == CardId.Hit;
        }

        return false;
    }

    private void ApplyDefenseEffect(CardId defenseId, SingleLanePlayer attacker)
    {
        switch (defenseId)
        {
            case CardId.GreatCatch:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                attacker.AddOut(1); break;

            case CardId.DoublePlay:
                attacker.AddOut(2);
                attacker.RemoveRunners(1); break;

            case CardId.TriplePlay:
                attacker.AddOut(2);
                attacker.RemoveRunners(2); break;
        }
    }

    private void RemoveSetCardByKey(int key)
    {
        CardId temp;
        singleLaneElement.RemoveSetCard(key, out temp);
        RefreshSetUI();
    }

    private void AdvanceRunners(int value, bool batterAdvance)
    {
        int runs = 0;
        bool oldFirst = singleLaneElement.firstBase;
        bool oldSecond = singleLaneElement.secondBase;
        bool oldThird = singleLaneElement.thirdBase;

        singleLaneElement.firstBase = false;
        singleLaneElement.secondBase = false;
        singleLaneElement.thirdBase = false;

        if (oldThird) { if (3 + value >= 4) runs++; else SetBase(3 + value); }
        if (oldSecond) { if (2 + value >= 4) runs++; else SetBase(2 + value); }
        if (oldFirst) { if (1 + value >= 4) runs++; else SetBase(1 + value); }
        if (batterAdvance) { if (value >= 4) runs++; else SetBase(value); }

        AddScore(runs);
        UpdateBaseUI();
    }

    private void ApplyBunt()
    {
        if (singleLaneElement.firstBase)
        {
            singleLaneElement.firstBase = false;

            if (singleLaneElement.secondBase)
            {
                if (singleLaneElement.thirdBase) AddScore(1);
                else singleLaneElement.thirdBase = true;
            }

            singleLaneElement.secondBase = true;
        }

        UpdateBaseUI();
    }

    private void HomeRun()
    {
        int runs = 1;
        if (singleLaneElement.firstBase) runs++;
        if (singleLaneElement.secondBase) runs++;
        if (singleLaneElement.thirdBase) runs++;

        singleLaneElement.ResetBases();
        AddScore(runs);
        UpdateBaseUI();
    }

    private void SetBase(int baseNum)
    {
        switch (baseNum)
        {
            case 1: singleLaneElement.firstBase = true; break;
            case 2: singleLaneElement.secondBase = true; break;
            case 3: singleLaneElement.thirdBase = true; break;
        }
    }

    public void RefreshAllUI()
    {
        UpdateScoreUI();
        UpdateOutUI();
        UpdateBaseUI();
        RefreshHandUI();
        RefreshSetUI();
    }

    public void RefreshHandUI()
    {
        ClearObjectsByPrefix("Card_");

        Vector2 position = new Vector2(0, handPositionY);
        int gap = 180;
        int startX = -(Mathf.Max(0, singleLaneElement.handCard.Count - 1) * gap) / 2;
        int index = 0;

        foreach (var item in singleLaneElement.handCard)
        {
            // 카드 ID에 맞는 프리팹 선택
            GameObject prefab = GetCardPrefab(item.Value);
            if (prefab == null) continue;

            GameObject temp = Instantiate(prefab, transform);
            position.x = startX + (index * gap);
            temp.transform.localPosition = position;

            Card cardComp = temp.GetComponent<Card>();
            Button buttonComp = temp.GetComponent<Button>();

            if (cardComp != null)
            {
                cardComp.cardId = item.Value;
                cardComp.category = GetCardCategory(item.Value);
            }

            // 상대 카드면 이미지 숨기고 ??? 표시
            Transform textTf = temp.transform.Find("Text");
            if (textTf != null)
            {
                Text txt = textTf.GetComponent<Text>();
                if (txt != null)
                    txt.text = "";
            }

            // 상대 카드면 카드 이미지도 숨기기
            Image cardImage = temp.GetComponent<Image>();
            if (cardImage != null && opponent)
                cardImage.sprite = GetBackSprite(); // 카드 뒷면 이미지

            temp.name = "Card_" + item.Key;

            if (!opponent)
            {
                CardDragHandler dragHandler = temp.GetComponent<CardDragHandler>();
                if (dragHandler == null)
                    dragHandler = temp.AddComponent<CardDragHandler>();

                dragHandler.cardId = item.Value;
                dragHandler.category = GetCardCategory(item.Value);
                dragHandler.cardKey = "Card_" + item.Key;

                LongPressInfo longPress = temp.GetComponent<LongPressInfo>();
                if (longPress == null)
                    longPress = temp.AddComponent<LongPressInfo>();

                if (buttonComp != null)
                {
                    buttonComp.onClick.RemoveAllListeners();
                    buttonComp.interactable = true;
                }

                if (cardComp != null) cardComp.SetInfo();
            }
            else
            {
                if (buttonComp != null)
                    buttonComp.interactable = false;

                if (cardComp != null) cardComp.HideInfo();
            }

            index++;
        }
    }

    // 카드 ID에 맞는 프리팹 반환
    private GameObject GetCardPrefab(CardId id)
    {
        int index = (int)id;
        if (cardPrefabs == null || index >= cardPrefabs.Length) return null;
        if (cardPrefabs[index] == null) return null;
        return cardPrefabs[index];
    }

    public void RefreshSetUI()
    {
        ClearObjectsByPrefix("Set_");

        Vector2 position = new Vector2(0, setPositionY);
        int gap = 140;
        int startX = -(Mathf.Max(0, singleLaneElement.setCard.Count - 1) * gap) / 2;
        int index = 0;

        foreach (var item in singleLaneElement.setCard)
        {
            // 세트존은 뒷면 프리팹 사용 (카드 숨김)
            GameObject prefab = GetCardPrefab(item.Value);
            if (prefab == null) continue;

            GameObject temp = Instantiate(prefab, transform);
            position.x = startX + (index * gap);
            temp.transform.localPosition = position;

            Card cardComp = temp.GetComponent<Card>();
            Button buttonComp = temp.GetComponent<Button>();

            if (cardComp != null)
            {
                cardComp.cardId = item.Value;
                cardComp.category = GetCardCategory(item.Value);

                if (opponent) cardComp.HideInfo();
                else cardComp.SetInfo();
            }

            // 세트존은 항상 SET 텍스트 표시
            Transform textTf = temp.transform.Find("Text");
            if (textTf != null)
            {
                Text txt = textTf.GetComponent<Text>();
                if (txt != null) txt.text = "";
            }

            // 상대 세트존은 뒷면 이미지로 교체
            if (opponent)
            {
                Image cardImage = temp.GetComponent<Image>();
                if (cardImage != null)
                    cardImage.sprite = GetBackSprite();
            }

            temp.name = "Set_" + item.Key;

            if (buttonComp != null)
                buttonComp.interactable = false;

            index++;
        }
    }

    private void ClearObjectsByPrefix(string prefix)
    {
        List<GameObject> deleteList = new List<GameObject>();

        // transform 자식뿐 아니라 비활성화된 것도 포함해서 삭제
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith(prefix))
                deleteList.Add(child.gameObject);
        }

        // Canvas 최상단으로 이동한 카드도 찾아서 삭제
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            foreach (Transform child in rootCanvas.transform)
            {
                if (child.name.StartsWith(prefix))
                {
                    CardDragHandler drag = child.GetComponent<CardDragHandler>();
                    // 이 플레이어 소유의 카드인지 확인
                    if (drag != null && drag.cardKey != null &&
                        drag.cardKey.StartsWith(prefix))
                        deleteList.Add(child.gameObject);
                }
            }
        }

        foreach (GameObject obj in deleteList)
            Destroy(obj);
    }

    private void UpdateScoreUI()
    {
        if (scoreText == null) return;
        scoreText.text = opponent
            ? "상대 점수 : " + singleLaneElement.score
            : "내 점수 : " + singleLaneElement.score;
    }

    private void UpdateOutUI()
    {
        if (outCountUI != null)
            outCountUI.UpdateOutCount(singleLaneElement.outCount);
    }

    private void UpdateBaseUI()
    {
        if (baseDiamond != null)
            baseDiamond.UpdateBases(
                singleLaneElement.firstBase,
                singleLaneElement.secondBase,
                singleLaneElement.thirdBase
            );
    }

    private void SetBaseImage(Image targetImage, bool active)
    {
        if (targetImage == null) return;
        targetImage.color = active ? activeColor : inactiveColor;
    }

    private void SetOutImage(Image targetImage, bool active)
    {
        if (targetImage == null) return;
        targetImage.color = active ? activeColor : inactiveColor;
    }

    public void SetFieldUIVisible(bool visible)
    {
        if (outPanelObject != null)
            outPanelObject.SetActive(visible);

        if (baseDiamondObject != null)
            baseDiamondObject.SetActive(visible);
    }

    private Sprite GetBackSprite()
    {
        return cardBackSprite;
    }
}