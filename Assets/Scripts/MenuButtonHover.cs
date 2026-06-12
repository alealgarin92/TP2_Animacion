using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("UI References")]
    public Image highlightBorder;
    public Text buttonText;
    public GameObject hoverDecoration;

    private void OnEnable()
    {
        Highlight(false);
    }

    private void Start()
    {
        Highlight(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlight(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Highlight(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Highlight(false);
    }

    private void Highlight(bool isHighlighted)
    {
        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(isHighlighted);
        }
        if (hoverDecoration != null)
        {
            hoverDecoration.SetActive(isHighlighted);
        }
        if (buttonText != null)
        {
            // Optional: light up text slightly, or keep white as in reference
            buttonText.color = isHighlighted ? new Color(0.95f, 0.95f, 0.95f) : Color.white;
        }
    }
}
