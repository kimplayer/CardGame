using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Dialog")]
    public GameObject dialogPanel;
    public Text messageText;
    public Text hintText;           // "▼ 탭하여 계속" 텍스트 (선택)
    public Text instructionText;    // 액션 단계 안내 텍스트

    [Header("Arrow")]
    public RectTransform arrowRect;

    public System.Func<Transform> targetProvider;

    private Button panelButton;
    private Image panelImage;

    private void Awake()
    {
        panelImage = dialogPanel.GetComponent<Image>();

        panelButton = dialogPanel.GetComponent<Button>();
        if (panelButton == null)
            panelButton = dialogPanel.AddComponent<Button>();

        // 클릭 시 색상 변화 최소화
        ColorBlock cb = panelButton.colors;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.selectedColor = Color.white;
        panelButton.colors = cb;

        SetPanelClickable(false);
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

    private void SetPanelClickable(bool clickable)
    {
        if (panelImage != null)
            panelImage.raycastTarget = clickable;
        if (panelButton != null)
            panelButton.interactable = clickable;
        if (hintText != null)
            hintText.gameObject.SetActive(clickable);
    }

    public void Show(string message, bool clickable)
    {
        dialogPanel.SetActive(true);
        messageText.text = message;
        SetPanelClickable(clickable);
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

    public void ShowInstruction(string text)
    {
        if (instructionText == null) return;
        instructionText.text = text;
        instructionText.gameObject.SetActive(true);
    }

    public void HideInstruction()
    {
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);
    }

    public void Hide()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        HideInstruction();
        HideArrow();
    }

    public void SetNextCallback(System.Action callback)
    {
        panelButton.onClick.RemoveAllListeners();
        panelButton.onClick.AddListener(() => callback?.Invoke());
    }
}
