using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    enum MovementStep
    {
        SELECTSTART,
        SELECTDESTINATION,
    }

    MovementStep currentStep = MovementStep.SELECTSTART;
    Grid grid;
    GameObject selectedShip;
    (int, int) selectedShipCoordinates;
    Dictionary<(int, int), GameObject> ships = new();
    [SerializeField] GameObject shipPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GetComponent<Grid>();
        AddShip(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            var tileClicked = grid.WorldToCell(mousePosition);
            Debug.Log($"clicked tile {tileClicked}");

            if (currentStep == MovementStep.SELECTSTART)
            {
                if (ships.TryGetValue((tileClicked.x, tileClicked.y), out var ship))
                {
                    selectedShip = ship;
                    selectedShipCoordinates = (tileClicked.x, tileClicked.y);
                    currentStep = MovementStep.SELECTDESTINATION;

                    Debug.Log($"selected ship {ship}");
                }
                else
                {
                    Debug.Log($"there are no ships here");
                }
            }
            else if (currentStep == MovementStep.SELECTDESTINATION)
            {
                var newTileCoordinates = (tileClicked.x, tileClicked.y);
                if (AreTilesAdjacent(selectedShipCoordinates, newTileCoordinates))
                {
                    //move
                    MoveShip(selectedShipCoordinates, newTileCoordinates);
                    Debug.Log($"moving to {newTileCoordinates}");
                    currentStep = MovementStep.SELECTSTART;
                }
                else
                {
                    Debug.Log($"did not pick an adjacent tile");
                }
            }
        }
    }

    void AddShip(int x, int y)
    {
        var worldPosition = grid.GetCellCenterWorld(new(x, y));
        ships[(x, y)] = Instantiate(shipPrefab, worldPosition, Quaternion.identity, transform);
    }

    void MoveShip((int, int) from, (int, int) to)
    {
        ships[to] = ships[from];
        ships.Remove(from);
        ships[to].transform.position = grid.GetCellCenterWorld(new(to.Item1, to.Item2));
    }

    bool AreTilesAdjacent((int, int) tile1, (int, int) tile2)
    {
        return true;
    }
}
