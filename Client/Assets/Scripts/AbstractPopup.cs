using DG.Tweening;
using TMPro;
using UnityEngine;

public class AbstractPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleLabel;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 0.4f;
    
    public virtual void Open(string title)
    {
        gameObject.SetActive(true);
        transform.DOScale(Vector3.one, animationSpeed).From(Vector3.zero).SetEase(Ease.OutElastic);
        titleLabel.text = title;
    }

    public void Close()
    {
        transform.DOScale(Vector3.zero, animationSpeed).SetEase(Ease.OutCirc).OnComplete(() => { gameObject.SetActive(false); });
    }
}
