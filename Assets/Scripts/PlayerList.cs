using TMPro;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI player1Label;
    [SerializeField] private TextMeshProUGUI player2Label;
    [SerializeField] private TMP_EmojiTextUGUI player1WinIcon;
    [SerializeField] private TMP_EmojiTextUGUI player2WinIcon;
    [SerializeField] private TMP_EmojiTextUGUI player1TurnIcon;
    [SerializeField] private TMP_EmojiTextUGUI player2TurnIcon;

    private void OnEnable()
    {
        Initialize();
        BoardManager.OnBoardUpdated += UpdatePlayerList;
        BoardManager.OnNewGame += Initialize;
    }

    private void Initialize()
    {
        player1TurnIcon.enabled = false;
        player2TurnIcon.enabled = false;

        player1WinIcon.enabled = false;
        player2WinIcon.enabled = false;

        player1Label.text = "Waiting Player 1";
        player2Label.text = "Waiting Player 2";

        player1Label.color = Color.white;
        player2Label.color = Color.white;

    }

    private void OnDisable()
    {
        BoardManager.OnBoardUpdated -= UpdatePlayerList;
        BoardManager.OnNewGame -= Initialize;
    }

    /// <summary>
    /// Check everything displayed to update according to some data
    /// </summary>
    /// <param name="data">the game data to look into to display the correct things</param>
    private void UpdatePlayerList(SGame data)
    {
        DisplayPlayerName(1, data.player1.id);
        DisplayPlayerName(2, data.player2.id);
        ColorLocalPlayer(BoardManager.Instance.currentTeam);
        UpdateTurnIcons(data.currentTurn);
        UpdateWinIcons(data.winner);
    }

    /// <summary>
    /// Display the player name
    /// </summary>
    /// <param name="playerID">1 if player1, 2 if player2</param>
    /// <param name="name">The name to display</param>
    private void DisplayPlayerName(int playerID, string name)
    {
        Debug.Log($"Displaying player{playerID} name ({name})");
        if (playerID == 1)
            player1Label.text = name;
        else if (playerID == 2)
            player2Label.text = name;
    }

    private void ColorLocalPlayer(int localPlayerID)
    {
        if (localPlayerID == 1)
        {
            Debug.Log($"Coloring Player 1 to green");
            player1Label.color = Color.green;
            player2Label.color = Color.white;
        }
        else if (localPlayerID == 2)
        {
            Debug.Log($"Coloring Player 2 to green");
            player1Label.color = Color.white;
            player2Label.color = Color.green;
        }

    }


    /// <summary>
    /// Display an icon in front of the player which is the turn to play
    /// </summary>
    /// <param name="turn">1 if it's player1 turn. 2 if it's player2 turn.</param>
    private void UpdateTurnIcons(int turn)
    {
        if (turn == 0)
            return;

        Debug.Log($"Displaying an arrow in front of player{turn}");
        if (turn == 1)
        {
            player1TurnIcon.enabled = true;
            player2TurnIcon.enabled = false;
        }
        else if (turn == 2)
        {
            player1TurnIcon.enabled = false;
            player2TurnIcon.enabled = true;
        }

    }

    /// <summary>
    /// Display a :tada: icon next to the player which has won
    /// </summary>
    /// <param name="winner">1 if player1 has won. 2 if player2 has won.</param>
    private void UpdateWinIcons(int winner)
    {
        if (winner == 0)
        {
            Debug.Log($"There is still no winner");
            return;
        }

        Debug.Log($"Displaying an :tada: in front of player{winner}");
        if (winner == 1)
            player1WinIcon.enabled = true;
        else if (winner == 2)
            player2WinIcon.enabled = true;

        player1TurnIcon.enabled = false;
        player2TurnIcon.enabled = false;
    }
}
