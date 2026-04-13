using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SingleLaneGame gameManager;

    public Image zoneImage;
    public Color normalColor = new Color(1, 1, 1, 0.1f);
    public Color highlightColor = new Color(0, 1, 0, 0.3f);

    private void Start()
    {
        if (zoneImage != null)
        {
            zoneImage.color = normalColor;
            // 처음엔 레이캐스트 끔 → 다른 UI 클릭 가능
            zoneImage.raycastTarget = false;
        }
    }

    private void Update()
    {
        // 드래그 중일 때만 레이캐스트 활성화
        bool isDragging = CardDragHandler.draggingCard != null;

        if (zoneImage != null)
            zoneImage.raycastTarget = isDragging;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (CardDragHandler.draggingCard == null) return;

        CardDragHandler card = CardDragHandler.draggingCard;

        // 게임매니저에서 먼저 유효성 체크
        bool success = gameManager.OnCardDropped(card.cardKey, card.category);

        // 성공했을 때만 드랍 성공 표시
        if (success)
            card.MarkAsDropped();
        else
            card.ReturnToOriginal(); // 실패 시 원래 위치로 복귀

        if (zoneImage != null)
            zoneImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardDragHandler.draggingCard == null) return;

        if (zoneImage != null)
            zoneImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (zoneImage != null)
            zoneImage.color = normalColor;
    }
}