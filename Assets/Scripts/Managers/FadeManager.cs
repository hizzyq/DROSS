using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        // Настройка Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Объект не будет уничтожаться при смене сцен

        // Убедимся, что при старте игры экран прозрачный
        fadeImage.gameObject.SetActive(true);
        SetAlpha(0f);
    }

    // Метод для затемнения (ухода в черный)
    public void FadeOut(Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(fadeImage.color.a, 1f, onComplete));
    }

    // Метод для осветления (выхода из черного)
    public void FadeIn(Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(fadeImage.color.a, 0f, onComplete));
    }

    // Мгновенное затемнение/осветление (без анимации)
    public void SetAlpha(float alpha)
    {
        StopAllCoroutines();
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, Action onComplete)
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
        onComplete?.Invoke(); // Вызываем действие после завершения (например, загрузку сцены)
    }
}