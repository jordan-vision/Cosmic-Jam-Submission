
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanPlayer : DecisionMaker
{
    GameObject mainTileHover;
    Predicate<Vector3Int> mainTileHoverEligibility;
    GameObject auxTileHover;
    Predicate<Vector3Int> auxTileHoverEligibility;
    Map.Ring selectedRing;
    Vector3Int selectedShipCoordinates;
    List<GameObject> ring1Hover = new();
    List<GameObject> ring2Hover = new();
    List<GameObject> ring3Hover = new();

    public ActionStep CurrentStep { get { return currentStep; } set 
        { 
            currentStep = value;

            if (value == ActionStep.SELECTACTION)
            {
                GameManager.Instance.UI.ShowActions();
                mainTileHover.SetActive(false);
                auxTileHover.SetActive(false);
                ring1Hover.ForEach(h => h.SetActive(false));
                ring2Hover.ForEach(h => h.SetActive(false));
                ring3Hover.ForEach(h => h.SetActive(false));
            }

            if (value == ActionStep.SELECTMOVESTART || value == ActionStep.SELECTORBITALSHIFTRING || value == ActionStep.SELECTBATTLELOCATION)
            {
                GameManager.Instance.UI.HideActions();
            }

            if (value == ActionStep.SELECTMOVESTART || value == ActionStep.SELECTBATTLELOCATION)
            {
                mainTileHover.SetActive(true);

                if (value == ActionStep.SELECTMOVESTART)
                {
                    mainTileHoverEligibility = t => GameManager.Instance.Map.IsShipInTile(playerInfo, t, out var _);
                }
                if (value == ActionStep.SELECTBATTLELOCATION)
                {
                    mainTileHoverEligibility = t => GameManager.Instance.Map.IsShipInTile(playerInfo, t, out var _) && GameManager.Instance.Map.IsThereOtherPlayerAt(playerInfo, t);
                }
            }

            if (value == ActionStep.SELECTMOVEDESTINATION)
            {
                auxTileHover.SetActive(true);
                auxTileHoverEligibility = t => GameManager.Instance.Map.AreTilesAdjacent(t, selectedShipCoordinates);
            }

            switch(value)
            {
                case ActionStep.SELECTMOVESTART:
                    GameManager.Instance.UI.WriteInstrictions("Choose a tile to move from.");
                    break;

                case ActionStep.SELECTMOVEDESTINATION:
                    GameManager.Instance.UI.WriteInstrictions("Choose a tile to move into.");
                    break;

                case ActionStep.SELECTORBITALSHIFTRING:
                    GameManager.Instance.UI.WriteInstrictions("Choose a ring to shift.");
                    break;

                case ActionStep.CHOOSEORBITALSHIFTOFFSET:
                    GameManager.Instance.UI.WriteInstrictions("Use scrollwheel to shift.\nClick NEXT when done.");
                    break;

                case ActionStep.SELECTBATTLELOCATION:
                    GameManager.Instance.UI.WriteInstrictions("Choose a tile to battle in.");
                    break;
            }
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainTileHover = Instantiate(GameManager.Instance.TileHoverPrefab);
        auxTileHover = Instantiate(GameManager.Instance.TileHoverPrefab);
        var auxColor = auxTileHover.GetComponent<SpriteRenderer>().color;
        auxTileHover.GetComponent<SpriteRenderer>().color = new(auxColor.r, auxColor.g, auxColor.b, 0.25f);
    }

    public void GetRingHover()
    {
        foreach (var position in GameManager.Instance.Map.Ring1)
        {
            ring1Hover.Add(Instantiate(GameManager.Instance.TileHoverPrefab, GameManager.Instance.Map.GetTileCenter(position), Quaternion.identity));
        }
        foreach (var position in GameManager.Instance.Map.Ring2)
        {
            ring2Hover.Add(Instantiate(GameManager.Instance.TileHoverPrefab, GameManager.Instance.Map.GetTileCenter(position), Quaternion.identity));
        }
        foreach (var position in GameManager.Instance.Map.Ring3)
        {
            ring3Hover.Add(Instantiate(GameManager.Instance.TileHoverPrefab, GameManager.Instance.Map.GetTileCenter(position), Quaternion.identity));
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (CurrentStep == ActionStep.SELECTACTION || !GameManager.Instance.IsItMyTurn(this))
        {
            return;
        }

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mainTileHover.activeSelf && (currentStep == ActionStep.SELECTMOVESTART || currentStep == ActionStep.SELECTBATTLELOCATION))
        {
            var tile = GameManager.Instance.Map.GetTileAtMouse(mousePosition);
            if (mainTileHoverEligibility(tile))
            {
                var tileCenter = GameManager.Instance.Map.GetTileCenter(tile);
                mainTileHover.transform.position = new(tileCenter.x, tileCenter.y);
                mainTileHover.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                mainTileHover.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        if (auxTileHover.activeSelf)
        {
            var tile = GameManager.Instance.Map.GetTileAtMouse(mousePosition);
            if (auxTileHoverEligibility(tile))
            {
                var tileCenter = GameManager.Instance.Map.GetTileCenter(tile);
                auxTileHover.transform.position = new(tileCenter.x, tileCenter.y);
                auxTileHover.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                auxTileHover.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        if (currentStep == ActionStep.SELECTORBITALSHIFTRING)
        {
            var tile = GameManager.Instance.Map.GetTileAtMouse(mousePosition);

            if (GameManager.Instance.Map.IsTileInRing(tile, Map.Ring.RING1))
            {
                ring1Hover.ForEach(h => h.SetActive(true));
            }
            else
            {
                ring1Hover.ForEach(h => h.SetActive(false));
            }
            if (GameManager.Instance.Map.IsTileInRing(tile, Map.Ring.RING2))
            {
                ring2Hover.ForEach(h => h.SetActive(true));
            }
            else
            {
                ring2Hover.ForEach(h => h.SetActive(false));
            }
            if (GameManager.Instance.Map.IsTileInRing(tile, Map.Ring.RING3))
            {
                ring3Hover.ForEach(h => h.SetActive(true));
            }
            else
            {
                ring3Hover.ForEach(h => h.SetActive(false));
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var tileClicked = GameManager.Instance.Map.GetTileAtMouse(mousePosition);
            if (!GameManager.Instance.Map.IsTileInMap(tileClicked))
            {
                return;
            }

            if (CurrentStep == ActionStep.SELECTMOVESTART)
            {
                if (GameManager.Instance.Map.IsShipInTile(playerInfo, tileClicked, out var ship))
                {
                    selectedShipCoordinates = tileClicked;
                    CurrentStep = ActionStep.SELECTMOVEDESTINATION;
                }
                else
                {
                }
            }
            else if (CurrentStep == ActionStep.SELECTMOVEDESTINATION)
            {
                var newTileCoordinates = tileClicked;
                if (GameManager.Instance.Map.AreTilesAdjacent(selectedShipCoordinates, newTileCoordinates))
                {
                    GameManager.Instance.Map.MoveShip(playerInfo, selectedShipCoordinates, newTileCoordinates);
                    CurrentStep = ActionStep.SELECTACTION;
                    ActionsLeft--;
                }
                else
                {
                }
            }
            else if (CurrentStep == ActionStep.SELECTORBITALSHIFTRING)
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
                CurrentStep = ActionStep.CHOOSEORBITALSHIFTOFFSET;
                GameManager.Instance.UI.ActivateConfirmOrbitalButton();
            }
            else if (CurrentStep == ActionStep.SELECTBATTLELOCATION)
            {
                if (GameManager.Instance.Map.IsThereOtherPlayerAt(playerInfo, tileClicked))
                {
                    GameManager.Instance.Map.StartBattle(playerInfo, tileClicked);
                    CurrentStep = ActionStep.SELECTACTION;
                    playerInfo.BattlesThisTurn++;
                    ActionsLeft--;
                }
                else
                {
                    return;
                }
            }
        }

        if (CurrentStep == ActionStep.CHOOSEORBITALSHIFTOFFSET)
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

        CurrentStep = ActionStep.SELECTMOVESTART;
    }

    public void StartOrbitalShift()
    {
        if (!GameManager.Instance.IsItMyTurn(this))
        {
            return;
        }
        CurrentStep = ActionStep.SELECTORBITALSHIFTRING;
    }

    public void ConfirmOrbitalShift()
    {
        CurrentStep = ActionStep.SELECTACTION;
        playerInfo.Gold -= 2;
        ActionsLeft--;
    }

    protected override void EndOfTurnShenanigans()
    {
        GameManager.Instance.UI.HideActions();

        // Check for control
        GameManager.Instance.Map.SetPlanetsControlledBy(playerInfo);

        // Score objectives
        var scorable = GameManager.Instance.ObjectiveDeck.Take(3).Where(o => o.Condition(playerInfo)).ToList();

        if (scorable.Count() == 0)
        {
            GameManager.Instance.UI.WriteInstrictions("You have no actions left.\nPass your turn.");
        }
        else
        {
            GameManager.Instance.UI.WriteInstrictions("You may score an objective.\nChoose which to score or pass.");
            GameManager.Instance.UI.HighlightObjectives(scorable);
        }

        GameManager.Instance.UI.ShowPass();
        GameManager.Instance.UI.UpdateInfoText();
    }

    public void ScoreObjective(int publicId)
    {
        Score++;
        GameManager.Instance.UI.RemoveObjective(publicId, out var privateId);
        GameManager.Instance.ScoreObjective(privateId, out var objective);
        if (objective.WhenScored != null)
        {
            objective.WhenScored(playerInfo);
        }
        GameManager.Instance.UI.HideAllScoreButtons();

        if (Score >= GameManager.Instance.PointGoal)
        {
            GameManager.Instance.EndGame(playerInfo);
        }
        else
        {
            GameManager.Instance.NextTurn();
        }
    }

    public override void BuildShip()
    {
        CurrentStep = ActionStep.SELECTACTION;
        base.BuildShip();
        playerInfo.Gold -= 1;
        ActionsLeft--;
    }

    public override void StartTurn()
    { 
        GameManager.Instance.UI.NewTurn(playerInfo);
        CurrentStep = ActionStep.SELECTACTION;
        playerInfo.BattlesThisTurn = 0;
        playerInfo.Gold += playerInfo.GetProductionValue();

        GameManager.Instance.UI.UpdateInfoText();
        GameManager.Instance.UI.UpdatePointsAndGoldText(this);

        if (playerInfo.ControllingSun && playerInfo.Ships[(0, 0)] >= 3)
        {
            Score++;
            GameManager.Instance.UI.SunPoint(playerInfo);
        }

        base.StartTurn();
    }

    public void StartBattle()
    {
        CurrentStep = ActionStep.SELECTBATTLELOCATION;
    }
}
