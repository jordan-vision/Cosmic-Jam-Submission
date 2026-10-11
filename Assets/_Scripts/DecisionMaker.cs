using UnityEngine;

public class DecisionMaker : MonoBehaviour
{
    public enum ActionStep
    {
        SELECTACTION,
        SELECTMOVESTART,
        SELECTMOVEDESTINATION,
        SELECTORBITALSHIFTRING,
        CHOOSEORBITALSHIFTOFFSET,
        SELECTBATTLELOCATION,
    }

    protected int actionsLeft;
    protected ActionStep currentStep = ActionStep.SELECTACTION;
    protected int score;

    [SerializeField] protected PlayerInfo playerInfo;

    public int ActionsLeft { get { return actionsLeft; } set
        {
            actionsLeft = value;

            GameManager.Instance.UI.UpdateTurnCounter(value);
            if (actionsLeft == 0)
            {
                EndOfTurnShenanigans();
            }
            else
            {
                GameManager.Instance.UI.ReassessPossibleActions
                (
                    playerInfo.TotalShips() > 0,
                    playerInfo.Gold >= 1,
                    GameManager.Instance.Map.AreBothPlayersInSameTile(),
                    playerInfo.Gold >= 2
                );
            }
        }
    }

    public PlayerInfo Info => playerInfo;
    public int Score { get { return score; } set 
        {
            score = value;
            GameManager.Instance.UI.UpdateInfoText();
        } 
    }

    public virtual void StartTurn()
    {
        ActionsLeft = 3;
    }

    protected virtual void EndOfTurnShenanigans()
    {

    }

    public virtual void BuildShip()
    {
        GameManager.Instance.Map.AddShip(playerInfo);
    }

    public bool HasShipInSun()
    {
        return GameManager.Instance.Map.IsShipInTile(playerInfo, new(0, 0), out var _);
    }
}
