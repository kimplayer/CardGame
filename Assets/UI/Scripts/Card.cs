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
    public bool rightClickEnabeld;
    private GameObject instanceInfoText;


    void Awake()
    {
        rightClickEnabeld = true;
        instanceInfoText = Instantiate(infoText, transform);
        Vector2 position = new Vector2(0, 100);
        instanceInfoText.transform.localPosition = position;
        instanceInfoText.SetActive(false);
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(1) && rightClickEnabeld)
        {
            Debug.Log("Down");
            instanceInfoText.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData event_Data)
    {
        if (event_Data.button == PointerEventData.InputButton.Right && rightClickEnabeld)
        {
            Debug.Log("Click");
            instanceInfoText.SetActive(true);
        }
    }

    public void InfoTextSetActive(bool active, bool right_ClickEnabled)
    {
        instanceInfoText.SetActive(active);
        rightClickEnabeld = right_ClickEnabled;
    }

    public void SetInfo()
    {
        instanceInfoText.GetComponent<Text>().text = GetInfo();
    }

    public string GetInfo()
    {
        switch (cardType)
        {
            case 0:
                return "적의 공격을 방어합니다.";
            case 1:
                return "적에게 1의 대미지를 줍니다.";
            case 2:
                return "적에게 2의 대미지를 줍니다.";
            case 3:
                return "적에게 3의 대미지를 줍니다.";
            case 4:
                return "적에게 4의 대미지를 줍니다.";
        }

        return "";
    }
}
