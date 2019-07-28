[System.Serializable]
public struct SGame
{
    public string id;
    public string player1ID;
    public string player2ID;
    public int[] board;
    public int winner;
    public int currentTurn;

    public SGame(string _id, string _player1ID)
    {
        id = _id;
        player1ID = _player1ID;
        player2ID = "";
        board = new int[] 
            { 0, 0, 0,
              0, 0, 0,
              0, 0, 0 };
        winner = 0;
        currentTurn = 0;
    }

    public SGame(string _id, string _player1ID, string _player2ID, int[] _board, int _currentTurn)
    {
        id = _id;
        player1ID = _player1ID;
        player2ID = _player2ID;
        board = _board;
        winner = 0;
        currentTurn = _currentTurn;
    }
}
