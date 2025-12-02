using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI buttonText;
    private Vector3 originalScale;

    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        originalScale = buttonText.transform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.transform.localScale = originalScale * 1.2f;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.transform.localScale = originalScale;
    }
}
