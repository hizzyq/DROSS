using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class PixelationSettings
{
    [Range(64, 540)]
    public int verticalPixels = 180;

    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
}