using TMPro;
using UnityEngine;

public class JoinGameButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI label;

    private string id;
    private GamesList parent;

    public void Initialize(string _id, GamesList _parent)
    {
        label.text = $"{_id} ({OnlineManager.Instance.GetNumberOfPlayerOfGame(_id)}/2)";
        id = _id;
        parent = _parent;
    }
    public void JoinGame()
    {
        OnlineManager.Instance.JoinGame(id, parent.playerIDInputField.text);
        parent.Close();
    }
}
