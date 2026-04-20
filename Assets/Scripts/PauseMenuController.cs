using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;

    [Header("Effects")]
    private Volume blurVolume;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    public static bool GameIsPaused = false;

    // Впиши сюда точные названия твоих стартовых сцен
    private readonly string[] menuScenes = { "MainMenu", "BootstrapScene", "Bootstrap" };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void Start()
    {
        ResetMenuState();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetMenuState();

        // Пытаемся найти блюр (проверь, чтобы на сцене он назывался именно так и был включен)
        GameObject blurObj = GameObject.Find("BlurVolume");
        if (blurObj == null) blurObj = GameObject.Find("PauseBlurVolume"); // Резервное имя

        if (blurObj != null) blurVolume = blurObj.GetComponent<Volume>();
    }

    private bool IsInMenuScene()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        foreach (string scene in menuScenes)
        {
            if (activeScene == scene) return true;
        }
        return false;
    }

    private void ResetMenuState()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        // Если мы в меню — курсор свободен, если в игре — залочен
        if (IsInMenuScene())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (blurVolume != null) blurVolume.weight = 0;
    }

    void Update()
    {
        // Блокируем вызов паузы в стартовых сценах
        if (IsInMenuScene())
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenuUI != null && settingsMenuUI.activeInHierarchy)
                CloseSettings();
            else if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        PlayClickSound();
        if (blurVolume != null) blurVolume.weight = 0f;

        pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;

        // Включаем HUD игрока обратно
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ToggleHUD(true);
        }

        // Сбрасываем фокус с UI-кнопок, чтобы курсор не "залипал"
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        HUDManager.Instance.ToggleHUD(false);
        pauseMenuUI.SetActive(true);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (blurVolume != null) blurVolume.weight = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void OpenSettings()
    {
        PlayClickSound();
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayClickSound();
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    public void LoadMenu()
    {
        PlayClickSound();
        Resume();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}