using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    [Header("스코어보드 UI")]
    public Transform myScoreRow;
    public Transform enemyScoreRow;
    public Transform inningHeaderRow;
    public GameObject cellPrefab;

    [Header("이닝 정보 텍스트")]
    public Text currentInningText;
    public Text currentSideText;
    public Text currentTurnText;

    [Header("팀 라벨")]
    public Text firstTeamLabel;
    public Text secondTeamLabel;

    private const int MAX_INNING = 12;

    private int[] myInningScores = new int[MAX_INNING + 1];
    private int[] enemyInningScores = new int[MAX_INNING + 1];
    private bool[] inningPlayed = new bool[MAX_INNING + 1];

    private List<ScoreBoardCell> myScoreCells = new List<ScoreBoardCell>();
    private List<ScoreBoardCell> enemyScoreCells = new List<ScoreBoardCell>();
    private List<ScoreBoardCell> headerCells = new List<ScoreBoardCell>();

    private int currentInning = 1;
    private bool currentIsTop = true;
    private bool currentIsPlayer = true;
    private bool playerIsFirst = true;

    // 스코어보드 초기화 - 동전 결과 확정 후 호출
    public void InitScoreBoard(bool isPlayerFirst)
    {
        playerIsFirst = isPlayerFirst;

        // 선공이 위, 후공이 아래로 행 순서 설정
        if (playerIsFirst)
        {
            myScoreRow.SetSiblingIndex(1);
            enemyScoreRow.SetSiblingIndex(2);
        }
        else
        {
            enemyScoreRow.SetSiblingIndex(1);
            myScoreRow.SetSiblingIndex(2);
        }

        // 라벨 텍스트 갱신
        if (playerIsFirst)
        {
            if (firstTeamLabel != null) firstTeamLabel.text = "나 (선공)";
            if (secondTeamLabel != null) secondTeamLabel.text = "상대 (후공)";
        }
        else
        {
            if (firstTeamLabel != null) firstTeamLabel.text = "상대 (선공)";
            if (secondTeamLabel != null) secondTeamLabel.text = "나 (후공)";
        }

        // 기존 셀 삭제
        ClearCells(myScoreRow);
        ClearCells(enemyScoreRow);
        ClearCells(inningHeaderRow);

        myScoreCells.Clear();
        enemyScoreCells.Clear();
        headerCells.Clear();

        // 점수 초기화
        for (int i = 0; i <= MAX_INNING; i++)
        {
            myInningScores[i] = 0;
            enemyInningScores[i] = 0;
            inningPlayed[i] = false;
        }

        // 헤더 행 생성
        CreateHeaderCell(""); // 팀명 공간
        for (int i = 1; i <= MAX_INNING; i++)
            CreateHeaderCell(i.ToString());
        CreateHeaderCell("R"); // 총점

        // 내 점수 행 생성
        CreateScoreCell(myScoreRow, myScoreCells, "나");
        for (int i = 1; i <= MAX_INNING; i++)
            CreateScoreCell(myScoreRow, myScoreCells, "-");
        CreateScoreCell(myScoreRow, myScoreCells, "0");

        // 상대 점수 행 생성
        CreateScoreCell(enemyScoreRow, enemyScoreCells, "상대");
        for (int i = 1; i <= MAX_INNING; i++)
            CreateScoreCell(enemyScoreRow, enemyScoreCells, "-");
        CreateScoreCell(enemyScoreRow, enemyScoreCells, "0");

        RefreshUI();

        Debug.Log($"ScoreBoard 초기화 완료 - 플레이어 선공 : {playerIsFirst}");
    }

    // 반이닝 시작 시 호출
    public void OnHalfInningStart(int inning, bool isTop, bool isPlayerBatting)
    {
        currentInning = inning;
        currentIsTop = isTop;
        currentIsPlayer = isPlayerBatting;

        RefreshUI();
    }

    // 점수 변경 시 호출
    public void OnScoreChanged(int inning, bool isTop,
                                int myScore, int enemyScore,
                                int myTotal, int enemyTotal)
    {
        // 현재 공격 중인 팀의 이닝 점수 업데이트
        if (currentIsPlayer)
            myInningScores[inning] = myScore;
        else
            enemyInningScores[inning] = enemyScore;

        inningPlayed[inning] = true;

        // 총점 셀 갱신
        int lastIdx = myScoreCells.Count - 1;
        if (lastIdx >= 0)
        {
            myScoreCells[lastIdx].SetText(myTotal.ToString());
            enemyScoreCells[lastIdx].SetText(enemyTotal.ToString());
        }

        RefreshUI();
    }

    // 반이닝 종료 시 호출
    public void OnHalfInningEnd(int inning, bool isTop,
                                 int myTotal, int enemyTotal)
    {
        inningPlayed[inning] = true;

        // 총점 셀 갱신
        int lastIdx = myScoreCells.Count - 1;
        if (lastIdx >= 0)
        {
            myScoreCells[lastIdx].SetText(myTotal.ToString());
            enemyScoreCells[lastIdx].SetText(enemyTotal.ToString());
        }

        RefreshUI();
    }

    // UI 전체 갱신
    private void RefreshUI()
    {
        // 이닝 정보 텍스트 갱신
        if (currentInningText != null)
            currentInningText.text = currentInning + "회";

        if (currentSideText != null)
            currentSideText.text = currentIsTop ? "초" : "말";

        if (currentTurnText != null)
            currentTurnText.text = currentIsPlayer ? "내 공격" : "상대 공격";

        // 셀 갱신 (인덱스 0 = 팀명, 1~12 = 이닝, 13 = 총점)
        for (int i = 1; i <= MAX_INNING; i++)
        {
            // 내 점수 행 셀
            if (i < myScoreCells.Count)
            {
                ScoreBoardCell myCell = myScoreCells[i];

                if (i == currentInning)
                {
                    myCell.SetCurrent();
                    myCell.SetText(myInningScores[i] > 0
                                   ? myInningScores[i].ToString()
                                   : "-");
                }
                else if (inningPlayed[i])
                {
                    myCell.SetMyScore();
                    myCell.SetText(myInningScores[i].ToString());
                }
                else
                {
                    myCell.SetDefault();
                    myCell.SetText("-");
                }
            }

            // 상대 점수 행 셀
            if (i < enemyScoreCells.Count)
            {
                ScoreBoardCell enemyCell = enemyScoreCells[i];

                if (i == currentInning)
                {
                    enemyCell.SetCurrent();
                    enemyCell.SetText(enemyInningScores[i] > 0
                                      ? enemyInningScores[i].ToString()
                                      : "-");
                }
                else if (inningPlayed[i])
                {
                    enemyCell.SetEnemyScore();
                    enemyCell.SetText(enemyInningScores[i].ToString());
                }
                else
                {
                    enemyCell.SetDefault();
                    enemyCell.SetText("-");
                }
            }
        }

        // 총점 셀 색상
        int lastIndex = myScoreCells.Count - 1;
        if (lastIndex >= 0)
        {
            myScoreCells[lastIndex].SetTotal();
            enemyScoreCells[lastIndex].SetTotal();
        }
    }

    // 헤더 셀 생성
    private void CreateHeaderCell(string text)
    {
        if (cellPrefab == null || inningHeaderRow == null) return;

        GameObject obj = Instantiate(cellPrefab, inningHeaderRow);
        ScoreBoardCell cell = obj.GetComponent<ScoreBoardCell>();

        if (cell == null)
            cell = obj.AddComponent<ScoreBoardCell>();

        cell.SetText(text);
        cell.SetDefault();
        headerCells.Add(cell);
    }

    // 점수 셀 생성
    private void CreateScoreCell(Transform parent,
                                  List<ScoreBoardCell> cellList,
                                  string text)
    {
        if (cellPrefab == null || parent == null) return;

        GameObject obj = Instantiate(cellPrefab, parent);
        ScoreBoardCell cell = obj.GetComponent<ScoreBoardCell>();

        if (cell == null)
            cell = obj.AddComponent<ScoreBoardCell>();

        cell.SetText(text);
        cell.SetDefault();
        cellList.Add(cell);
    }

    // 셀 전체 삭제
    private void ClearCells(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
}