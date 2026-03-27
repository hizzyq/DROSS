using System.Collections;
using UnityEngine;

public class LightFlickerController : MonoBehaviour
{
    [Header("Light Components")]
    public Light targetLight;
    public Renderer[] bulbRenderers;

    [Header("Materials")]
    public Material bulbOnMaterial;
    public Material bulbOffMaterial;

    [Header("Flicker Settings")]
    public int flickerCount = 4;
    public float minFlickerDuration = 0.05f;
    public float maxFlickerDuration = 0.1f;


    private bool isFlickering = false;


    public void TriggerFlickerSequence(bool onAtEnd)
    {
        if (!isFlickering)
        {
            StartCoroutine(FlickerLights(onAtEnd));
        }
    }

    private IEnumerator FlickerSequence(bool onAtEnd)
    {
        yield return StartCoroutine(FlickerLights(onAtEnd));
    }

    public IEnumerator FlickerLights(bool onAtEnd)
    {
        isFlickering = true;

        for (int i = 0; i < flickerCount; i++)
        {
            targetLight.enabled = false;
            foreach (Renderer r in bulbRenderers)
            { 
                r.material = bulbOffMaterial;
            }

            yield return new WaitForSeconds(Random.Range(minFlickerDuration, maxFlickerDuration));

            targetLight.enabled = true;
            foreach (Renderer r in bulbRenderers)
            {
                r.material = bulbOnMaterial;
            }

            yield return new WaitForSeconds(Random.Range(minFlickerDuration, maxFlickerDuration));
        }

        targetLight.enabled = onAtEnd;
        foreach (Renderer r in bulbRenderers)
        {
            r.material = onAtEnd ? bulbOnMaterial : bulbOffMaterial;
        }

        isFlickering = false;
    }

    public void TurnOff()
    {
        targetLight.enabled = false;
        foreach (Renderer r in bulbRenderers)
        {
            r.material = bulbOffMaterial;
        }
    }

    public void TurnOn()
    {
        targetLight.enabled = true;
        foreach (Renderer r in bulbRenderers)
        {
            r.material = bulbOnMaterial;
        }
    }
}
