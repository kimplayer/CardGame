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
            Debug.Log("Down");
            instanceInfoText.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData event_Data)
    {
        if (event_Data.button == PointerEventData.InputButton.Right && rightClickEnabeld && instanceInfoText != null)
        {
            Debug.Log("Click");
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
                return "안타";
            case 1:
                return "2루타";
            case 2:
                return "3루타";
            case 3:
                return "홈런";
            case 4:
                return "도루";
            case 5:
                return "번트";
            case 6:
                return "호수비";
            case 7:
                return "더블플레이";
            case 8:
                return "삼중살";
            case 9:
                return "루킹삼진";
            case 10:
                return "헛스윙삼진";
            case 11:
                return "눈부심";
            case 12:
                return "불규칙 바운드";
            case 13:
                return "대타";
            case 14:
                return "대주자";
            case 15:
                return "대수비";
        }

        return "";
    }
}
