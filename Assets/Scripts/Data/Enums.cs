public enum GameStageEnum : byte
{
    WaitingForPlayers = 1, SelectOwnerOfLoyal = 2, SelectTolba = 3, DoublingCards = 4, DropCards = 5,
    TakeTrick = 6, EndOfTolba = 7, EndOfGame = 8
}

public enum TolbaEnum : byte
{
    Lotoch = 1, OldKoba = 2, Girls = 4, Denary = 8, TRS = 16
}

public enum CardSuit : byte
{
    Koba = 1, Bastony = 2, Denary = 3, Sanak = 4
}

public enum CardRank : byte
{
    Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
    Boy = 11, Girl = 12, Old = 13, Ace = 14
}