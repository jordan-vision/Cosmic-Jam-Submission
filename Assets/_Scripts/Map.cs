using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    enum ActionStep
    {
        SELECTACTION,
        SELECTMOVESTART,
        SELECTMOVEDESTINATION,
        SELECTORBITALSHIFTRING,
        CHOOSEORBITALSHIFTOFFSET
    }

    enum Ring
    {
        RING1,
        RING2,
        RING3,
    }

    ActionStep currentStep = ActionStep.SELECTACTION;
    Grid grid;
    Ring selectedRing;
    GameObject selectedShip;
    Vector3Int selectedShipCoordinates;
    List<Vector3Int> ring1 = new() { new(-1, 1), new(0, 1), new(1, 0), new(0, -1), new(-1, -1), new(-1, 0) };
    List<Vector3Int> ring2 = new() { new(0, 2), new(1, 2), new(1, 1), new(2, 0), new(1, -1), new(1, -2), new(0, -2), new(-1, -2), new(-2, -1), new(-2, 0), new(-2, 1), new(-1, 2) };
    List<Vector3Int> ring3 = new() { new(0, 3), new(1, 3), new(2, 2), new(2, 1), new(3, 0), new(2, -1), new(2, -2), new(1, -3), new(0, -3), new(-1, -3), new(-2, -3), new(-2, -2), new(-3, -1), new(-3, 0), new(-3, 1), new(-2, 2), new(-2, 3) };
    Dictionary<(int, int), GameObject> ships = new();

    [SerializeField] GameObject shipPrefab;
    [SerializeField] Tilemap tilemap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GetComponent<Grid>();
        AddShip(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentStep == ActionStep.SELECTACTION)
        {
            return;
        }

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            var tileClicked = grid.WorldToCell(mousePosition);

            if (currentStep == ActionStep.SELECTMOVESTART)
            {
                if (ships.TryGetValue((tileClicked.x, tileClicked.y), out var ship))
                {
                    selectedShip = ship;
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
                if (AreTilesAdjacent(selectedShipCoordinates, newTileCoordinates))
                {
                    MoveShip(selectedShipCoordinates, newTileCoordinates);
                    Debug.Log($"moving to {newTileCoordinates}");
                    currentStep = ActionStep.SELECTACTION;
                }
                else
                {
                    Debug.Log($"did not pick an adjacent tile");
                }
            }
            else if (currentStep == ActionStep.SELECTORBITALSHIFTRING)
            {
                Vector3Int toCompare = new(tileClicked.x, tileClicked.y);
                if (ring1.Contains(toCompare))
                {
                    selectedRing = Ring.RING1;
                } 
                else if (ring2.Contains(toCompare))
                {
                    selectedRing = Ring.RING2;
                }
                else if (ring3.Contains(toCompare)) {
                    selectedRing = Ring.RING3;
                }
                currentStep = ActionStep.CHOOSEORBITALSHIFTOFFSET;
            }
        }

        if (currentStep == ActionStep.CHOOSEORBITALSHIFTOFFSET)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                OrbitalShift(selectedRing, -1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                OrbitalShift(selectedRing, 1);
            }
        }
    }

    void AddShip(int x, int y)
    {
        var worldPosition = grid.GetCellCenterWorld(new(x, y));
        ships[(x, y)] = Instantiate(shipPrefab, worldPosition, Quaternion.identity, transform);
    }

    void MoveShip(Vector3Int from, Vector3Int to)
    {
        ships[(to.x, to.y)] = ships[(from.x, from.y)];
        ships.Remove((from.x, from.y));
        ships[(to.x, to.y)].transform.position = grid.GetCellCenterWorld(new(to.x, to.y));
    }

    bool AreTilesAdjacent(Vector3Int tile1, Vector3Int tile2)
    {
        if (tile1.x == tile2.x && tile1.y == tile2.y)
        {
            return false;
        }

        if (tile1.x == tile2.x && Math.Abs(tile1.y - tile2.y) <= 1)
        {
            return true;
        }
        if (tile1.y == tile2.y && Math.Abs(tile1.x - tile2.x) <= 1)
        {
            return true;
        }

        if (tile1.y % 2 == 0)
        {
            return (tile2.x == tile1.x - 1 && Math.Abs(tile1.y - tile2.y) <= 1);
        } 
        else
        {
            return (tile2.x == tile1.x + 1 && Math.Abs(tile1.y - tile2.y) <= 1);
        }
    }

    void OrbitalShift(Ring ring, int offset)
    {
        List<Vector3Int> ringList = new();
        switch (ring)
        {
            case Ring.RING1:
                ringList = ring1;
                break;

            case Ring.RING2:
                ringList = ring2;
                break;

            case Ring.RING3:
                ringList = ring3;
                break;
        }

        var numberOfTiles = ringList.Count;

        List<TileBase> tileBases = Enumerable.Repeat<TileBase>(null, numberOfTiles).ToList();

        // Removing tiles
        for (var i = 0; i < numberOfTiles; i++)
        {
            tileBases[Util.Mod(i + offset, numberOfTiles)] = tilemap.GetTile(ringList[i]);
            tilemap.SetTile(ringList[i], null);
        }

        // Readding tiles
        for (var i = 0; i < numberOfTiles; i++)
        {
            tilemap.SetTile(ringList[i], tileBases[i]);
        }
    }

    public void StartMove()
    {
        currentStep = ActionStep.SELECTMOVESTART;
    }

    public void StartOrbitalShift()
    {
        if (currentStep != ActionStep.SELECTORBITALSHIFTRING)
        {
            currentStep = ActionStep.SELECTORBITALSHIFTRING;
        }
        else
        {
            currentStep = ActionStep.SELECTACTION;
        }
    }
}
