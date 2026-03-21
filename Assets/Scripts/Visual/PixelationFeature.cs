using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Visual
{
    public class PixelationFeature : ScriptableRendererFeature
    {
        public PixelationSettings settings = new PixelationSettings();

        private PixelationPass _pass;
        private Material _material;

        public override void Create()
        {
            Shader shader = Shader.Find("Custom/Pixelation");

            if (shader == null)
            {
                Debug.LogError("[PixelationFeature] Шейдер 'Custom/Pixelation' не найден!");
                return;
            }

            _material = CoreUtils.CreateEngineMaterial(shader);
            _pass = new PixelationPass(settings, _material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (_pass == null) return;
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);  // ← исправлено
        }
    }
}