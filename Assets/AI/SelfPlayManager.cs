using System.Collections;
using System.IO;
using UnityEngine;

public class SelfPlayManager : MonoBehaviour
{
    [Header("Self Play 설정")]
    public int totalGames = 500;   // 총 게임 수
    public int mctsIter = 50;    // MCTS 반복 횟수
    public int samplingCount = 20;    // IS 샘플링 횟수
    public int batchSize = 50;    // 배치 저장 단위

    [Header("저장 경로")]
    public string savePath = "SelfPlayData";

    private int gameCount = 0;
    private int p1Wins = 0;
    private int p2Wins = 0;
    private int draws = 0;
    private int fileIndex = 0;

    private SelfPlayDataSet currentBatch = new SelfPlayDataSet();

    private void Start()
    {
        StartCoroutine(RunSelfPlay());
    }

    private IEnumerator RunSelfPlay()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, savePath);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        Debug.Log($"Self Play 시작 - 총 {totalGames}판");
        Debug.Log($"저장 경로 : {fullPath}");

        MCTSAgent agent1 = new MCTSAgent(mctsIter, samplingCount);
        MCTSAgent agent2 = new MCTSAgent(mctsIter, samplingCount);

        while (gameCount < totalGames)
        {
            SelfPlayGame game = new SelfPlayGame(agent1, agent2);
            int result = game.Run();

            // 결과 집계
            if (result == 1) p1Wins++;
            else if (result == -1) p2Wins++;
            else draws++;

            // 데이터 배치에 추가
            currentBatch.entries.AddRange(game.collectedData);
            gameCount++;

            // 배치 저장
            if (gameCount % batchSize == 0)
            {
                SaveBatch(fullPath);
                Debug.Log($"진행 : {gameCount}/{totalGames} | " +
                          $"P1 {p1Wins}승 P2 {p2Wins}승 무 {draws}");
            }

            // 프레임 분산 (Unity 프리징 방지)
            if (gameCount % 5 == 0)
                yield return null;
        }

        // 마지막 배치 저장
        if (currentBatch.entries.Count > 0)
            SaveBatch(fullPath);

        Debug.Log($"Self Play 완료! 총 {gameCount}판");
        Debug.Log($"P1 {p1Wins}승 / P2 {p2Wins}승 / 무 {draws}");
        Debug.Log($"데이터 저장 위치 : {fullPath}");
    }

    // JSON으로 배치 저장
    private void SaveBatch(string path)
    {
        string json = JsonUtility.ToJson(currentBatch, true);
        string fileName = $"selfplay_{fileIndex:D4}.json";
        string filePath = Path.Combine(path, fileName);

        File.WriteAllText(filePath, json);
        Debug.Log($"배치 저장 완료 : {fileName} ({currentBatch.entries.Count}개)");

        currentBatch = new SelfPlayDataSet();
        fileIndex++;
    }
}