using UnityEngine;
using UnityEngine.Rendering.Universal;
using Visual;

public class PixelationController : MonoBehaviour
{
    [SerializeField] private UniversalRendererData rendererData;

    private PixelationFeature _feature;

    private void Awake()
    {
        // Ищем наш Feature среди всех Renderer Features
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is PixelationFeature pf)
            {
                _feature = pf;
                break;
            }
        }
    }

    // Вызывается из UI Slider
    public void SetPixelation(float value)
    {
        if (_feature == null) return;
        _feature.settings.verticalPixels = Mathf.RoundToInt(value);
        rendererData.SetDirty(); // говорим URP пересобрать пасс
    }

    public float GetPixelation()
    {
        return _feature?.settings.verticalPixels ?? 180;
    }
}