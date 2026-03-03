using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private float scaleFactor = 1.2f;
    [SerializeField] private float animationDuration = 0.1f;

    private TextMeshProUGUI buttonText;
    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateScale(originalScale * scaleFactor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateScale(originalScale));
    }

    IEnumerator AnimateScale(Vector3 targetScale)
    {
        float time = 0;
        Vector3 startScale = transform.localScale;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / animationDuration);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}

