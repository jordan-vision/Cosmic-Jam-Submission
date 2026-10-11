using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    int currentPage = 0;
    string persistentText = "";
    bool pleaseUpdate = true;
    Dictionary<string, string> ruleBookPages;

    [SerializeField] GameObject actions, confirmOrbitalButton, ruleBook, nextButton, previousButton, passButton;
    [SerializeField] ObjectiveTab[] objectiveTabs;
    [SerializeField] TextMeshProUGUI turnCounter, infoText, instructionsText, ruleBookTitle, ruleBookContents, helpSection, whoseTurn, pointCount, goldCount;
    [SerializeField] Button moveButton, buildButton, battleButton, shiftButton;
    [SerializeField] GameObject actionPanel, objectivePanel, sidePanel, titlePanel;

    private void Start()
    {
        ruleBookPages = new()
        {
            { "Rules - Basics", 
                "You have 3 actions on your turn.\r\n\r\n" +
                "\tMove: Move a ship from a tile to an adjacent tile.\r\n" +
                "\tBuild: Spend 1 Gold to add a ship to your home tile.\r\n" +
                "\tBattle: (explained on next page)\r\n" +
                "\tOrbital Shift: Spend 2 Gold to a ring around the sun\r\n\r\n" +
                "At the end of your turn, you may score one of the objectives above. This locks the objective from your opponent.\r\n\r\n" +
                $"You win if you score {GameManager.Instance.PointGoal} Points." },
            { "Rules - Battle", 
                "When you battle,\r\n\r\n" +
                "\t1. Choose a tile with both players' ships.\r\n\r\n" +
                "\t2. Each of your ships flips 1 coin. For each heads, you destroy one of your opponent's ships.\r\n\r\n" +
                "\t3. Your oppenent does the same as step 2." },
            { "Rules - Planets & Sun", 
                "At the end of your turn, if you have more ships at a planet than your oppenent, you start controlling that planet.\r\n\r\n" +
                "At the start of your turn, you get Gold equal to 1 + the number of planets you control.\r\n\r\n" +
                "You can control the Sun the same way. At the start of your turn, if you control the Sun and have 3 or more ships on it, you score 1 point." },
            { "Rules - Planet X", 
                "One of the empty tiles on the map secretly contains Planet X. At the end of your turn, if you control planet X, it is revealed.\r\n\r\n" +
                "Planet X generates 3 Gold instead of 1 each turn." }
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

    public void RemoveObjective(int publicId, out int privateId)
    {
        privateId = objectiveTabs[publicId].ObjectiveId;
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
        var finalText = $"Score {GameManager.Instance.PointGoal} points to win.\n\n";

        foreach (var player in GameManager.Instance.TurnOrder)
        {
            var color = player.Info.PlayerColor;
            var hexCode = ColorUtility.ToHtmlStringRGB(color);
            finalText += $"<color=#{hexCode}>{player.Info.Name}</color>: ";
            finalText += $"{player.Score} Points\n";
            finalText += $"\t{player.Info.Gold} Gold (+{player.Info.GetProductionValue()}/turn)";
            finalText += $"\t{player.Info.GetPlanetCount()} Planets\n";
        }
        var activePlayer = GameManager.Instance.ActivePlayer();
        var activePlayerColor = activePlayer.PlayerColor;
        var activePlayerHex = ColorUtility.ToHtmlStringRGB(activePlayerColor);

        finalText += $"\n<color=#{activePlayerHex}>{activePlayer.Name}</color>'s turn.";
        finalText += persistentText;

        infoText.text = finalText;

        return finalText;
    }

    public void BattleSummary((PlayerInfo, PlayerInfo) order, (List<bool>, List<bool>) flips)
    {
        persistentText = "";

        var finalText = UpdateInfoText();
        var p1Flips = flips.Item1.Select(f => f ? "heads" : "tails").ToArray();
        var p2Flips = flips.Item2.Select(f => f ? "heads" : "tails").ToArray();

        finalText += "\n\nBattle summary:\n\n";

        finalText += $"<color=#{ColorUtility.ToHtmlStringRGB(order.Item1.PlayerColor)}>Attacker</color>'s flips: {string.Join(", ", p1Flips)}. ";
        finalText += $"Destroyed {flips.Item1.Where(f => f).Count()} ships.\n\n";

        finalText += $"<color=#{ColorUtility.ToHtmlStringRGB(order.Item2.PlayerColor)}>Defender</color>'s flips: {string.Join(", ", p2Flips)}. ";
        finalText += $"Destroyed {flips.Item2.Where(f => f).Count()} ships.\n";

        infoText.text = finalText;
    }

    public void SunPoint(PlayerInfo player)
    {
        persistentText = "";

        var finalText = UpdateInfoText();
        finalText += $"\n\n<b><color=#{ColorUtility.ToHtmlStringRGB(player.PlayerColor)}>{player.Name}</color> just scored 1 point from controlling the Sun and having 3+ ships on it.</b>\n";

        infoText.text = finalText;
    }

    public void ObjectiveScored(string objectiveScored, string objectiveReplacement)
    {
        persistentText = "";
        persistentText += $"\n\nObjective <b>{objectiveScored}</b> was scored.\n";
        persistentText += $"Replacing with <b>{objectiveReplacement}</b>.";
        UpdateInfoText();
    }

    public void PlanetXDiscovered(PlayerInfo player)
    {
        persistentText = "";
        persistentText += $"\n\nPlanet X discovered by <color=#{ColorUtility.ToHtmlStringRGB(player.PlayerColor)}>{player.Name}</color>!";

        UpdateInfoText();
    }

    public void GameEndSummary(PlayerInfo winner)
    {
        persistentText = "";

        var finalText = UpdateInfoText();
        finalText += $"\n\n<b><color=#{ColorUtility.ToHtmlStringRGB(winner.PlayerColor)}>{winner.Name}</color> wins!</b>\n";
        finalText += "Thank you for playing my game. Play again or see credits.";

        WriteInstrictions($"<color=#{ColorUtility.ToHtmlStringRGB(winner.PlayerColor)}>{winner.Name}</color> wins!");

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

    public void NewTurn(PlayerInfo playerInfo)
    {
        UpdateInfoText();
        whoseTurn.text = $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.PlayerColor)}>{playerInfo.Name}'s turn</color>";
    }

    public void ShowPass()
    {
        passButton.SetActive(true);
    }

    public void ReassessPossibleActions(bool move, bool build, bool battle, bool shift)
    {
        moveButton.interactable = move;
        buildButton.interactable = build;
        battleButton.interactable = battle;
        shiftButton.interactable = shift;
    }

    public void UpdatePointsAndGoldText(DecisionMaker player)
    {
        pointCount.text = $"SCORE: {player.Score} / {GameManager.Instance.PointGoal}";
        goldCount.text = $"GOLD: {player.Info.Gold} ({player.Info.GetProductionValue()}/turn)";
    }

    public void StartGame()
    {
        actionPanel.SetActive(true);
        objectivePanel.SetActive(true);
        sidePanel.SetActive(true);
        titlePanel.SetActive(false);

        NewTurn(GameManager.Instance.TurnOrder[0].Info);
    }
}
