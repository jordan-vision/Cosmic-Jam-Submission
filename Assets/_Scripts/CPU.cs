using System;

public class CPU : DecisionMaker
{
    enum AvailableActions
    {
        MOVE,
        BUILD,
        ORBITALSHIFT,
    }

    Array actions = Enum.GetValues(typeof(AvailableActions));

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //playerInfo = GameManager.Instance.Map.CPUPlayerInfo;
    }

    public override void StartTurn()
    {
        base.StartTurn();

        while (ActionsLeft > 0)
        {
            var actionChosen = (AvailableActions)actions.GetValue(UnityEngine.Random.Range(0, actions.Length));

            switch (actionChosen)
            {
                case AvailableActions.MOVE:
                    GameManager.Instance.Map.RandomMove(playerInfo);
                    break;

                case AvailableActions.BUILD:
                    GameManager.Instance.Map.AddShip(playerInfo);
                    break;

                case AvailableActions.ORBITALSHIFT:
                    GameManager.Instance.Map.RandomOrbitalShift();
                    break;

                default:
                    return;
            }

            ActionsLeft--;
        }
    }

    protected override void ScoreObjectives()
    {
        GameManager.Instance.NextTurn();
    }
}
