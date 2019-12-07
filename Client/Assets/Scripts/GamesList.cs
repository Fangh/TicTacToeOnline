using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamesList : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject gameButtonPrefab;
    [SerializeField] private Transform gamesButtonsRoot;
    public TMP_InputField playerIDInputField;

    [Header("Animation Settings")]
    [SerializeField] private float movementDelta = 1900f;
    [SerializeField] private float movementSpeed = 0.4f;

    private List<GameObject> buttons = new List<GameObject>();

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void AddButton(string gameID)
    {
        GameObject go = Instantiate(gameButtonPrefab, gamesButtonsRoot.position, Quaternion.identity, gamesButtonsRoot);
        go.GetComponentInChildren<JoinGameButton>().Initialize(gameID, this);
        if (OnlineManager.Instance.GetGameDataByID(gameID).winner != 0)
            go.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;

        buttons.Add(go);
    }

    public void Open()
    {
        UpdateList();
        gameObject.SetActive(true);
        transform.DOLocalMoveY(0, movementSpeed).From(movementDelta).SetEase(Ease.OutBack);
    }

    public void Close()
    {
        transform.DOLocalMoveY(movementDelta, movementSpeed).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void UpdateList()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i]);
        }
        buttons.Clear();

        foreach (SGame game in OnlineManager.Instance.GetAllGames())
        {
            AddButton(game.id);
        }
    }

}
