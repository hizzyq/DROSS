using UnityEngine;
using UnityEngine.UI;

public class BrightnessOverlay : MonoBehaviour
{
    [SerializeField] private GameSettings settings;
    private Image _overlayImage;

    private void Start()
    {
        // Создаём полноэкранный Image
        GameObject overlayGO = new GameObject("BrightnessOverlay", typeof(CanvasRenderer), typeof(Image));
        overlayGO.transform.SetParent(transform, false);

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

        _overlayImage = overlayGO.GetComponent<Image>();
        _overlayImage.color = Color.clear;
        _overlayImage.rectTransform.anchorMin = Vector2.zero;
        _overlayImage.rectTransform.anchorMax = Vector2.one;
        _overlayImage.rectTransform.sizeDelta = Vector2.zero;
        _overlayImage.raycastTarget = false; // Не блокирует клики
    }

    private void Update()
    {
        if (settings == null) return;

        if (settings.brightness <= 1f)
        {
            // 0..1: от чёрного до нормального
            float alpha = 1f - settings.brightness;
            _overlayImage.color = new Color(0f, 0f, 0f, alpha);
        }
        else
        {
            // 1..2: от нормального до белого (ярче)
            float alpha = settings.brightness - 1f;
            _overlayImage.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}