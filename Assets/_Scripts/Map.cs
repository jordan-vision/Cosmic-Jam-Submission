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
    List<Vector3Int> ring3 = new() { new(0, 3), new(1, 3), new(2, 2), new(2, 1), new(3, 0), new(2, -1), new(2, -2), new(1, -3), new(0, -3), new(-1, -3), new(-2, -3), new(-2, -2), new(-3, -1), new(-3, 0), new(-3, 1), new(-2, 2), new(-2, 3), new(-1, 3) };
    Array rings = Enum.GetValues(typeof(Ring));
    bool planetXDiscovered = false;
    Vector3Int planetXLocation;
    PlayerInfo player1Info;
    PlayerInfo player2Info;

    [SerializeField] TileBase emptyTileBase, homeTileBase, sunTileBase, planetXTileBase;
    [SerializeField] TileBase[] planetTiles;
    [SerializeField] GameObject shipPrefab;
    [SerializeField] Tilemap tilemap;

    public List<Vector3Int> Ring1 => ring1;
    public List<Vector3Int> Ring2 => ring2;
    public List<Vector3Int> Ring3 => ring3;
    public PlayerInfo Player1Info => player1Info;
    public PlayerInfo Player2Info => player2Info;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ring1.ForEach(v => allMapPositions.Add(new(v.x, v.y)));
        ring2.ForEach(v => allMapPositions.Add(new(v.x, v.y)));
        ring3.ForEach(v => allMapPositions.Add(new(v.x, v.y)));
        allMapPositions.Add(new(0, 0));
        grid = GetComponent<Grid>();

        GameManager.Instance.GameReady();
    }

    public void SetupBoard()
    {
        player1Info = GameManager.Instance.TurnOrder[0].Info;
        player2Info = GameManager.Instance.TurnOrder[1].Info;

        player1Info.InitializeDictionaries();
        player2Info.InitializeDictionaries();

        Vector3Int player1Home = new(player1Info.HomeTile.x, player1Info.HomeTile.y);
        Vector3Int player2Home = new(player2Info.HomeTile.x, player2Info.HomeTile.y);

        // Home tile color
        tilemap.SetTileFlags(player1Home, TileFlags.None);
        tilemap.SetTileFlags(player2Home, TileFlags.None);

        Color.RGBToHSV(player1Info.PlayerColor, out var h1, out var s1, out var v1);
        Color.RGBToHSV(player2Info.PlayerColor, out var h2, out var s2, out var v2);
        var tile1Color = Color.HSVToRGB(h1, s1 / 2, v1 / 2);
        var tile2Color = Color.HSVToRGB(h2, s2 / 2, v2 / 2);

        tilemap.SetColor(player1Home, tile1Color);
        tilemap.SetColor(player2Home, tile2Color);

        AddShip(Player1Info);
        AddShip(Player2Info);

        // Place PlanetX
        var emptyTiles = allMapPositions.Where(pos => tilemap.GetTile(pos) == emptyTileBase).ToList();
        planetXLocation = emptyTiles[UnityEngine.Random.Range(0, emptyTiles.Count())];
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
        if (!IsTileInMap(new(tile1.x, tile1.y)) || !IsTileInMap(new(tile2.x, tile2.y)))
        {
            return false;
        }

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

        // Get pre shift coordinates for planet X and homes
        var planetXIndex = ringList.IndexOf(planetXLocation);
        var home1Index = ringList.IndexOf(new(player1Info.HomeTile.x, player1Info.HomeTile.y));
        var home2Index = ringList.IndexOf(new(player2Info.HomeTile.x, player2Info.HomeTile.y));
        

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

        if (planetXIndex != -1)
        {
            planetXLocation = ringList[Util.Mod(planetXIndex + offset, numberOfTiles)];
        }

        if (home1Index != -1)
        {
            var pos = ringList[Util.Mod(home1Index + offset, numberOfTiles)];
            player1Info.HomeTile = new(pos.x, pos.y);

            // Home tile color
            tilemap.SetTileFlags(pos, TileFlags.None);
            Color.RGBToHSV(player1Info.PlayerColor, out var h, out var s, out var v);
            var tileColor = Color.HSVToRGB(h, s / 2, v / 2);
            tilemap.SetColor(pos, tileColor);
        }

        // Shift planet X and home
        if (home2Index != -1)
        {
            var pos = ringList[Util.Mod(home2Index + offset, numberOfTiles)];
            player2Info.HomeTile = new(pos.x, pos.y);

            // Home tile color
            tilemap.SetTileFlags(pos, TileFlags.None);
            Color.RGBToHSV(player2Info.PlayerColor, out var h, out var s, out var v);
            var tileColor = Color.HSVToRGB(h, s / 2, v / 2);
            tilemap.SetColor(pos, tileColor);
        }
    }

    public Vector3Int GetTileAtMouse(Vector3 mousePosition)
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

        Player1Info.Ships.TryGetValue(position, out var player1Ships);
        Player2Info.Ships.TryGetValue(position, out var player2Ships);

        if (player1Ships == 0 && player2Ships == 0)
        {
            return;
        }
        else if (player1Ships > 0 && player2Ships > 0)
        {
            Player1Info.ShipCounters[position].transform.position = tileCenter - new Vector3(.1f, 0);
            Player1Info.ShipCounters[position].transform.localScale = new(.75f, .75f);
            Player2Info.ShipCounters[position].transform.position = tileCenter + new Vector3(.1f, 0);
            Player2Info.ShipCounters[position].transform.localScale = new(.75f, .75f);
        }
        else
        {
            if (player1Ships > 0)
            {
                Player1Info.ShipCounters[position].transform.position = tileCenter;
                Player1Info.ShipCounters[position].transform.localScale = new(1, 1);
            }
            if (player2Ships > 0)
            {
                Player2Info.ShipCounters[position].transform.position = tileCenter;
                Player2Info.ShipCounters[position].transform.localScale = new(1, 1);
            }
        }
    }

    public bool IsThereOtherPlayerAt(PlayerInfo playerInfo, Vector3Int position)
    {
        var toCompare = playerInfo == player1Info ? player2Info : player1Info;
        return toCompare.ShipCounters.TryGetValue((position.x, position.y), out var _);
    }

    public void StartBattle(PlayerInfo attacker, Vector3Int position)
    {
        var defender = attacker == player1Info ? player2Info : player1Info;

        List<bool> attackerFlips = new();
        List<bool> defenderFlips = new();

        for (int i = 0; i < attacker.Ships[(position.x, position.y)]; i++)
        {
            if (UnityEngine.Random.value >= .5f)
            {
                RemoveShip(defender, position.x, position.y);
                attackerFlips.Add(true);
            }
            else
            {
                attackerFlips.Add(false);
            }
        }

        for (int i = 0; i < defender.Ships[(position.x, position.y)]; i++)
        {
            if (UnityEngine.Random.value >= .5f)
            {
                RemoveShip(attacker, position.x, position.y);
                defenderFlips.Add(true);
            }
            else
            {
                defenderFlips.Add(false);
            }
        }

        GameManager.Instance.UI.BattleSummary((attacker, defender), (attackerFlips, defenderFlips));
    }

    public Vector3 GetTileCenter(Vector3Int tile)
    {
        return grid.GetCellCenterWorld(tile);
    }

    public string GetExplanation(Vector3Int position)
    {
        var tileBase = tilemap.GetTile(new(position.x, position.y));

        if (tileBase == emptyTileBase)
        {
            var text = "<b>Empty tile</b>";

            if (!planetXDiscovered)
            {
                text += "\n\n May contain Planet X";
            }

            return text;
        }
        else if (tileBase == homeTileBase)
        {
            return "<b>Home tile</b>\n\nShips you build appear in your home tile";
        }
        else if (tileBase == sunTileBase)
        {
            return "<b>The Sun</b>\n\nAt the start of your turn, if you control the Sun and have 3 or more ships on it, you score 1 point.";
        }
        else if (tileBase == planetXTileBase)
        {
            return "<b>Planet X</b>\n\nThis planets generates 3 Gold per turn instead of 1.";
        }
        else if (planetTiles.Contains(tileBase))
        {
            return "<b>Planet tile</b>\n\nAt the end of your turn, if you have more ships here than your opponent, you control the planet. At the start of your turn, you get Gold depending on how many planets you control.";
        }
        else
        {
            return "";
        }
    }

    public void SetPlanetsControlledBy(PlayerInfo player)
    {
        var otherPlayer = player == player1Info ? player2Info : player1Info;
        var potentialPlanets = allMapPositions.Where(v => planetTiles.Contains(tilemap.GetTile(new(v.x, v.y))));

        foreach (var planet in potentialPlanets)
        {
            // If no ships there, lose control
            if (!player.Ships.TryGetValue((planet.x, planet.y), out var shipCount) || shipCount <= 0)
            {
                player.RemovePlanet(planet);
                continue;
            }

            // If other player has more or same number of ships, status quo
            if (otherPlayer.Ships.TryGetValue((planet.x, planet.y), out var otherPlayerShipCount) && otherPlayerShipCount >= shipCount)
            {
                continue;
            }

            player.AddPlanet(planet);
            otherPlayer.RemovePlanet(planet);
        }
        
        // Do the same with the sun
        if (!player.Ships.TryGetValue((0, 0), out var shipCountSun) || shipCountSun <= 0)
        {
            player.ControllingSun = false;
            CheckPlanetX(player, otherPlayer);
            return;
        }

        if (otherPlayer.Ships.TryGetValue((0, 0), out var otherPlayerShipCountSun) && otherPlayerShipCountSun >= shipCountSun)
        {
            CheckPlanetX(player, otherPlayer);
            return;
        }

        player.ControllingSun = true;
        otherPlayer.ControllingSun = false;
        CheckPlanetX(player, otherPlayer);
    }

    private void CheckPlanetX(PlayerInfo player, PlayerInfo otherPlayer)
    {
        // Same thing with planet X
        if (!player.Ships.TryGetValue((planetXLocation.x, planetXLocation.y), out var shipCountSun) || shipCountSun <= 0)
        {
            player.ControllingPlanetX = false;
            player.RemovePlanet(planetXLocation);
            return;
        }

        if (otherPlayer.Ships.TryGetValue((planetXLocation.x, planetXLocation.y), out var otherPlayerShipCountSun) && otherPlayerShipCountSun >= shipCountSun)
        {
            return;
        }

        player.ControllingPlanetX = true;
        player.AddPlanet(planetXLocation);
        otherPlayer.ControllingPlanetX = false;
        otherPlayer.RemovePlanet(planetXLocation);

        if (!planetXDiscovered)
        {
            tilemap.SetTile(planetXLocation, planetXTileBase);
            planetXDiscovered = true;
            GameManager.Instance.UI.PlanetXDiscovered(player);
        }
    }

    public bool AreBothPlayersInSameTile()
    {
        var players = GameManager.Instance.TurnOrder.ToList();
        return allMapPositions.Any(position => players.All(player => player.Info.Ships.TryGetValue((position.x, position.y), out var count) && count >= 1));
    }
}
