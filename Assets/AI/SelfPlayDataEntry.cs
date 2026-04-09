using System.Collections.Generic;

// 학습 데이터 1개 단위
[System.Serializable]
public class SelfPlayDataEntry
{
    public float[] stateVector;   // 게임 상태 입력 벡터 (60차원)
    public int actionIndex;   // 선택한 행동 인덱스
    public float outcome;       // 최종 결과 (1 = 승, 0 = 무, -1 = 패)
}

// 전체 데이터 묶음
[System.Serializable]
public class SelfPlayDataSet
{
    public List<SelfPlayDataEntry> entries = new List<SelfPlayDataEntry>();
}