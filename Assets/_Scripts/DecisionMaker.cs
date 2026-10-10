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
    protected PlayerInfo playerInfo;
    protected int score;

    public int ActionsLeft { get { return actionsLeft; } set 
        { 
            actionsLeft = value;
            if (actionsLeft == 0)
            {
                ScoreObjectives();
            }
        } 
    }

    public virtual void StartTurn()
    {
        currentStep = ActionStep.SELECTACTION;
        ActionsLeft = 3;
    }

    protected virtual void ScoreObjectives()
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
