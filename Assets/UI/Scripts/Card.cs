using System.Collections;
using System.Collections.Generic;
using UnityEditor.VisionOS;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int cardType;
    public GameObject infoText;
    public bool rightClickEnabeld = true;
    private GameObject instanceInfoText;


    void Awake()
    {
        if (infoText != null)
        {
            instanceInfoText = Instantiate(infoText, transform);
            instanceInfoText.transform.localPosition = new Vector2(0, 100);
            instanceInfoText.SetActive(false);
        }
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(1) && rightClickEnabeld && instanceInfoText != null)
        {
            instanceInfoText.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData event_Data)
    {
        if (event_Data.button == PointerEventData.InputButton.Right && rightClickEnabeld && instanceInfoText != null)
        {
            instanceInfoText.SetActive(true);
        }
    }

    public void InfoTextSetActive(bool active, bool right_ClickEnabled)
    {
        if (instanceInfoText != null)
        {
            instanceInfoText.SetActive(active);
        }

        rightClickEnabeld = right_ClickEnabled;
    }

    public void SetInfo()
    {
        if (instanceInfoText != null) return;

        Text txt = instanceInfoText.GetComponent<Text>();
        if(txt != null)
        {
            txt.text = GetInfo();
        }
    }

    public string GetInfo()
    {
        switch (cardType)
        {
            case 0:
                return "안타 - 모든 주자 1루씩 진루";
            case 1:
                return "2루타 - 모든 주자 2루씩 진루";
            case 2:
                return "3루타 - 모든 주자 3루씩 진루";
            case 3:
                return "홈런 - 타자 포함 전원 득점";
            case 4:
                return "도루 - 모든 주자 1루씩 진루";
            case 5:
                return "번트 - 1루에 주자가 있을시 1루주자 2루로 진루";
            case 6:
                return "호수비 - 상대가 안타, 2루타, 3루타 카드를 낼때 발동되며 상대방 카드 발동 취소되고 아웃카운트 1개 증가";
            case 7:
                return "더블플레이 - 주자가 1루 2루 3루 중에 한명이라도 있고 상대가 안타, 2루타 카드를 낼때 발동되며 상대방 카드 발동 취소되고 아웃카운트 2개 증가";
            case 8:
                return "삼중살 - 주자가 1루 2루 3루 중에 2명 있고 상대가 안타 카드를 낼때 발동되며 상대방 카드 발동 취소되고 아웃카운트 2개 증가";
            case 9:
                return "루킹삼진 - 상대가 안타, 2루타, 3루타 카드를 낼때 발동되며 상대방 카드 발동 취소되고 아웃카운트 1개 증가";
            case 10:
                return "헛스윙삼진 - 상대가 안타, 2루타, 3루타 카드를 낼때 발동되며 상대방 카드 발동 취소되고 아웃카운트 1개 증가";
            case 11:
                return "눈부심 - 공격카드가 취소가 안되었을때 타자, 주자 추가로 1루씩 진루";
            case 12:
                return "불규칙 바운드 - 수비카드 발동취소";
            case 13:
                return "대타 - 카드 3장 드로우";
            case 14:
                return "대주자 - 카드 2장 드로우";
            case 15:
                return "투수교체 - 상대 패 2장 제거";
            case 16:
                return "대수비 - 상대 패 1장 제거";
        }

        return "";
    }
}
