using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public enum Ring
    {
        RING1,
        RING2,
        RING3,
    }

    List<Vector3Int> allMapPositions = new();
    Grid grid;
    List<Vector3Int> ring1 = new() { new(-1, 1), new(0, 1), new(1, 0), new(0, -1), new(-1, -1), new(-1, 0) };
    List<Vector3Int> ring2 = new() { new(0, 2), new(1, 2), new(1, 1), new(2, 0), new(1, -1), new(1, -2), new(0, -2), new(-1, -2), new(-2, -1), new(-2, 0), new(-2, 1), new(-1, 2) };
    List<Vector3Int> ring3 = new() { new(0, 3), new(1, 3), new(2, 2), new(2, 1), new(3, 0), new(2, -1), new(2, -2), new(1, -3), new(0, -3), new(-1, -3), new(-2, -3), new(-2, -2), new(-3, -1), new(-3, 0), new(-3, 1), new(-2, 2), new(-2, 3) };
    Array rings = Enum.GetValues(typeof(Ring));
    PlayerInfo humanPlayerInfo = new(new(-2, -3), Color.red);
    PlayerInfo cpuPlayerInfo = new(new(1, 3), Color.green);

    [SerializeField] GameObject shipPrefab;
    [SerializeField] Tilemap tilemap;

    public PlayerInfo CPUPlayerInfo => cpuPlayerInfo;
    public PlayerInfo HumanPlayerInfo => humanPlayerInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ring1.ForEach(v => allMapPositions.Add(new(v.x, v.y)));
        ring2.ForEach(v => allMapPositions.Add(new(v.x, v.y)));
        ring3.ForEach(v => allMapPositions.Add(new(v.x, v.y)));
        allMapPositions.Add(new(0, 0));
        grid = GetComponent<Grid>();

        AddShip(humanPlayerInfo);
        AddShip(cpuPlayerInfo);

        GameManager.Instance.HumanPlayer.StartTurn();
    }

    public void AddShip(PlayerInfo player, int x, int y)
    {
        if (!player.Ships.TryGetValue((x, y), out var numberOfShipsHere) || numberOfShipsHere == 0)
        {
            var worldPosition = grid.GetCellCenterWorld(new(x, y));
            player.ShipCounters[(x, y)] = Instantiate(shipPrefab, worldPosition, Quaternion.identity, transform);
            player.ShipCounters[(x, y)].GetComponent<ShipCounter>().SetColor(player.PlayerColor);
            player.Ships[(x, y)] = 0;
        }
        player.Ships[(x, y)]++;
        player.ShipCounters[(x, y)].GetComponent<ShipCounter>().SetCounter(player.Ships[(x, y)]);

        AssessCoexistence((x, y));
    }

    public void AddShip(PlayerInfo player)
    {
        AddShip(player, player.HomeTile.x, player.HomeTile.y);
    }

    void RemoveShip(PlayerInfo player, int x, int y)
    {
        if (player.Ships[(x, y)] == 0)
        {
            return;
        }

        player.Ships[(x, y)]--;
        if (player.Ships[(x, y)] == 0)
        {
            Destroy(player.ShipCounters[(x, y)]);
            player.ShipCounters.Remove((x, y));
        }
        else
        {
            player.ShipCounters[(x, y)].GetComponent<ShipCounter>().SetCounter(player.Ships[(x, y)]);
        }

        AssessCoexistence((x, y));
    }

    public void MoveShip(PlayerInfo player, Vector3Int from, Vector3Int to)
    {
        RemoveShip(player, from.x, from.y);
        AddShip(player, to.x, to.y);
    }

    public bool AreTilesAdjacent(Vector3Int tile1, Vector3Int tile2)
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

    List<Vector3Int> GetAllAdjacentTiles(Vector3Int tile)
    {
        List<Vector3Int> results = new()
        {
            new(tile.x - 1, tile.y),
            new(tile.x + 1, tile.y),
            new(tile.x, tile.y - 1),
            new(tile.x, tile.y + 1)
        };

        if (tile.y % 2 == 0)
        {
            results.Add(new(tile.x - 1, tile.y - 1));
            results.Add(new(tile.x - 1, tile.y + 1));
        }
        else
        {
            results.Add(new(tile.x + 1, tile.y - 1));
            results.Add(new(tile.x + 1, tile.y + 1));
        }

        results = results.FindAll(tile => IsTileInMap(tile));

        return results;
    }

    public void OrbitalShift(Ring ring, int offset)
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

    public Vector3Int GetTileClicked(Vector3 mousePosition)
    {
        return grid.WorldToCell(mousePosition);
    }

    public bool IsTileInMap(Vector3Int tile)
    {
        return allMapPositions.Contains(new(tile.x, tile.y));
    }

    public bool IsShipInTile(PlayerInfo player, Vector3Int tile, out GameObject ship)
    {
        return player.ShipCounters.TryGetValue((tile.x, tile.y), out ship);
    }

    public bool IsTileInRing(Vector3Int tile, Ring ring)
    {
        Vector3Int toCompare = new(tile.x, tile.y);
        return ring switch
        {
            Ring.RING1 => ring1.Contains(toCompare),
            Ring.RING2 => ring2.Contains(toCompare),
            Ring.RING3 => ring3.Contains(toCompare),
            _ => false,
        };
    }

    public void RandomMove(PlayerInfo player)
    {
        var startPosition = player.GetRandomShip();
        var startPositionCoordinates = new Vector3Int(startPosition.Key.Item1, startPosition.Key.Item2);

        var possibleDestinations = GetAllAdjacentTiles(startPositionCoordinates);
        var chosenDestination = possibleDestinations[UnityEngine.Random.Range(0, possibleDestinations.Count)];
        MoveShip(player, startPositionCoordinates, chosenDestination);
    }

    public void RandomOrbitalShift()
    {
        var ringChosen = (Ring)rings.GetValue(UnityEngine.Random.Range(0, rings.Length));
        List<Vector3Int> ringList = new();
        switch (ringChosen)
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
        OrbitalShift(ringChosen, UnityEngine.Random.Range(0, ringList.Count));
    }

    void AssessCoexistence((int, int) position)
    {
        var tileCenter = grid.GetCellCenterWorld(new(position.Item1, position.Item2));

        humanPlayerInfo.Ships.TryGetValue(position, out var humanShips);
        cpuPlayerInfo.Ships.TryGetValue(position, out var cpuShips);

        if (humanShips == 0 && cpuShips == 0)
        {
            return;
        }
        else if (humanShips > 0 && cpuShips > 0)
        {
            humanPlayerInfo.ShipCounters[position].transform.position = tileCenter - new Vector3(.1f, 0);
            humanPlayerInfo.ShipCounters[position].transform.localScale = new(.75f, .75f);
            cpuPlayerInfo.ShipCounters[position].transform.position = tileCenter + new Vector3(.1f, 0);
            cpuPlayerInfo.ShipCounters[position].transform.localScale = new(.75f, .75f);
        }
        else
        {
            if (humanShips > 0)
            {
                humanPlayerInfo.ShipCounters[position].transform.position = tileCenter;
                humanPlayerInfo.ShipCounters[position].transform.localScale = new(1, 1);
            }
            if (cpuShips > 0)
            {
                CPUPlayerInfo.ShipCounters[position].transform.position = tileCenter;
                CPUPlayerInfo.ShipCounters[position].transform.localScale = new(1, 1);
            }
        }
    }

    public bool IsThereOtherPlayerAt(PlayerInfo playerInfo, Vector3Int position)
    {
        var toCompare = playerInfo == humanPlayerInfo ? cpuPlayerInfo : humanPlayerInfo;
        return toCompare.ShipCounters.TryGetValue((position.x, position.y), out var _);
    }

    public void StartBattle(PlayerInfo attacker, Vector3Int position)
    {
        var defender = attacker == humanPlayerInfo ? cpuPlayerInfo : humanPlayerInfo;

        for (int i = 0; i < attacker.Ships[(position.x, position.y)]; i++)
        {
            if (UnityEngine.Random.value >= .5f)
            {
                RemoveShip(defender, position.x, position.y);
            }
        }

        for (int i = 0; i < defender.Ships[(position.x, position.y)]; i++)
        {
            if (UnityEngine.Random.value >= .5f)
            {
                RemoveShip(attacker, position.x, position.y);
            }
        }
    }
}
