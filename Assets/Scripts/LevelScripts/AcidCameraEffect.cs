using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AcidCameraEffect : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform acid;
    public Transform      player;
    public Volume         postProcessVolume;

    [Header("Настройки эффекта")]
    public float fadeSpeed     = 3f;
    public Color overlayColor  = new Color(0.1f, 0.8f, 0.05f, 0f);
    public float maxVignette   = 0.55f;
    public float maxChromaticAb = 0.4f;

    ColorAdjustments _colorAdj;
    Vignette         _vignette;
    ChromaticAberration _chromatic;
    DepthOfField     _dof;

    [Header("UI оверлей")]
    public CanvasGroup acidOverlay;

    bool  _submerged;
    float _intensity;

    void Start()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out _colorAdj);
            postProcessVolume.profile.TryGet(out _vignette);
            postProcessVolume.profile.TryGet(out _chromatic);
            postProcessVolume.profile.TryGet(out _dof);
        }
    }

    void Update()
    {
        _submerged = player.position.y < acid.transform.position.y;
        float target = _submerged ? 1f : 0f;
        _intensity = Mathf.MoveTowards(_intensity, target, fadeSpeed * Time.deltaTime);

        ApplyEffects(_intensity);
    }

    void ApplyEffects(float t)
    {
        if (acidOverlay != null)
            acidOverlay.alpha = t * 0.25f;

        if (_vignette != null)
            _vignette.intensity.Override(t * maxVignette);

        if (_chromatic != null)
            _chromatic.intensity.Override(t * maxChromaticAb);

        if (_colorAdj != null)
        {
            _colorAdj.colorFilter.Override(Color.Lerp(Color.white,
                new Color(0.6f, 1.2f, 0.5f), t * 0.7f));
            _colorAdj.postExposure.Override(Mathf.Lerp(0f, -0.2f, t));
        }

        if (_dof != null)
        {
            _dof.active = t > 0.05f;
            _dof.gaussianStart.Override(Mathf.Lerp(10f, 0.1f, t));
            _dof.gaussianEnd.Override(Mathf.Lerp(30f, 3f, t));
        }
    }
}