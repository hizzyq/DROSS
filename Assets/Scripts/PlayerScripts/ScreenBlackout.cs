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
        if (fadeImage != null)
        {
            // Гарантируем, что при старте сцены панель включена и поверх всего
            fadeImage.gameObject.SetActive(true);
            fadeImage.transform.SetAsLastSibling();
            // Выключаем блокировку кликов, чтобы после осветления можно было играть
            fadeImage.raycastTarget = false;
            StartCoroutine(FadeIn());
        }
    }

    public void StartFade()
    {
        if (fadeImage != null)
        {
            StopAllCoroutines();
            fadeImage.gameObject.SetActive(true);
            fadeImage.transform.SetAsLastSibling();
            // Блокируем UI на время ухода в темноту
            fadeImage.raycastTarget = true;
            StartCoroutine(FadeOut());
        }
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
            timer += Time.unscaledDeltaTime; // Изменено здесь
            yield return null;
        }

        fadeImage.color = endColor;
        // Отключаем объект, чтобы не грузил Canvas, когда прозрачный
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0f, 0f, 0f, 1f);

        while (timer < fadeDuration)
        {
            fadeImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            timer += Time.unscaledDeltaTime; // Изменено здесь
            yield return null;
        }

        fadeImage.color = endColor;
    }

    public void ReverseFade()
    {
        StopAllCoroutines();
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    public void KillFade()
    {
        StopAllCoroutines();
    }
}