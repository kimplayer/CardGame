using UnityEngine;
using UnityEngine.EventSystems;

// 카드 오브젝트에 붙이는 롱프레스 정보 표시
public class LongPressInfo : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private const float LONG_PRESS_TIME = 2.0f;

    // 이 거리(px) 이상 움직이면 드래그 의도로 판단 → 롱프레스 취소
    private const float DRAG_CANCEL_THRESHOLD = 25f;

    private float pressTimer = 0f;
    private bool isPressing = false;
    private bool infoShowing = false;
    private Vector2 pressStartPosition;

    public bool IsShowingInfo => infoShowing;
    public bool IsPressing => isPressing;

    private Card cardComp;

    private void Awake()
    {
        cardComp = GetComponent<Card>();
    }

    private void Update()
    {
        if (!isPressing) return;

        // 손가락/마우스가 임계값 이상 이동하면 드래그로 판단 → 롱프레스 취소
        if (Vector2.Distance(GetCurrentPointerPosition(), pressStartPosition) > DRAG_CANCEL_THRESHOLD)
        {
            isPressing = false;
            pressTimer = 0f;
            return;
        }

        pressTimer += Time.deltaTime;

        if (pressTimer >= LONG_PRESS_TIME && !infoShowing)
            ShowInfo();
    }

    // 현재 포인터(터치 우선, 없으면 마우스) 위치 반환
    private Vector2 GetCurrentPointerPosition()
    {
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;
        return Input.mousePosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // 이미 정보창이 열려 있으면 탭으로 닫기
        if (infoShowing)
        {
            HideInfo();
            return;
        }

        isPressing = true;
        pressTimer = 0f;
        pressStartPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;

        if (!infoShowing) return;

        // 2초 이상 눌렀다 떼면 정보창 유지 (한 번 더 탭해서 닫음)
        // 2초 미만이었으면 손가락 떼는 시점에 닫기
        if (pressTimer < LONG_PRESS_TIME)
            HideInfo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 모바일에서는 손가락이 약간 움직이면 OnPointerExit가 발생하므로
        // 여기서 취소하지 않음. 이동 거리 임계값(Update)으로 처리.
    }

    // 드래그 시작 등 외부에서 롱프레스 강제 취소
    public void CancelPress()
    {
        isPressing = false;
        pressTimer = 0f;
    }

    private void ShowInfo()
    {
        infoShowing = true;
        isPressing = false; // 표시 완료 후 pressing 상태 종료
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
