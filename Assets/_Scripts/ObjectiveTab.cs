using TMPro;
using UnityEngine;

public class ObjectiveTab : MonoBehaviour
{
    int objectiveId;
    [SerializeField] GameObject scoreButton;
    [SerializeField] TextMeshProUGUI textDescription;

    public int ObjectiveId { get { return objectiveId; } set { objectiveId = value; } }
    public TextMeshProUGUI TextDescription => textDescription;

    public void ShowScoreButton()
    {
        scoreButton.SetActive(true);
    }

    public void HideScoreButton()
    {
        scoreButton.SetActive(false);
    }
}
