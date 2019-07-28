using TMPro;
using UnityEngine;

public class SimplePopup : AbstractPopup
{
    public static SimplePopup Instance;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI messageLabel;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(string title, string message)
    {
        base.Open(title);
        messageLabel.text = message;
    }
}
