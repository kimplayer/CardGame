using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Dialog")]
    public GameObject dialogPanel;
    public Text messageText;
    public Button nextButton;

    [Header("Arrow")]
    public RectTransform arrowRect;

    public System.Func<Transform> targetProvider;

    private void Awake()
    {
        // 배경 패널이 게임 UI 클릭을 막지 않도록
        if (dialogPanel != null)
        {
            Image panelImage = dialogPanel.GetComponent<Image>();
            if (panelImage != null)
                panelImage.raycastTarget = false;
        }
        Hide();
    }

    private void Update()
    {
        if (arrowRect == null || !arrowRect.gameObject.activeSelf) return;
        if (targetProvider == null) return;

        Transform target = targetProvider();
        if (target == null)
        {
            arrowRect.gameObject.SetActive(false);
            return;
        }

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
        arrowRect.position = new Vector3(screenPos.x, screenPos.y + 90f, 0f);
    }

    public void Show(string message, bool showNext)
    {
        dialogPanel.SetActive(true);
        messageText.text = message;
        nextButton.gameObject.SetActive(showNext);
    }

    public void ShowArrow(System.Func<Transform> provider)
    {
        targetProvider = provider;
        if (arrowRect != null)
            arrowRect.gameObject.SetActive(true);
    }

    public void HideArrow()
    {
        if (arrowRect != null)
            arrowRect.gameObject.SetActive(false);
        targetProvider = null;
    }

    public void Hide()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        HideArrow();
    }

    public void SetNextCallback(System.Action callback)
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => callback?.Invoke());
    }
}
