using System.Linq;
using UnityEngine;

public class HumanPlayer : DecisionMaker
{
    Map.Ring selectedRing;
    Vector3Int selectedShipCoordinates;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerInfo = GameManager.Instance.Map.HumanPlayerInfo;
    }

    // Update is called once per frame
    private void Update()
    {
        if (currentStep == ActionStep.SELECTACTION || !GameManager.Instance.IsItMyTurn(this))
        {
            return;
        }

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            var tileClicked = GameManager.Instance.Map.GetTileClicked(mousePosition);
            if (!GameManager.Instance.Map.IsTileInMap(tileClicked))
            {
                return;
            }

            if (currentStep == ActionStep.SELECTMOVESTART)
            {
                if (GameManager.Instance.Map.IsShipInTile(playerInfo, tileClicked, out var ship))
                {
                    selectedShipCoordinates = tileClicked;
                    currentStep = ActionStep.SELECTMOVEDESTINATION;

                    Debug.Log($"selected ship {ship}");
                }
                else
                {
                    Debug.Log($"there are no ships here");
                }
            }
            else if (currentStep == ActionStep.SELECTMOVEDESTINATION)
            {
                var newTileCoordinates = tileClicked;
                if (GameManager.Instance.Map.AreTilesAdjacent(selectedShipCoordinates, newTileCoordinates))
                {
                    GameManager.Instance.Map.MoveShip(playerInfo, selectedShipCoordinates, newTileCoordinates);
                    Debug.Log($"moving to {newTileCoordinates}");
                    currentStep = ActionStep.SELECTACTION;
                    ActionsLeft--;
                    GameManager.Instance.UI.ShowActions();
                }
                else
                {
                    Debug.Log($"did not pick an adjacent tile");
                }
            }
            else if (currentStep == ActionStep.SELECTORBITALSHIFTRING)
            {
                if (GameManager.Instance.Map.IsTileInRing(tileClicked, Map.Ring.RING1))
                {
                    selectedRing = Map.Ring.RING1;
                }
                else if (GameManager.Instance.Map.IsTileInRing(tileClicked, Map.Ring.RING2))
                {
                    selectedRing = Map.Ring.RING2;
                }
                else if (GameManager.Instance.Map.IsTileInRing(tileClicked, Map.Ring.RING3))
                {
                    selectedRing = Map.Ring.RING3;
                }
                else
                {
                    return;
                }
                currentStep = ActionStep.CHOOSEORBITALSHIFTOFFSET;
                GameManager.Instance.UI.ActivateConfirmOrbitalButton();
            }
            else if (currentStep == ActionStep.SELECTBATTLELOCATION)
            {
                if (GameManager.Instance.Map.IsThereOtherPlayerAt(playerInfo, tileClicked))
                {
                    GameManager.Instance.Map.StartBattle(playerInfo, tileClicked);
                    GameManager.Instance.UI.ShowActions();
                    ActionsLeft--;
                }
                else
                {
                    return;
                }
            }
        }

        if (currentStep == ActionStep.CHOOSEORBITALSHIFTOFFSET)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                GameManager.Instance.Map.OrbitalShift(selectedRing, -1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                GameManager.Instance.Map.OrbitalShift(selectedRing, 1);
            }
        }
    }

    public void StartMove()
    {
        if (!GameManager.Instance.IsItMyTurn(this))
        {
            return;
        }

        currentStep = ActionStep.SELECTMOVESTART;
        GameManager.Instance.UI.HideActions();
    }

    public void StartOrbitalShift()
    {
        if (!GameManager.Instance.IsItMyTurn(this))
        {
            return;
        }
        currentStep = ActionStep.SELECTORBITALSHIFTRING;
        GameManager.Instance.UI.HideActions();
    }

    public void ConfirmOrbitalShift()
    {
        currentStep = ActionStep.SELECTACTION;
        ActionsLeft--;
        GameManager.Instance.UI.ShowActions();
    }

    protected override void ScoreObjectives()
    {
        GameManager.Instance.UI.HideActions();
        var scorable = GameManager.Instance.ObjectiveDeck.Where(o => o.Condition(playerInfo)).ToList();

        if (scorable.Count() == 0)
        {
            GameManager.Instance.NextTurn();
            return;
        }

        GameManager.Instance.UI.HighlightObjectives(scorable);
    }

    public void ScoreObjective(int publicId)
    {
        score++;
        GameManager.Instance.UI.RemoveObjective(publicId);
        GameManager.Instance.UI.HideAllScoreButtons();
        GameManager.Instance.NextTurn();
    }

    public override void BuildShip()
    {
        base.BuildShip();
        GameManager.Instance.UI.ShowActions();
        ActionsLeft--;
    }

    public override void StartTurn()
    {
        base.StartTurn();
        GameManager.Instance.UI.ShowActions();
    }

    public void StartBattle()
    {
        GameManager.Instance.UI.HideActions();
        currentStep = ActionStep.SELECTBATTLELOCATION;
    }
}
