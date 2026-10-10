using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int currentTurnIndex = 0;

    public GameObject ConfirmOrbitalButton;
    public CPU CPUPlayer;
    public Map Map;
    public HumanPlayer HumanPlayer;
    public DecisionMaker[] TurnOrder;
    
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

    public bool IsItMyTurn(DecisionMaker decisionMaker)
    {
        return TurnOrder[currentTurnIndex] == decisionMaker;
    }

    public void NextTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % TurnOrder.Length;
        TurnOrder[currentTurnIndex].StartTurn();
    }
}