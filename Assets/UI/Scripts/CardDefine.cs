// 카드 고유 ID
public enum CardId
{
    Hit = 0,             // 안타
    Double = 1,          // 2루타
    Triple = 2,          // 3루타
    HomeRun = 3,         // 홈런
    Steal = 4,           // 도루
    Bunt = 5,            // 번트

    GreatCatch = 6,      // 호수비
    DoublePlay = 7,      // 더블플레이
    TriplePlay = 8,      // 삼중살
    LookingStrikeOut = 9,// 루킹삼진
    SwingStrikeOut = 10, // 헛스윙삼진

    Dazzle = 11,         // 눈부심
    BadBounce = 12,      // 불규칙 바운드

    PinchHitter = 13,    // 대타
    PinchRunner = 14,    // 대주자
    PitcherChange = 15,  // 투수교체
    DefensiveSub = 16    // 대수비
}

// 카드 분류
public enum CardCategory
{
    Attack,
    Defense,
    Trap,
    Draw
}