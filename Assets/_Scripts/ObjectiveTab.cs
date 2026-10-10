using UnityEngine;

public class ObjectiveTab : MonoBehaviour
{
    int objectiveId;
    [SerializeField] GameObject scoreButton;

    public int ObjectiveId { get { return objectiveId; } set { objectiveId = value; } }

    public void ShowScoreButton()
    {
        scoreButton.SetActive(true);
    }

    public void HideScoreButton()
    {
        scoreButton.SetActive(false);
    }
}
