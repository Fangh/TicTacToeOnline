using System;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public static event Action<SGame> OnBoardUpdated;
    public static event Action OnNewGame;

    [Header("References")]
    [SerializeField] private GameObject pawnPrefab;
    [SerializeField] private Square[] squares;
    [SerializeField] private GameObject playerList;

    internal SGame currentGame;
    internal int currentTeam;       

    private void Awake()
    {
        Instance = this;
    }

    public SGame InitializeNewGame(string uniqueID, string player1ID)
    {
        currentGame = new SGame(uniqueID, player1ID);
        CleanBoard();
        OnNewGame?.Invoke();
        NextTurn();

        OnBoardUpdated?.Invoke(currentGame);
        return currentGame;
    }

    public void InitializeGameWithData(string id, string player1ID, string player2ID, int[] board, int currentTurn)
    {
        CleanBoard();
        OnNewGame?.Invoke();

        currentGame = new SGame(id, player1ID, player2ID, board, currentTurn);
        if (currentGame.currentTurn == 0)
            NextTurn();

        UpdateBoard(currentGame);

    }

    /// <summary>
    /// Update the displayed board with some data
    /// </summary>
    /// <param name="data">The data to update the displayed board with</param>
    public void UpdateBoard(SGame data)
    {
        if (currentGame.board != data.board)
            currentGame.board = data.board;

        if (currentGame.winner != data.winner)
            currentGame.winner = data.winner;

        if (currentGame.currentTurn != data.currentTurn)
            currentGame.currentTurn = data.currentTurn;

        if (currentGame.player1ID != data.player1ID)
        {
            currentGame.player1ID = data.player1ID;
            OnBoardUpdated?.Invoke(currentGame);
        }
        if (currentGame.player2ID != data.player2ID)
        {
            currentGame.player2ID = data.player2ID;
        }

        for (int i = 0; i < 9; i++)
        {
            if (data.board[i] != 0 && squares[i].pawnTeam == 0)
            {
                DisplayPawn(i);
            }
        }
        CheckWinner();
        OnBoardUpdated?.Invoke(currentGame);
    }

    /// <summary>
    /// Change the board data. If you try to place a pawn of a team which is not the turn, does nothing.
    /// Then it checks if there is a winner. If not, it goes to next turn.
    /// </summary>
    /// <param name="_index">Which square of the board is changed</param>
    /// <param name="_team">Which team does this square belongs now</param>
    public void PlacePawn(int _index, int _team)
    {
        //Don't place pawn if there is already a winner
        if (currentGame.winner != 0)
            return;

        if (currentGame.board[_index] == 0)
        {
            currentGame.board[_index] = (int)_team;
            CheckWinner();
            if (currentGame.winner == 0)
                NextTurn();
        }
        else
        {
            Debug.LogWarning("There is already a pawn here");
        }
    }

    /// <summary>
    /// Instantiate a pawn at a specific position on the board. Automatically checks the data to know which to spawn
    /// </summary>
    /// <param name="_index">where to put the pawn</param>
    public void DisplayPawn(int _index)
    {
        squares[_index].InstantiatePawn(pawnPrefab, currentGame.board[_index]);
    }

    private void CleanBoard()
    {
        if (currentGame.board == null || currentGame.board.Length == 0)
            return;

        for (int i = 0; i < currentGame.board.Length; i++)
        {
            currentGame.board[i] = 0;
            squares[i].DeletePawn();
        }
    }

    private void NextTurn()
    {
        if (currentGame.currentTurn == 1)
            currentGame.currentTurn = 2;
        else
            currentGame.currentTurn = 1;
    }

    private void CheckWinner()
    {
        if (currentGame.currentTurn == 0)
            return;

        if ((currentGame.board[0] == currentGame.board[1] && currentGame.board[0] == currentGame.board[2] && currentGame.board[0] != 0)
            || (currentGame.board[0] == currentGame.board[4] && currentGame.board[0] == currentGame.board[8] && currentGame.board[0] != 0)
            || (currentGame.board[0] == currentGame.board[3] && currentGame.board[0] == currentGame.board[6] && currentGame.board[0] != 0)
            || (currentGame.board[1] == currentGame.board[4] && currentGame.board[1] == currentGame.board[7] && currentGame.board[1] != 0)
            || (currentGame.board[2] == currentGame.board[4] && currentGame.board[2] == currentGame.board[6] && currentGame.board[2] != 0)
            || (currentGame.board[2] == currentGame.board[6] && currentGame.board[2] == currentGame.board[8] && currentGame.board[2] != 0)
            || (currentGame.board[3] == currentGame.board[4] && currentGame.board[3] == currentGame.board[5] && currentGame.board[3] != 0)
            || (currentGame.board[6] == currentGame.board[7] && currentGame.board[6] == currentGame.board[8] && currentGame.board[6] != 0)
            )
        {
            Debug.Log($"3 pawns are in a row ! It is player {currentGame.currentTurn} turn so it is their pawns");
            currentGame.winner = currentGame.currentTurn;
            currentGame.currentTurn = 0;
        }
    }
}