[System.Serializable]
public struct SGame
{
    public string id;
    public SPlayer player1;
    public SPlayer player2;
    public int[] board;
    public int winner;
    public int currentTurn;

    public SGame(string _id, SPlayer _player1)
    {
        id = _id;
        player1 = _player1;
        player2 = new SPlayer("");
        board = new int[] 
            { 0, 0, 0,
              0, 0, 0,
              0, 0, 0 };
        winner = 0;
        currentTurn = 0;
    }

    public SGame(string _id, SPlayer _player1, SPlayer _player2, int[] _board, int _currentTurn)
    {
        id = _id;
        player1 = _player1;
        player2 = _player2;
        board = _board;
        winner = 0;
        currentTurn = _currentTurn;
    }
}
