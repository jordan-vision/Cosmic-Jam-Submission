using UnityEngine;

public class DecisionMaker : MonoBehaviour
{
    public enum ActionStep
    {
        SELECTACTION,
        SELECTMOVESTART,
        SELECTMOVEDESTINATION,
        SELECTORBITALSHIFTRING,
        CHOOSEORBITALSHIFTOFFSET
    }

    protected int actionsLeft;
    protected ActionStep currentStep = ActionStep.SELECTACTION;
    protected PlayerInfo playerInfo;

    public int ActionsLeft { get { return actionsLeft; } set 
        { 
            actionsLeft = value;
            if (actionsLeft == 0)
            {
                Debug.Log("Next player's turn");
                GameManager.Instance.NextTurn();
            }
        } 
    }

    public virtual void StartTurn()
    {
        currentStep = ActionStep.SELECTACTION;
        ActionsLeft = 3;
    }
}
