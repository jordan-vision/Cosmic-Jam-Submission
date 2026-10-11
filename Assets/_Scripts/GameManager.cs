using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int currentTurnIndex = 0;
    bool ready = false;

    public CPU CPUPlayer;
    public Map Map;
    public List<Objective> ObjectiveDeck;
    public int PointGoal;
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
            new("Have 3+ ships in a non-home tile", p => p.Ships.Where(kv => kv.Key != (p.HomeTile.x, p.HomeTile.y)).Select(kv => kv.Value).Any(v => v >= 3), 0),
            new("Have 1+ ship in each of 3+ different non-home tiles", p => p.Ships.Where(kv => kv.Key != (p.HomeTile.x, p.HomeTile.y)).Select(kv => kv.Value).Where(v => v >= 1).Count() >= 3, 1),
            new("Control 2+ planets", p => p.GetPlanetCount() >= 2, 2),
            new("Initiate 2+ battles in the same turn", p => p.BattlesThisTurn >= 2, 3),
            new("Have 2+ ships on the Sun while your opponent has 0", p => p.Ships.TryGetValue((0, 0), out var value) && value >= 2 && TurnOrder.Where(dm => dm.HasShipInSun()).Count() == 1 , 4),
            new("have 6+ ships total", p => p.TotalShips() >= 6, 5),
            new("Spend 4 Gold", p => p.Gold >= 4, 6, p => p.Gold -= 4)
        };

        ObjectiveDeck = ObjectiveDeck.OrderBy(_ => Random.value).ToList();
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

    public void ScoreObjective(int id, out Objective objective)
    {
        objective = ObjectiveDeck.Find(o => o.Id == id);
        // Bottom of deck
        ObjectiveDeck.Remove(objective);
        ObjectiveDeck.Add(objective);

        // Rewrite objectives
        UI.SetupObjectives();
        UI.ObjectiveScored(objective.Description, ObjectiveDeck[2].Description);
    }

    public PlayerInfo ActivePlayer()
    {
        return TurnOrder[currentTurnIndex].Info;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quitting game");
        }
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

    public void PlayerScore(int publicId)
    {
        TurnOrder[currentTurnIndex].ScoreObjective(publicId);
    }

    public void EndGame(PlayerInfo winner)
    {
        UI.GameEndSummary(winner);
    }

    public void Restart()
    {
        // Return to main menu? Or reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ChangePlayer1Name(TMP_InputField name)
    {
        TurnOrder[0].Info.Name = name.text;
    }

    public void ChangePlayer2Name(TMP_InputField name)
    {
        TurnOrder[1].Info.Name = name.text;
    }
}