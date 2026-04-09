using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using Unity.Properties;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 플레이어 1명의 행동, 카드 사용, UI 갱신을 담당하는 스크립트

public class SingleLanePlayer : MonoBehaviour
{
    public GameObject cardPrefab;
    public int handPositionY;
    public int setPositionY;
    public int battlePositionY;

    public Text scoreText;

    [Header("VIsible UI Group")]
    public GameObject outPanelObject;
    public GameObject baseDiamondObject;

    [Header("Base Diamond UI")]
    public Image firstBaseImage;
    public Image secondBaseImage;
    public Image thirdBaseImage;

    [Header("Out Count UI")]
    public Image out1Image;
    public Image out2Image;
    public Image out3Image;

    [Header("UI Colors")]
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.gray;

    private SingleLaneElement singleLaneElement;
    private bool opponent;

    private const int MAX_HAND_COUNT = 11;

    private void Awake()
    {
        singleLaneElement = new SingleLaneElement();
    }

    // 플레이어 상태 초기화
    public void Initialize(bool isOpponent)
    {
        opponent = isOpponent;

        singleLaneElement.score = 0;
        singleLaneElement.ResetOutCount();
        singleLaneElement.ResetBases();
        singleLaneElement.handCard.Clear();
        singleLaneElement.setCard.Clear();
        singleLaneElement.discard.Clear();

        // 플레이어만 커스텀 덱 적용, 상대는 기본 덱
        if (!isOpponent && DeckData.Instance != null && DeckData.Instance.useCustomDeck)
        {
            singleLaneElement.SetCustomDeck(DeckData.Instance.playerDeck);
        }
        else
        {
            singleLaneElement.ResetDeck();
            singleLaneElement.ShuffleDeck();
        }

        DrawStartHand();
        RefreshAllUI();
    }

    // 시작 손패 5장 드로우 + 초과 정리
    public List<string> DrawStartHand()
    {
        singleLaneElement.DrawCards(5);
        List<string> discarded = TrimHandToLimit();
        RefreshHandUI();
        return discarded;
    }

    // 턴 시작 시 3장 드로우 + 초과 정리
    public List<string> DrawTurnCards()
    {
        singleLaneElement.DrawCards(3);
        List<string> discarded = TrimHandToLimit();
        RefreshHandUI();
        return discarded;
    }

    // 손패 최대치 11장 유지, 초과분 랜덤 버림
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

