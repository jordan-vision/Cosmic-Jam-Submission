using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UserInterface : MonoBehaviour
{
    [SerializeField] GameObject actions, confirmOrbitalButton;
    [SerializeField] ObjectiveTab[] objectiveTabs;

    public void ActivateConfirmOrbitalButton()
    {
        confirmOrbitalButton.SetActive(true);
    }

    public void SetupObjectives()
    {
        for (var i = 0; i < objectiveTabs.Length; i++)
        {
            objectiveTabs[i].GetComponent<TextMeshProUGUI>().text = GameManager.Instance.ObjectiveDeck[i].Description;
            objectiveTabs[i].ObjectiveId = GameManager.Instance.ObjectiveDeck[i].Id;
        }
    }

    public void HighlightObjectives(List<Objective> objectives)
    {
        foreach (var objectiveTab in objectiveTabs)
        {
            if (!objectives.Select(o => o.Id).Contains(objectiveTab.ObjectiveId))
            {
                continue;
            }

            objectiveTab.ShowScoreButton();
        }
    }

    public void RemoveObjective(int publicId)
    {
        objectiveTabs[publicId].gameObject.SetActive(false);
    }

    public void HideAllScoreButtons()
    {
        foreach (var objectiveTab in objectiveTabs)
        {
            objectiveTab.HideScoreButton();
        }
    }

    public void ShowActions()
    {
        actions.SetActive(true);
    }

    public void HideActions()
    {
        actions.SetActive(false);
    }
}
