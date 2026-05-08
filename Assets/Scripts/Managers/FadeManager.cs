using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        // 1. Проверка на синглтон
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 2. ОТВЯЗЫВАЕМ объект от родителей (DontDestroyOnLoad работает только в корне!)
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // 3. Делаем экран черным при создании
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            SetAlpha(1f);
        }
    }

    private void Start()
    {
        // 4. ГАРАНТИЯ выхода из тьмы при самом первом запуске игры
        FadeIn();
    }

    private void OnEnable()
    {
        // Подписываемся на смену активной сцены (это надежнее, чем sceneLoaded)
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        // Как только сцена переключилась — выходим из темноты
        FadeIn();
    }

    public void FadeOut(Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(fadeImage.color.a, 1f, onComplete));
    }

    public void FadeIn(Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(fadeImage.color.a, 0f, onComplete));
    }

    public void SetAlpha(float alpha)
    {
        StopAllCoroutines();
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, Action onComplete)
    {
        float timer = 0f;

        // Защита от зависания, если игра на паузе (Time.timeScale = 0)
        while (timer < fadeDuration)
        {
            // Используем unscaledDeltaTime, чтобы фейд работал даже во время паузы игры!
            timer += Time.unscaledDeltaTime;

            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
                fadeImage.color = color;
            }
            yield return null;
        }

        if (fadeImage != null)
        {
            Color finalColor = fadeImage.color;
            finalColor.a = targetAlpha;
            fadeImage.color = finalColor;
        }

        onComplete?.Invoke();
    }
}