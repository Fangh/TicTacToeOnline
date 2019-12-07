using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private SpriteRenderer myRenderer;
    [SerializeField] private Sprite[] possibleSprites;

    public void Initialize(int currentTeam)
    {
        switch (currentTeam)
        {
            case 0:
                Debug.LogError("Please choose a team");
                break;
            case 1:
                myRenderer.sprite = possibleSprites[0];
                break;
            case 2:
                myRenderer.sprite = possibleSprites[1];
                break;
            default:
                break;
        }
    }
}
