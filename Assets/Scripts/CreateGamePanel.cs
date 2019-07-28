using TMPro;
using UnityEngine;

public class CreateGamePanel : AbstractPopup
{
    [Header("References")]
    [SerializeField] private TMP_InputField gameIDInputField;
    [SerializeField] private TMP_InputField playerIDInputField;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void CreateGame()
    {
        BoardManager.Instance.currentTeam = 1;
        OnlineManager.Instance.CreateGame(gameIDInputField.text, playerIDInputField.text);
        Close();
    }
}
