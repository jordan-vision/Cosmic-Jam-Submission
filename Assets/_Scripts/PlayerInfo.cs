using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    Dictionary<(int, int), int> ships;
    Dictionary<(int, int), GameObject> shipCounters;

    [SerializeField] Vector2Int homeTile;
    [SerializeField] string name;
    [SerializeField] Color playerColor;

    public Vector2Int HomeTile => homeTile;
    public string Name => name;
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
    }
}
