using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// 개별 카드 오브젝트에 붙는 스크립트
// 카드 이름 / 카드 설명 / 우클릭 설명창 기능을 담당
public class Card : MonoBehaviour, IPointerClickHandler
{
    public CardId cardId;
    public CardCategory category;
    public GameObject infoText;
    public bool rightClickEnabled = true;

    private GameObject instanceInfoText;

    // 카드 생성 시 설명창 프리팹 생성
    private void Awake()
    {
        if (infoText != null)
        {
            instanceInfoText = Instantiate(infoText, transform);
            instanceInfoText.transform.localPosition = new Vector2(0, 100);
            instanceInfoText.SetActive(false);
        }
    }

    // 우클릭 시 설명창 닫기
    private void Update()
    {
        if (rightClickEnabled && Input.GetMouseButtonDown(1) && instanceInfoText != null)
        {
            instanceInfoText.SetActive(false);
        }
    }

    // 우클릭 시 설명창 열기
    public void OnPointerClick(PointerEventData eventData)
    {
        if (rightClickEnabled &&
            eventData.button == PointerEventData.InputButton.Right &&
            instanceInfoText != null)
        {
            instanceInfoText.SetActive(true);
        }
    }

    // 설명창 열고 닫기 + 우클릭 허용 설정
    public void InfoTextSetActive(bool active, bool rightClickEnabledValue)
    {
        if (instanceInfoText != null)
            instanceInfoText.SetActive(active);

        rightClickEnabled = rightClickEnabledValue;
    }

    // 설명 텍스트 설정
    public void SetInfo()
    {
        if (instanceInfoText == null) return;

        Text txt = instanceInfoText.GetComponent<Text>();
        if (txt != null)
            txt.text = GetInfo();
    }

    // 카드 정보 숨김 처리
    public void HideInfo()
    {
        if (instanceInfoText != null)
        {
            instanceInfoText.SetActive(false);

            Text txt = instanceInfoText.GetComponent<Text>();
            if (txt != null)
                txt.text = "";
        }

        rightClickEnabled = false;
    }

    // 카드 이름 반환
    public string GetCardName()
    {
        switch (cardId)
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

    // 카드 설명 반환
    public string GetInfo()
    {
        switch (cardId)
        {
            case CardId.Hit: return "안타 - 모든 주자 1루씩 진루";
            case CardId.Double: return "2루타 - 모든 주자 2루씩 진루";
            case CardId.Triple: return "3루타 - 모든 주자 3루씩 진루";
            case CardId.HomeRun: return "홈런 - 타자 포함 전원 득점";
            case CardId.Steal: return "도루 - 모든 주자 1루씩 진루";
            case CardId.Bunt: return "번트 - 1루 주자가 있을시 1루주자 2루로 진루";

            case CardId.GreatCatch: return "호수비 - 상대 안타, 2루타, 3루타 취소 / 아웃 +1";
            case CardId.DoublePlay: return "더블플레이 - 주자 존재 + 상대 안타, 2루타 취소 / 아웃 +2";
            case CardId.TriplePlay: return "삼중살 - 주자 2명 이상 + 상대 안타 취소 / 아웃 +3";
            case CardId.LookingStrikeOut: return "루킹삼진 - 상대 안타, 2루타, 3루타 취소 / 아웃 +1";
            case CardId.SwingStrikeOut: return "헛스윙삼진 - 상대 안타, 2루타, 3루타 취소 / 아웃 +1";

            case CardId.Dazzle: return "눈부심 - 공격카드가 취소되지 않으면 주자/타자 추가 1루 진루";
            case CardId.BadBounce: return "불규칙 바운드 - 수비카드 발동 취소";

            case CardId.PinchHitter: return "대타 - 카드 3장 드로우";
            case CardId.PinchRunner: return "대주자 - 카드 2장 드로우";
            case CardId.PitcherChange: return "투수교체 - 상대 패 2장 제거";
            case CardId.DefensiveSub: return "대수비 - 상대 패 1장 제거";
        }

        return "";
    }
}
