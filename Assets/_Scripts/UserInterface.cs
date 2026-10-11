using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UserInterface : MonoBehaviour
{
    int currentPage = 0;
    bool pleaseUpdate = true;
    Dictionary<string, string> ruleBookPages;

    [SerializeField] GameObject actions, confirmOrbitalButton, ruleBook, nextButton, previousButton;
    [SerializeField] ObjectiveTab[] objectiveTabs;
    [SerializeField] TextMeshProUGUI turnCounter, infoText, instructionsText, ruleBookTitle, ruleBookContents, helpSection;

    private void Start()
    {
        ruleBookPages = new()
        {
            { "Rules - Basics", 
                "You have 3 actions on your turn.\r\n\r\n" +
                "\tMove: Move a ship from a tile to an adjacent tile.\r\n" +
                "\tBuild: Add a ship to your home tile.\r\n" +
                "\tBattle: (explained on next page)\r\n" +
                "\tOrbital Shift: Rotate a ring around the sun\r\n\r\n" +
                "At the end of your turn, you may score one of the objectives above. This locks the objective from your opponent.\r\n\r\n" +
                "You win if you score 2 objectives." },
            { "Rules - Battle", 
                "When you battle,\r\n\r\n" +
                "\t1. Choose a tile with both players' ships.\r\n\r\n" +
                "\t2. Each of your ships flips 1 coin. For each heads, you destroy one of your opponent's ships.\r\n\r\n" +
                "\t3. Your oppenent does the same as step 2." }
        };
    }

    public void ActivateConfirmOrbitalButton()
    {
        confirmOrbitalButton.SetActive(true);
    }

    public void SetupObjectives()
    {
        for (var i = 0; i < objectiveTabs.Length; i++)
        {
            objectiveTabs[i].TextDescription.text = GameManager.Instance.ObjectiveDeck[i].Description;
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
        instructionsText.gameObject.SetActive(false);
    }

    public void HideActions()
    {
        actions.SetActive(false);
        instructionsText.gameObject.SetActive(true);
    }

    public void UpdateTurnCounter(int value)
    {
        turnCounter.text = $"{value}\n/\n3";
    }

    public void WriteInstrictions(string text)
    {
        instructionsText.text = text;
    }

    public string UpdateInfoText()
    {
        pleaseUpdate = false;
        var finalText = "Score 2 points to win.\n\n";

        foreach (var player in GameManager.Instance.TurnOrder)
        {
            var color = player.Info.PlayerColor;
            var hexCode = ColorUtility.ToHtmlStringRGB(color);
            finalText += $"<color=#{hexCode}>Player 1</color>\n";
            finalText += $"\t{player.Score} Points\n\n";
        }

        infoText.text = finalText;

        return finalText;
    }

    public void BattleSummary((PlayerInfo, PlayerInfo) order, (List<bool>, List<bool>) flips)
    {
        var finalText = UpdateInfoText();
        var p1Flips = flips.Item1.Select(f => f ? "heads" : "tails").ToArray();
        var p2Flips = flips.Item2.Select(f => f ? "heads" : "tails").ToArray();

        finalText += "Battle summary:\n\n";

        finalText += $"<color=#{ColorUtility.ToHtmlStringRGB(order.Item1.PlayerColor)}>Attacker</color>'s flips: {string.Join(", ", p1Flips)}\n";
        finalText += $"Destroyed {flips.Item1.Where(f => f).Count()} ships.\n\n";

        finalText += $"<color=#{ColorUtility.ToHtmlStringRGB(order.Item2.PlayerColor)}>Defender</color>'s flips: {string.Join(", ", p2Flips)}\n";
        finalText += $"Destroyed {flips.Item2.Where(f => f).Count()} ships.\n";

        infoText.text = finalText;
    }

    public void OpenRules()
    {
        ruleBook.SetActive(true);
    }

    public void CloseRules()
    {
        ruleBook.SetActive(false);
    }

    public void ChangePage(int forward)
    {
        currentPage = Util.Mod(currentPage + forward, ruleBookPages.Count);
        var page = ruleBookPages.ElementAt(currentPage);
        ruleBookTitle.text = page.Key;
        ruleBookContents.text = page.Value;

        nextButton.SetActive(currentPage < ruleBookPages.Count - 1);
        previousButton.SetActive(currentPage > 0);
    }

    public void AddHelp(string text)
    {
        helpSection.text = text;
    }

    private void Update()
    {
        if (!pleaseUpdate)
        {
            return;
        }

        UpdateInfoText();
    }
}
