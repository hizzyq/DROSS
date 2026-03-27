using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelationPass : ScriptableRenderPass
{
    private PixelationSettings _settings;
    private Material _material;

    // Handles для RenderGraph
    private class PassData
    {
        public TextureHandle source;
        public TextureHandle temp;
        public Material material;
    }

    public PixelationPass(PixelationSettings settings, Material material)
    {
        _settings = settings;
        _material = material;
        renderPassEvent = settings.renderPassEvent;
        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph,
        ContextContainer frameData)
    {
        if (_material == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData   = frameData.Get<UniversalCameraData>();

        // Не применяем в Scene View
        if (cameraData.isSceneViewCamera) return;

        // Описываем маленький RT (сам пиксель-эффект)
        int h = _settings.verticalPixels;
        int w = Mathf.RoundToInt(h * cameraData.camera.aspect);

        var desc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
        desc.width        = w;
        desc.height       = h;
        desc.filterMode   = FilterMode.Point;   // ← магия пиксе­лизации
        desc.depthBufferBits = DepthBits.None;
        desc.name         = "_PixelTemp";

        TextureHandle tempHandle = renderGraph.CreateTexture(desc);
        TextureHandle source     = resourceData.activeColorTexture;

        // Запись пасса в граф
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                   "Pixelation Pass", out var passData))
        {
            passData.source   = source;
            passData.temp     = tempHandle;
            passData.material = _material;

            // Читаем source, пишем в temp
            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(tempHandle, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.source,
                    new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // Второй пасс: temp → обратно в активный буфер
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                   "Pixelation Blit Back", out var passData))
        {
            passData.source   = tempHandle;
            passData.temp     = tempHandle;
            passData.material = _material;

            builder.UseTexture(tempHandle, AccessFlags.Read);
            builder.SetRenderAttachment(source, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.source,
                    new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }
}