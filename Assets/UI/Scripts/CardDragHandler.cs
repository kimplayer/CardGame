using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Vector3 originalPosition;
    private Transform originalParent;

    public static CardDragHandler draggingCard;

    public CardId cardId;
    public CardCategory category;
    public string cardKey;

    // 드랍 성공 여부 플래그
    private bool droppedSuccessfully = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        LongPressInfo longPress = GetComponent<LongPressInfo>();
        if (longPress != null && longPress.IsShowingInfo) return;

        draggingCard = this;
        droppedSuccessfully = false; // 드래그 시작 시 초기화
        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        // Canvas 최상단으로 이동
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingCard != this) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingCard == this)
            draggingCard = null;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // 드랍 성공했으면 복귀 안 함 → 그냥 숨기기
        if (droppedSuccessfully)
        {
            gameObject.SetActive(false);
            return;
        }

        // 드랍존 못 찾았으면 원래 위치로 복귀
        transform.SetParent(originalParent, true);
        rectTransform.position = originalPosition;
    }

    // 드랍존에서 성공적으로 드랍됐을 때 호출
    public void MarkAsDropped()
    {
        droppedSuccessfully = true;
    }

    public void ReturnToOriginal()
    {
        droppedSuccessfully = false;
        transform.SetParent(originalParent, true);
        rectTransform.position = originalPosition;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}