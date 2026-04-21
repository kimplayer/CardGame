using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutCountUI : MonoBehaviour
{
    [Header("아웃 원형 이미지")]
    public Image out1Image;
    public Image out2Image;
    public Image out3Image;

    [Header("스프라이트")]
    public Sprite outEmptySprite;
    public Sprite outFilledSprite;

    [Header("OUT 텍스트")]
    public Text outCountText;

    [Header("애니메이션")]
    public float flashDuration = 0.3f; // 아웃 발생 시 깜빡임 시간

    private int currentOutCount = 0;
    private bool isFlashing = false;

    // 아웃카운트 갱신
    public void UpdateOutCount(int count)
    {
        int prevCount = currentOutCount;
        currentOutCount = count;

        // 아웃 증가 시 애니메이션
        if (count > prevCount)
            StartCoroutine(FlashNewOut(count));
        else
            RefreshUI();
    }

    // 아웃카운트 초기화
    public void ResetOutCount()
    {
        currentOutCount = 0;
        RefreshUI();
    }

    // UI 갱신
    private void RefreshUI()
    {
        SetOutImage(out1Image, currentOutCount >= 1);
        SetOutImage(out2Image, currentOutCount >= 2);
        SetOutImage(out3Image, currentOutCount >= 3);

        if (outCountText != null)
            outCountText.text = currentOutCount + " OUT";
    }

    // 개별 이미지 설정
    private void SetOutImage(Image img, bool filled)
    {
        if (img == null) return;
        img.sprite = filled ? outFilledSprite : outEmptySprite;
        img.color = Color.white;
    }

    // 새 아웃 발생 시 깜빡임 애니메이션
    private System.Collections.IEnumerator FlashNewOut(int newCount)
    {
        if (isFlashing) yield break;
        isFlashing = true;

        // 새로 아웃된 이미지 찾기
        Image newOutImage = newCount == 1 ? out1Image
                          : newCount == 2 ? out2Image
                          : out3Image;

        RefreshUI();

        // 3번 깜빡임
        for (int i = 0; i < 3; i++)
        {
            if (newOutImage != null)
                newOutImage.color = new Color(1f, 1f, 1f, 0.2f);

            yield return new WaitForSeconds(flashDuration * 0.4f);

            if (newOutImage != null)
                newOutImage.color = Color.white;

            yield return new WaitForSeconds(flashDuration * 0.3f);
        }

        isFlashing = false;
    }
}
