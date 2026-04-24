using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenBlackout : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 7.0f; // Для ухода в темноту
    public float fadeInDuration = 2.0f; // Для появления уровня при загрузке

    private void Start()
    {
        // При старте уровня автоматически осветляем экран
        StartCoroutine(FadeIn());
    }

    public void StartFade()
    {
        // Останавливаем осветление, если оно еще шло, чтобы смерть/смена уровня сработали надежно
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;
        Color startColor = new Color(0f, 0f, 0f, 1f); // Черный
        Color endColor = new Color(0f, 0f, 0f, 0f);   // Прозрачный

        fadeImage.color = startColor;

        while (timer < fadeInDuration)
        {
            fadeImage.color = Color.Lerp(startColor, endColor, timer / fadeInDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = endColor;
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0f, 0f, 0f, 1f); // Black with alpha 1.

        while (timer < fadeDuration)
        {
            fadeImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = endColor;
    }

    public void ReverseFade()
    {
        StopAllCoroutines();
        fadeImage.color = new Color(0f, 0f, 0f, 0f); // Мгновенно делаем прозрачным
    }
    
    public void KillFade()
    {
        StopAllCoroutines();
    }
}