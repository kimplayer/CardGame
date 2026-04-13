using UnityEngine;
using UnityEngine.EventSystems;

// 카드 오브젝트에 붙이는 롱프레스 정보 표시
public class LongPressInfo : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private const float LONG_PRESS_TIME = 2.0f; // 2초

    private float pressTimer = 0f;
    private bool isPressing = false;
    private bool infoShowing = false;

    public bool IsShowingInfo => infoShowing;

    private Card cardComp;
    private CardDragHandler dragHandler;

    private void Awake()
    {
        cardComp = GetComponent<Card>();
        dragHandler = GetComponent<CardDragHandler>();
    }

    private void Update()
    {
        if (!isPressing) return;

        pressTimer += Time.deltaTime;

        if (pressTimer >= LONG_PRESS_TIME && !infoShowing)
        {
            ShowInfo();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 우클릭 무시
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isPressing = true;
        pressTimer = 0f;
        infoShowing = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;

        // 롱프레스가 아니었으면 정보창 닫기
        if (!infoShowing) return;

        // 2초 이상 눌렀다 떼면 정보창 유지
        // 한번 더 탭하면 닫기
        if (pressTimer < LONG_PRESS_TIME)
            HideInfo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressing = false;
    }

    private void ShowInfo()
    {
        infoShowing = true;
        if (cardComp != null)
            cardComp.InfoTextSetActive(true, false);
    }

    private void HideInfo()
    {
        infoShowing = false;
        if (cardComp != null)
            cardComp.InfoTextSetActive(false, false);
    }

    // 외부에서 정보창 닫기
    public void ForceHideInfo()
    {
        isPressing = false;
        infoShowing = false;
        pressTimer = 0f;
        if (cardComp != null)
            cardComp.InfoTextSetActive(false, false);
    }
}