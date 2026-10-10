using UnityEngine;

public class ShipCounter : MonoBehaviour
{
    int counter = 0;
    int Counter { get { return counter; } set { counter = value; textMesh.text = counter.ToString(); } }

    [SerializeField] TextMesh textMesh;

    public void SetCounter(int value)
    {
        Counter = value;
    }

    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}
