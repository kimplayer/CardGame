using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    // 덱빌드 씬으로 이동 (커스텀 덱 구성 후 SingleLane으로 넘어감)
    public void GoToDeckBuild()
    {
        SceneManager.LoadScene("DeckBuild");
    }

    // 기본 덱으로 바로 게임 시작
    public void GoToSingleLaneDefault()
    {
        if (DeckData.Instance != null)
            DeckData.Instance.useCustomDeck = false;
        SceneManager.LoadScene("SingleLane");
    }

    // 튜토리얼 씬으로 이동
    public void GoToTutorial()
    {
        if (DeckData.Instance != null)
            DeckData.Instance.useCustomDeck = false;
        SceneManager.LoadScene("Tutorial");
    }
}
