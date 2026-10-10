using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int currentTurnIndex = 0;

    public CPU CPUPlayer;
    public Map Map;
    public List<Objective> ObjectiveDeck;
    public HumanPlayer HumanPlayer;
    public DecisionMaker[] TurnOrder;
    public UserInterface UI;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        ObjectiveDeck = new()
        {
            new("Have 4 or more ships in the same tile.", p => p.Ships.Values.Any(v => v >= 4), 0),
            new("Have 4 ships in 4 or more different tiles.", p => p.Ships.Values.Where(v => v >= 1).Count() >= 4, 1),
            new("Be the only player with ships on the Sun", p => p.Ships.TryGetValue((0, 0), out var value) && value >= 1 && TurnOrder.Where(dm => dm.HasShipInSun()).Count() == 1 , 2),
        };

        UI.SetupObjectives();
    }

    public bool IsItMyTurn(DecisionMaker decisionMaker)
    {
        return TurnOrder[currentTurnIndex] == decisionMaker;
    }

    public void NextTurn()
    {
        Debug.Log("next turn");
        currentTurnIndex = (currentTurnIndex + 1) % TurnOrder.Length;
        TurnOrder[currentTurnIndex].StartTurn();
    }
}