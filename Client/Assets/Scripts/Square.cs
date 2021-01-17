using UnityEngine;
using UnityEngine.EventSystems;

public class Square : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int index;

    private BoardManager board;
    internal int pawnTeam = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (board.currentGame.currentTurn != 0 && board.currentGame.currentTurn == board.currentTeam)
        {
            board.PlacePawn(index, board.currentTeam);
            OnlineManager.Instance.UpdateOnlineGame(board.currentGame);
        }
    }

    public void InstantiatePawn(GameObject pawnPrefab, int team)
    {
        GameObject go = Instantiate(pawnPrefab, transform.position, transform.rotation, transform);
        go.GetComponent<Pawn>().Initialize(team);
        pawnTeam = team;
    }

    public void DeletePawn()
    {
        Destroy(GetComponentInChildren<Pawn>()?.gameObject);
        pawnTeam = 0;
    }

    private void Start()
    {
        board = BoardManager.Instance;
    }
}