    // 카드 클릭 시 선택 처리
    public void ClickCard()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;
        singleLaneElement.selectedCard = EventSystem.current.currentSelectedGameObject.name;
    }

    // 카드 선택 여부
    public bool CheckSelectedCard()
    {
        return !string.IsNullOrEmpty(singleLaneElement.selectedCard);
    }

    // 선택 카드 ID 반환
    public CardId GetSelectedCardId()
    {
        if (string.IsNullOrEmpty(singleLaneElement.selectedCard)) return CardId.Hit;

        string[] split = singleLaneElement.selectedCard.Split('_');
        int key;
        if (split.Length < 2 || !int.TryParse(split[1], out key)) return CardId.Hit;
        if (!singleLaneElement.handCard.ContainsKey(key)) return CardId.Hit;

        return singleLaneElement.handCard[key];
    }

    // 카드 분류 반환
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

    // 카드 이름 반환
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

    // 선택 카드를 세트존으로 이동
    public bool SetSelectedCard()
    {
        bool result = singleLaneElement.SetSelectedCardFromHand();
        RefreshHandUI();
        RefreshSetUI();
        return result;
    }

    // 선택 카드를 사용 처리
    public CardId UseSelectedCard()
    {
        CardId id = singleLaneElement.RemoveSelectedCardFromHand();
        RefreshHandUI();
        return id;
    }

    // 아웃 증가
    public void AddOut(int value = 1)
    {
        singleLaneElement.outCount += value;
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

    // 베이스 초기화
    public void ResetBases()
    {
        singleLaneElement.ResetBases();
        UpdateBaseUI();
    }

    // 점수 증가
    public void AddScore(int value)
    {
        singleLaneElement.score += value;
        UpdateScoreUI();
    }

    public int GetScore()
    {
        return singleLaneElement.score;
    }

    public int GetHandCount()
    {
        return singleLaneElement.handCard.Count;
    }

    public int GetSetCount()
    {
        return singleLaneElement.setCard.Count;
    }

    public int GetRunnerCount()
    {
        return singleLaneElement.RunnerCount();
    }

    public bool HasRunnerOnFirst()
    {
        return singleLaneElement.firstBase;
    }

    public bool HasAnyRunner()
    {
        return singleLaneElement.firstBase || singleLaneElement.secondBase || singleLaneElement.thirdBase;
    }

    // 모든 주자를 1루씩 진루
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

    // 가장 앞선 주자부터 1명 제거
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

    // 주자를 최대 count명 제거
    private int RemoveRunners(int count)
    {
        int removed = 0;
        for (int i = 0; i < count; i++)
            removed += RemoveOneRunner();

        UpdateBaseUI();
        return removed;
    }

    // 공격카드 적용
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

    // 드로우/특수카드 적용
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
                        return "카드 3장을 드로우했다. 손패 초과로 " + string.Join(", ", removed) + " 버림.";
                    return "카드 3장을 드로우했다.";
                }
            case CardId.PinchRunner:
                {
                    singleLaneElement.DrawCards(2);
                    List<string> removed = TrimHandToLimit();
                    RefreshHandUI();
                    if (removed.Count > 0)
                        return "카드 2장을 드로우했다. 손패 초과로 " + string.Join(", ", removed) + " 버림.";
                    return "카드 2장을 드로우했다.";
                }
            case CardId.PitcherChange:
                enemy.RemoveRandomHandCards(2);
                RefreshHandUI();
                return "상대 패 2장을 제거했다.";

            case CardId.DefensiveSub:
                enemy.RemoveRandomHandCards(1);
                RefreshHandUI();
                return "상대 패 1장을 제거했다.";
        }

        RefreshHandUI();
        return "";
    }

    // 손패 랜덤 제거
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

    // 수비/함정 발동 처리
    public bool TryActivateDefenseOrTrap(CardId attackCard, SingleLanePlayer attacker, out string activatedName)
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
            if (dazzled)
                activatedName = "눈부심";
        }

        return defenseTriggered;
    }

    // 불규칙 바운드 발동
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

    // 눈부심 발동
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

    // 수비카드 발동 조건 판정
    private bool CanActivateDefense(CardId defenseId, CardId attackCard, SingleLanePlayer attacker)
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
                return attacker.GetRunnerCount() >= 2 && attackCard == CardId.Hit;
        }

        return false;
    }

    // 수비카드 효과 적용
    private void ApplyDefenseEffect(CardId defenseId, SingleLanePlayer attacker)
    {
        switch (defenseId)
        {
            case CardId.GreatCatch:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                attacker.AddOut(1);
                break;

            case CardId.DoublePlay:
                attacker.AddOut(2);
                attacker.RemoveRunners(1);
                break;

            case CardId.TriplePlay:
                attacker.AddOut(2);
                attacker.RemoveRunners(2);
                break;
        }
    }

    // 세트 카드 제거
    private void RemoveSetCardByKey(int key)
    {
        CardId temp;
        singleLaneElement.RemoveSetCard(key, out temp);
        RefreshSetUI();
    }

    // 주자 진루 처리
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

    // 번트 처리
    private void ApplyBunt()
    {
        if (singleLaneElement.firstBase)
        {
            singleLaneElement.firstBase = false;

            if (singleLaneElement.secondBase)
            {
                if (singleLaneElement.thirdBase)
                    AddScore(1);
                else
                    singleLaneElement.thirdBase = true;
            }

            singleLaneElement.secondBase = true;
        }

        UpdateBaseUI();
    }

    // 홈런 처리
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

    // 베이스 점유
    private void SetBase(int baseNum)
    {
        switch (baseNum)
        {
            case 1: singleLaneElement.firstBase = true; break;
            case 2: singleLaneElement.secondBase = true; break;
            case 3: singleLaneElement.thirdBase = true; break;
        }
    }

    // AI 카드 선택
    public void AISelectPlayableCard(bool preferSetCard)
    {
        if (preferSetCard)
        {
            foreach (var item in singleLaneElement.handCard)
            {
                CardCategory cat = GetCardCategory(item.Value);
                if (cat == CardCategory.Defense || cat == CardCategory.Trap)
                {
                    singleLaneElement.selectedCard = "Card_" + item.Key;
                    return;
                }
            }
        }

        List<int> keys = new List<int>(singleLaneElement.handCard.Keys);
        if (keys.Count == 0)
        {
            singleLaneElement.selectedCard = "";
            return;
        }

        int rand = Random.Range(0, keys.Count);
        singleLaneElement.selectedCard = "Card_" + keys[rand];
    }

    // 전체 UI 갱신
    public void RefreshAllUI()
    {
        UpdateScoreUI();
        UpdateOutUI();
        UpdateBaseUI();
        RefreshHandUI();
        RefreshSetUI();
    }

    // 손패 UI 재생성
    public void RefreshHandUI()
    {
        ClearObjectsByPrefix("Card_");

        Vector2 position = new Vector2(0, handPositionY);
        int gap = 180;
        int startX = -(Mathf.Max(0, singleLaneElement.handCard.Count - 1) * gap) / 2;

        int index = 0;
        foreach (var item in singleLaneElement.handCard)
        {
            GameObject temp = Instantiate(cardPrefab, transform);
            position.x = startX + (index * gap);
            temp.transform.localPosition = position;

            Card cardComp = temp.GetComponent<Card>();
            Button buttonComp = temp.GetComponent<Button>();

            if (cardComp != null)
            {
                cardComp.cardId = item.Value;
                cardComp.category = GetCardCategory(item.Value);
            }

            Transform textTf = temp.transform.Find("Text");
            if (textTf != null)
            {
                Text txt = textTf.GetComponent<Text>();
                if (txt != null)
                {
                    if (opponent)
                        txt.text = "???";
                    else if (cardComp != null)
                        txt.text = cardComp.GetCardName();
                }
            }

            temp.name = "Card_" + item.Key;

            if (buttonComp != null)
            {
                buttonComp.onClick.RemoveAllListeners();
                buttonComp.onClick.AddListener(ClickCard);

                if (opponent)
                    buttonComp.interactable = false;
            }

            if (cardComp != null)
            {
                if (opponent)
                    cardComp.HideInfo();
                else
                    cardComp.SetInfo();
            }

            index++;
        }
    }

    // 세트존 UI 재생성
    public void RefreshSetUI()
    {
        ClearObjectsByPrefix("Set_");

        Vector2 position = new Vector2(0, setPositionY);
        int gap = 140;
        int startX = -(Mathf.Max(0, singleLaneElement.setCard.Count - 1) * gap) / 2;

        int index = 0;
        foreach (var item in singleLaneElement.setCard)
        {
            GameObject temp = Instantiate(cardPrefab, transform);
            position.x = startX + (index * gap);
            temp.transform.localPosition = position;

            Card cardComp = temp.GetComponent<Card>();
            Button buttonComp = temp.GetComponent<Button>();

            if (cardComp != null)
            {
                cardComp.cardId = item.Value;
                cardComp.category = GetCardCategory(item.Value);

                if (opponent)
                    cardComp.HideInfo();
                else
                    cardComp.SetInfo();
            }

            Transform textTf = temp.transform.Find("Text");
            if (textTf != null)
            {
                Text txt = textTf.GetComponent<Text>();
                if (txt != null)
                    txt.text = "SET";
            }

            temp.name = "Set_" + item.Key;

            if (buttonComp != null)
                buttonComp.interactable = false;

            index++;
        }
    }

    // 특정 접두사 가진 UI 카드 삭제
    private void ClearObjectsByPrefix(string prefix)
    {
        List<GameObject> deleteList = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith(prefix))
                deleteList.Add(child.gameObject);
        }

        foreach (GameObject obj in deleteList)
            Destroy(obj);
    }

    // 점수 UI 갱신
    private void UpdateScoreUI()
    {
        if (scoreText == null) return;
        scoreText.text = opponent ? "상대 점수 : " + singleLaneElement.score
                                  : "내 점수 : " + singleLaneElement.score;
    }

    // 아웃카운트 UI 갱신
    private void UpdateOutUI()
    {
        SetOutImage(out1Image, singleLaneElement.outCount >= 1);
        SetOutImage(out2Image, singleLaneElement.outCount >= 2);
        SetOutImage(out3Image, singleLaneElement.outCount >= 3);
    }

    // 베이스 다이아몬드 UI 갱신
    private void UpdateBaseUI()
    {
        SetBaseImage(firstBaseImage, singleLaneElement.firstBase);
        SetBaseImage(secondBaseImage, singleLaneElement.secondBase);
        SetBaseImage(thirdBaseImage, singleLaneElement.thirdBase);
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

    // 아웃패널과 베이스 다이아 UI 표시 여부
    public void SetFieldUIVisible(bool visible)
    {
        if (outPanelObject != null)
            outPanelObject.SetActive(visible);

        if (baseDiamondObject != null)
            baseDiamondObject.SetActive(visible);
    }
}