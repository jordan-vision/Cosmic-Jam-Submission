using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    int battlesThisTurn;
    bool controllingSun, controllingPlanetX;
    int gold;
    List<Vector3Int> planetsControlled;
    Dictionary<(int, int), int> ships;
    Dictionary<(int, int), GameObject> shipCounters;

    [SerializeField] Vector2Int homeTile;
    [SerializeField] string name;
    [SerializeField] Color playerColor;

    public int BattlesThisTurn { get { return battlesThisTurn; } set { battlesThisTurn = value; } }
    public bool ControllingSun { get { return controllingSun; } set { controllingSun = value; } }
    public bool ControllingPlanetX { get { return controllingPlanetX; } set { controllingPlanetX = value; } }
    public int Gold { get { return gold; } set { gold = value; GameManager.Instance.UI.UpdateInfoText(); } }
    public Vector2Int HomeTile { get { return homeTile; } set { homeTile = value; } }
    public string Name { get { return name; } set { name = value; } }
    public Dictionary<(int, int), int> Ships => ships;
    public Dictionary<(int, int), GameObject> ShipCounters => shipCounters;
    public Color PlayerColor => playerColor;

    public PlayerInfo(string name, Vector2Int home, Color color)
    {
        this.name = name;
        homeTile = home;
        playerColor = color;
    }

    public KeyValuePair<(int, int), GameObject> GetRandomShip()
    {
        return ShipCounters.ElementAt(UnityEngine.Random.Range(0, ShipCounters.Count));
    }

    public void InitializeDictionaries()
    {
        ships = new();
        shipCounters = new();
        planetsControlled = new();
    }

    public void AddPlanet(Vector3Int planet)
    {
        if (planetsControlled.Contains(planet))
        {
            return;
        }

        planetsControlled.Add(planet);
    }

    public void RemovePlanet(Vector3Int planet)
    {
        if (!planetsControlled.Contains(planet))
        {
            return;
        }

        planetsControlled.Remove(planet);
    }

    public int GetPlanetCount() => planetsControlled.Count;

    public int TotalShips() => ships.Sum(s => s.Value);

    public int GetProductionValue() => 1 + planetsControlled.Count + (controllingPlanetX ? 2 : 0);
}
