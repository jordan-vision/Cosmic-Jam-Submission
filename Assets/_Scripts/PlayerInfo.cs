using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInfo
{
    Dictionary<(int, int), int> ships = new();
    Dictionary<(int, int), GameObject> shipCounters = new();
    Color playerColor;

    public Dictionary<(int, int), int> Ships => ships;
    public Dictionary<(int, int), GameObject> ShipCounters => shipCounters;
    public Color PlayerColor => playerColor;

    public PlayerInfo(Color color)
    {
        playerColor = color;
    }

    public KeyValuePair<(int, int), GameObject> GetRandomShip()
    {
        return ShipCounters.ElementAt(Random.Range(0, ShipCounters.Count));
    }
}
