using System.Net.NetworkInformation;
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
                GameManager.Instance.ConfirmOrbitalButton.SetActive(true);
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
    }

    public void StartOrbitalShift()
    {
        if (!GameManager.Instance.IsItMyTurn(this))
        {
            return;
        }
        currentStep = ActionStep.SELECTORBITALSHIFTRING;
    }

    public void ConfirmOrbitalShift()
    {
        currentStep = ActionStep.SELECTACTION;
        ActionsLeft--;
    }
}
