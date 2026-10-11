using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int currentTurnIndex = 0;
    bool ready = false;

    public CPU CPUPlayer;
    public Map Map;
    public List<Objective> ObjectiveDeck;
    public GameObject TileHoverPrefab;
    public HumanPlayer[] TurnOrder;
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
            new("Have 1 or more ship in each of 4 or different tiles.", p => p.Ships.Values.Where(v => v >= 1).Count() >= 4, 1),
            new("Be the only player with ships on the Sun", p => p.Ships.TryGetValue((0, 0), out var value) && value >= 1 && TurnOrder.Where(dm => dm.HasShipInSun()).Count() == 1 , 2),
        };

        UI.SetupObjectives();
    }

    public void GameReady()
    {
        ready = true;
    }

    public bool IsItMyTurn(HumanPlayer decisionMaker)
    {
        return TurnOrder[currentTurnIndex] == decisionMaker;
    }

    public void NextTurn()
    {
        Debug.Log("next turn");
        currentTurnIndex = (currentTurnIndex + 1) % TurnOrder.Length;
        TurnOrder[currentTurnIndex].StartTurn();
    }

    public void ScoreObjective(int id)
    {
        ObjectiveDeck.Find(o => o.Id == id).Scored = true;
    }

    public PlayerInfo ActivePlayer()
    {
        return TurnOrder[currentTurnIndex].Info;
    }

    private void Update()
    {
        // frame 1 things
        if (!ready)
        {
            return;
        }
        else
        {
            ready = false;

            foreach (var human in TurnOrder)
            {
                human.GetRingHover();
            }

            Map.SetupBoard();
            TurnOrder[currentTurnIndex].StartTurn();
        }
    }

    public void StartMove()
    {
        TurnOrder[currentTurnIndex].StartMove();
    }

    public void Build()
    {
        TurnOrder[currentTurnIndex].BuildShip();
    }

    public void Battle()
    {
        TurnOrder[currentTurnIndex].StartBattle();
    }

    public void Orbital()
    {
        TurnOrder[currentTurnIndex].StartOrbitalShift();
    }

    public void ConfirmOrbital()
    {
        TurnOrder[currentTurnIndex].ConfirmOrbitalShift();
    }

    public void Score(int publicId)
    {
        TurnOrder[currentTurnIndex].ScoreObjective(publicId);
    }
}