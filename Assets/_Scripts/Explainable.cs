using UnityEngine;
using UnityEngine.EventSystems;

public class Explainable : MonoBehaviour, IPointerClickHandler
{
    bool isMouseOver;

    [TextArea][SerializeField] string explanation;

    enum ExplainableCategory
    {
        UI,
        COLLIDER,
        TILEMAP,
    }

    [SerializeField] ExplainableCategory category;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameManager.Instance.UI.AddHelp(explanation);
        }
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }

    private void Update()
    {
        if (category == ExplainableCategory.UI)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (category == ExplainableCategory.COLLIDER && isMouseOver)
            {
                GameManager.Instance.UI.AddHelp(explanation);
            }

            else if (category == ExplainableCategory.TILEMAP)
            {
                var tilePosition = GameManager.Instance.Map.GetTileAtMouse(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (!GameManager.Instance.Map.IsTileInMap(tilePosition))
                {
                    return;
                }
                var expl = GameManager.Instance.Map.GetExplanation(tilePosition);
                GameManager.Instance.UI.AddHelp(expl);
            }
        }
    }
}
