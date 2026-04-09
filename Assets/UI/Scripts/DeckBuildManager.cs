using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckBuildManager : MonoBehaviour
{
    [Header("UI 참조")]
    public Transform cardPoolParent;   // 왼쪽 카드 풀 패널의 Content
    public Transform deckListParent;   // 오른쪽 덱 목록 패널의 Content
    public Text deckCountText;         // 총 장수 표시
    public Text infoText;              // 안내 문구
    public Button startButton;         // 게임 시작 버튼
    public GameObject cardButtonPrefab; // 카드 버튼 프리팹

    private const int MIN_DECK = 25;
    private const int MAX_DECK = 40;
    private const int MAX_SAME_CARD = 3;

    // 현재 덱 구성 (CardId → 장수)
    private Dictionary<CardId, int> deckDict = new Dictionary<CardId, int>();

    // 카드 풀 목록 (17종)
    private readonly CardId[] allCardIds = new CardId[]
    {
        CardId.Hit, CardId.Double, CardId.Triple, CardId.HomeRun,
        CardId.Steal, CardId.Bunt,
        CardId.GreatCatch, CardId.DoublePlay, CardId.TriplePlay,
        CardId.LookingStrikeOut, CardId.SwingStrikeOut,
        CardId.Dazzle, CardId.BadBounce,
        CardId.PinchHitter, CardId.PinchRunner,
        CardId.PitcherChange, CardId.DefensiveSub
    };

    private void Start()
    {
        // DeckData 싱글톤 없으면 생성
        if (DeckData.Instance == null)
        {
            GameObject obj = new GameObject("DeckData");
            obj.AddComponent<DeckData>();
        }

        BuildCardPool();
        RefreshDeckUI();
        UpdateStartButton();

        if (infoText != null)
            infoText.text = $"덱 구성 : {MIN_DECK}~{MAX_DECK}장 / 같은 카드 최대 {MAX_SAME_CARD}장";
    }

    // 왼쪽 카드 풀 UI 생성
    private void BuildCardPool()
    {
        foreach (CardId id in allCardIds)
        {
            GameObject btn = Instantiate(cardButtonPrefab, cardPoolParent);
            btn.name = "Pool_" + id.ToString();

            // 카드 이름 텍스트
            Transform nameTf = btn.transform.Find("NameText");
            if (nameTf != null)
            {
                Text nameText = nameTf.GetComponent<Text>();
                if (nameText != null)
                    nameText.text = GetCardName(id) + "\n[" + GetCategoryName(id) + "]";
            }

            // 추가 버튼
            Transform addBtnTf = btn.transform.Find("AddButton");
            if (addBtnTf != null)
            {
                Button addBtn = addBtnTf.GetComponent<Button>();
                CardId capturedId = id;
                if (addBtn != null)
                    addBtn.onClick.AddListener(() => AddCard(capturedId));
            }
        }
    }

    // 카드 추가
    public void AddCard(CardId id)
    {
        int totalCount = GetTotalDeckCount();

        if (totalCount >= MAX_DECK)
        {
            ShowMessage($"덱은 최대 {MAX_DECK}장까지 구성할 수 있습니다.");
            return;
        }

        if (deckDict.ContainsKey(id) && deckDict[id] >= MAX_SAME_CARD)
        {
            ShowMessage($"같은 카드는 최대 {MAX_SAME_CARD}장까지 넣을 수 있습니다.");
            return;
        }

        if (!deckDict.ContainsKey(id))
            deckDict[id] = 0;

        deckDict[id]++;
        RefreshDeckUI();
        UpdateStartButton();
    }

    // 카드 제거
    public void RemoveCard(CardId id)
    {
        if (!deckDict.ContainsKey(id) || deckDict[id] <= 0) return;

        deckDict[id]--;
        if (deckDict[id] == 0)
            deckDict.Remove(id);

        RefreshDeckUI();
        UpdateStartButton();
    }

    // 기본 덱으로 자동 채우기
    public void ClickDefaultDeck()
    {
        deckDict.Clear();

        // ResetDeck()과 동일한 구성
        AddToDict(CardId.Hit, 4);
        AddToDict(CardId.Double, 3);
        AddToDict(CardId.Triple, 2);
        AddToDict(CardId.HomeRun, 2);
        AddToDict(CardId.Steal, 2);
        AddToDict(CardId.Bunt, 2);
        AddToDict(CardId.GreatCatch, 2);
        AddToDict(CardId.DoublePlay, 2);
        AddToDict(CardId.TriplePlay, 1);
        AddToDict(CardId.LookingStrikeOut, 2);
        AddToDict(CardId.SwingStrikeOut, 2);
        AddToDict(CardId.Dazzle, 2);
        AddToDict(CardId.BadBounce, 2);
        AddToDict(CardId.PinchHitter, 2);
        AddToDict(CardId.PinchRunner, 2);
        AddToDict(CardId.PitcherChange, 1);
        AddToDict(CardId.DefensiveSub, 1);

        RefreshDeckUI();
        UpdateStartButton();
    }

    // 덱 초기화
    public void ClickClearDeck()
    {
        deckDict.Clear();
        RefreshDeckUI();
        UpdateStartButton();
    }

    // 게임 시작
    public void ClickStartGame()
    {
        int total = GetTotalDeckCount();
        if (total < MIN_DECK || total > MAX_DECK)
        {
            ShowMessage($"덱은 {MIN_DECK}~{MAX_DECK}장이어야 합니다. (현재 {total}장)");
            return;
        }

        // DeckData에 저장
        DeckData.Instance.playerDeck.Clear();
        foreach (var pair in deckDict)
        {
            for (int i = 0; i < pair.Value; i++)
                DeckData.Instance.playerDeck.Add(pair.Key);
        }

        DeckData.Instance.useCustomDeck = true;

        SceneManager.LoadScene("SingleLane");
    }

    // 덱 목록 UI 갱신
    private void RefreshDeckUI()
    {
        // 기존 목록 삭제
        foreach (Transform child in deckListParent)
            Destroy(child.gameObject);

        foreach (var pair in deckDict)
        {
            GameObject btn = Instantiate(cardButtonPrefab, deckListParent);

            Transform nameTf = btn.transform.Find("NameText");
            if (nameTf != null)
            {
                Text nameText = nameTf.GetComponent<Text>();
                if (nameText != null)
                    nameText.text = GetCardName(pair.Key) + "  x" + pair.Value;
            }

            // 제거 버튼
            Transform removeBtnTf = btn.transform.Find("AddButton");
            if (removeBtnTf != null)
            {
                Button removeBtn = removeBtnTf.GetComponent<Button>();
                CardId capturedId = pair.Key;
                if (removeBtn != null)
                {
                    // 덱 목록에서는 버튼을 제거 용도로 사용
                    Text btnText = removeBtnTf.GetComponentInChildren<Text>();
                    if (btnText != null) btnText.text = "-";
                    removeBtn.onClick.AddListener(() => RemoveCard(capturedId));
                }
            }
        }

        // 장수 텍스트 갱신
        int total = GetTotalDeckCount();
        if (deckCountText != null)
            deckCountText.text = $"총 {total} / {MAX_DECK}장";
    }

    // 시작 버튼 활성화 조건
    private void UpdateStartButton()
    {
        if (startButton == null) return;
        int total = GetTotalDeckCount();
        startButton.interactable = (total >= MIN_DECK && total <= MAX_DECK);
    }

    private int GetTotalDeckCount()
    {
        int count = 0;
        foreach (var pair in deckDict)
            count += pair.Value;
        return count;
    }

    private void AddToDict(CardId id, int count)
    {
        if (!deckDict.ContainsKey(id))
            deckDict[id] = 0;
        deckDict[id] += count;
    }

    private void ShowMessage(string msg)
    {
        if (infoText != null)
            infoText.text = msg;
        Debug.Log(msg);
    }

    // 카드 이름
    private string GetCardName(CardId id)
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

    // 카드 분류 이름
    private string GetCategoryName(CardId id)
    {
        switch (id)
        {
            case CardId.Hit:
            case CardId.Double:
            case CardId.Triple:
            case CardId.HomeRun:
            case CardId.Steal:
            case CardId.Bunt:
                return "공격";
            case CardId.GreatCatch:
            case CardId.DoublePlay:
            case CardId.TriplePlay:
            case CardId.LookingStrikeOut:
            case CardId.SwingStrikeOut:
                return "수비";
            case CardId.Dazzle:
            case CardId.BadBounce:
                return "함정";
            case CardId.PinchHitter:
            case CardId.PinchRunner:
            case CardId.PitcherChange:
            case CardId.DefensiveSub:
                return "드로우";
        }
        return "";
    }
}