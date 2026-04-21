using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 베이스 다이아몬드 UI 관리
// 기존 firstBaseImage, secondBaseImage, thirdBaseImage 대체
public class BaseDiamond : MonoBehaviour
{
    [Header("베이스 이미지")]
    public Image firstBaseImage;
    public Image secondBaseImage;
    public Image thirdBaseImage;
    public Image homePlateImage;

    [Header("주자 아이콘")]
    public GameObject firstRunnerIcon;
    public GameObject secondRunnerIcon;
    public GameObject thirdRunnerIcon;

    [Header("베이스 스프라이트")]
    public Sprite baseEmptySprite;
    public Sprite baseOccupiedSprite;

    [Header("애니메이션 설정")]
    public float pulseSpeed = 2.0f;  // 깜빡임 속도
    public float pulseMin = 0.6f;  // 최소 밝기
    public float pulseMax = 1.0f;  // 최대 밝기

    private bool firstBase;
    private bool secondBase;
    private bool thirdBase;

    private float pulseTimer = 0f;

    private void Update()
    {
        // 주자 있는 베이스 펄스 애니메이션
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = Mathf.Lerp(pulseMin, pulseMax,
                                 (Mathf.Sin(pulseTimer) + 1f) / 2f);

        if (firstBase && firstBaseImage != null)
            firstBaseImage.color = new Color(pulse, pulse * 0.8f, 0f, 1f);

        if (secondBase && secondBaseImage != null)
            secondBaseImage.color = new Color(pulse, pulse * 0.8f, 0f, 1f);

        if (thirdBase && thirdBaseImage != null)
            thirdBaseImage.color = new Color(pulse, pulse * 0.8f, 0f, 1f);
    }

    // 베이스 상태 갱신
    public void UpdateBases(bool first, bool second, bool third)
    {
        firstBase = first;
        secondBase = second;
        thirdBase = third;

        RefreshBaseUI();
    }

    private void RefreshBaseUI()
    {
        SetBase(firstBaseImage, firstRunnerIcon, firstBase);
        SetBase(secondBaseImage, secondRunnerIcon, secondBase);
        SetBase(thirdBaseImage, thirdRunnerIcon, thirdBase);
    }

    private void SetBase(Image baseImage, GameObject runnerIcon, bool occupied)
    {
        if (baseImage != null)
        {
            // 스프라이트 교체
            baseImage.sprite = occupied ? baseOccupiedSprite : baseEmptySprite;

            // 빈 베이스는 회색, 주자 있는 베이스는 노란색으로
            if (!occupied)
                baseImage.color = new Color(0.5f, 0.5f, 0.6f, 1f);
        }

        // 주자 아이콘 표시/숨김
        if (runnerIcon != null)
            runnerIcon.SetActive(occupied);
    }

    // 베이스 전체 초기화
    public void ResetBases()
    {
        UpdateBases(false, false, false);
    }
}