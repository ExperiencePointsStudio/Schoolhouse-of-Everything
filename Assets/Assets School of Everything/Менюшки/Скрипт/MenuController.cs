using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;
using Cinemachine;

public class MenuController : MonoBehaviour
{
    [Header("Заставка ХПС")]
    public VideoPlayer hpsLogoPlayer;
    public RawImage hpsLogoImage;

    [Header("Дисклеймер")]
    public CanvasGroup disclaimerGroup;
    public TMP_Text disclaimerText;

    [Header("Камеры")]

    public CinemachineVirtualCamera virtualCamera1; // смотрит на дверь + лого
    public CinemachineVirtualCamera virtualCamera2; // влетает к монитору

    [Header("Сцена")]
    public Light roomLight;
    public GameObject door;
    public Animator doorAnimator;

    [Header("Монитор")]
    public VideoPlayer monitorVideoPlayer;
    public GameObject monitorIdleScreen; // объект с анимацией ожидания Win95

    [Header("UI — Главное меню")]
    public CanvasGroup mainMenuGroup;
    public GameObject logoGameObject;
    public Button playButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("UI — Подменю Играть")]
    public CanvasGroup playSubmenuGroup;
    public Button continueButton;
    public Button newGameButton;
    public Button backFromPlayButton;

    [Header("UI — Настройки")]
    public CanvasGroup settingsGroup;
    public Button backFromSettingsButton;

    [Header("UI — Подтверждение выхода")]
    public CanvasGroup exitConfirmGroup;
    public Button exitYesButton;
    public Button exitNoButton;

    [Header("UI — Меню монитора")]
    public CanvasGroup monitorMenuGroup;
    public Button loadGameButton;
    public Button exitToMenuButton;

    [Header("Загрузка")]
    public CanvasGroup loadingGroup;
    public CanvasGroup loadingSpinner;

    [Header("Настройки переходов")]
    public float fadeDuration = 1f;
    public float disclaimerDuration = 3f;
    public string gameSceneName = "1К1Ц";

    private bool isTransitioning = false;

    void Start()
    {
        InitUI();
        StartCoroutine(PlayIntroSequence());

        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        backFromPlayButton.onClick.AddListener(OnBackFromPlay);
        backFromSettingsButton.onClick.AddListener(OnBackFromSettings);
        exitYesButton.onClick.AddListener(OnExitConfirmed);
        exitNoButton.onClick.AddListener(OnExitCancelled);
        loadGameButton.onClick.AddListener(OnLoadGame);
        exitToMenuButton.onClick.AddListener(OnExitToMenu);
    }

    void InitUI()
    {
        SetCanvasGroup(hpsLogoImage.GetComponent<CanvasGroup>(), 0, false);
        SetCanvasGroup(disclaimerGroup, 0, false);
        SetCanvasGroup(mainMenuGroup, 0, false);
        SetCanvasGroup(playSubmenuGroup, 0, false);
        SetCanvasGroup(settingsGroup, 0, false);
        SetCanvasGroup(exitConfirmGroup, 0, false);
        SetCanvasGroup(monitorMenuGroup, 0, false);
        SetCanvasGroup(loadingGroup, 0, false);

        if (roomLight != null) roomLight.intensity = 0f;
        virtualCamera1.Priority = 10;
        virtualCamera2.Priority = 0;
    }

    // =====================
    // ИНТРО
    // =====================

    IEnumerator PlayIntroSequence()
    {
        // 1. Лого ХПС
        CanvasGroup hpsGroup = hpsLogoImage.GetComponent<CanvasGroup>();
        hpsLogoPlayer.Play();
        yield return StartCoroutine(FadeCanvasGroup(hpsGroup, 0, 1, fadeDuration));
        yield return new WaitUntil(() => !hpsLogoPlayer.isPlaying);
        yield return StartCoroutine(FadeCanvasGroup(hpsGroup, 1, 0, fadeDuration));

        // 2. Дисклеймер
        SetCanvasGroup(disclaimerGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(disclaimerGroup, 0, 1, fadeDuration));
        yield return new WaitForSeconds(disclaimerDuration);
        yield return StartCoroutine(FadeCanvasGroup(disclaimerGroup, 1, 0, fadeDuration));

        // 3. Включается свет
        yield return StartCoroutine(FadeLight(0f, 1f, 1.5f));

        // 4. Камера плавно уходит вправо (Virtual Camera 1 уже смотрит на дверь+лого)
        // пауза чтобы игрок увидел дверь
        yield return new WaitForSeconds(0.5f);

        // 5. Показываем главное меню
        SetCanvasGroup(mainMenuGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(mainMenuGroup, 0, 1, fadeDuration));
    }

    // =====================
    // ГЛАВНОЕ МЕНЮ
    // =====================

    void OnPlayClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(ShowPlaySubmenu());
    }

    IEnumerator ShowPlaySubmenu()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeCanvasGroup(mainMenuGroup, 1, 0, 0.3f));
        SetCanvasGroup(mainMenuGroup, 0, false);
        SetCanvasGroup(playSubmenuGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(playSubmenuGroup, 0, 1, 0.3f));
        isTransitioning = false;
    }

    void OnSettingsClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(ShowSettings());
    }

    IEnumerator ShowSettings()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeCanvasGroup(mainMenuGroup, 1, 0, 0.3f));
        SetCanvasGroup(mainMenuGroup, 0, false);
        SetCanvasGroup(settingsGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(settingsGroup, 0, 1, 0.3f));
        isTransitioning = false;
    }

    void OnExitClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(ShowExitConfirm());
    }

    IEnumerator ShowExitConfirm()
    {
        isTransitioning = true;
        SetCanvasGroup(exitConfirmGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(exitConfirmGroup, 0, 1, 0.3f));
        isTransitioning = false;
    }

    // =====================
    // НАЗАД
    // =====================

    void OnBackFromPlay()
    {
        if (isTransitioning) return;
        StartCoroutine(BackToMainMenu(playSubmenuGroup));
    }

    void OnBackFromSettings()
    {
        if (isTransitioning) return;
        StartCoroutine(BackToMainMenu(settingsGroup));
    }

    IEnumerator BackToMainMenu(CanvasGroup from)
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeCanvasGroup(from, 1, 0, 0.3f));
        SetCanvasGroup(from, 0, false);
        SetCanvasGroup(mainMenuGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(mainMenuGroup, 0, 1, 0.3f));
        isTransitioning = false;
    }

    // =====================
    // ВЫХОД
    // =====================

    void OnExitConfirmed()
    {
        StartCoroutine(ExitGame());
    }

    void OnExitCancelled()
    {
        StartCoroutine(FadeCanvasGroup(exitConfirmGroup, 1, 0, 0.3f));
        SetCanvasGroup(exitConfirmGroup, 0, false);
    }

    IEnumerator ExitGame()
    {
        yield return StartCoroutine(FadeCanvasGroup(loadingGroup, 0, 1, fadeDuration));
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // =====================
    // ИГРАТЬ → ВЛЁТ К МОНИТОРУ
    // =====================

    public void OnNewGameClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(FlyToMonitor());
    }

    public void OnContinueClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(FlyToMonitor());
    }

    IEnumerator FlyToMonitor()
    {
        isTransitioning = true;

        // Прячем всё UI
        yield return StartCoroutine(FadeCanvasGroup(playSubmenuGroup, 1, 0, 0.3f));
        SetCanvasGroup(playSubmenuGroup, 0, false);

        // Открываем дверь
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        yield return new WaitForSeconds(1f);

        // Переключаем камеру на Virtual Camera 2 (влетает к монитору)
        virtualCamera1.Priority = 0;
        virtualCamera2.Priority = 20;

        // Ждём пока камера долетит (настраивай время под свою сцену)
        yield return new WaitForSeconds(2.5f);

        // Анимация ожидания Win95 на мониторе
        if (monitorIdleScreen != null)
            monitorIdleScreen.SetActive(true);

        if (monitorVideoPlayer != null)
            monitorVideoPlayer.Play();

        yield return new WaitForSeconds(2f);

        // Показываем меню монитора
        SetCanvasGroup(monitorMenuGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(monitorMenuGroup, 0, 1, fadeDuration));

        isTransitioning = false;
    }

    // =====================
    // МЕНЮ МОНИТОРА
    // =====================

    void OnLoadGame()
    {
        if (isTransitioning) return;
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        isTransitioning = true;
        SetCanvasGroup(loadingGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(loadingGroup, 0, 1, fadeDuration));
        StartCoroutine(SpinnerLoop());
        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        yield return op;
    }

    void OnExitToMenu()
    {
        if (isTransitioning) return;
        StartCoroutine(ExitFromMonitorToMenu());
    }

    IEnumerator ExitFromMonitorToMenu()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeCanvasGroup(monitorMenuGroup, 1, 0, 0.3f));
        SetCanvasGroup(monitorMenuGroup, 0, false);

        // Монитор в режим ожидания
        if (monitorVideoPlayer != null)
            monitorVideoPlayer.Stop();

        // Камера отлетает назад
        virtualCamera2.Priority = 0;
        virtualCamera1.Priority = 10;

        yield return new WaitForSeconds(2.5f);

        // Закрываем дверь
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Close");

        yield return new WaitForSeconds(1f);

        // Возвращаем главное меню
        SetCanvasGroup(mainMenuGroup, 0, true);
        yield return StartCoroutine(FadeCanvasGroup(mainMenuGroup, 0, 1, fadeDuration));

        isTransitioning = false;
    }

    // =====================
    // ЗАГРУЗКА — СПИННЕР
    // =====================

    IEnumerator SpinnerLoop()
    {
        while (true)
        {
            yield return StartCoroutine(FadeCanvasGroup(loadingSpinner, 0, 1, 0.4f));
            yield return StartCoroutine(FadeCanvasGroup(loadingSpinner, 1, 0, 0.4f));
        }
    }

    // =====================
    // УТИЛИТЫ
    // =====================

    IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        group.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    IEnumerator FadeLight(float from, float to, float duration)
    {
        float t = 0f;
        roomLight.intensity = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            roomLight.intensity = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        roomLight.intensity = to;
    }

    void SetCanvasGroup(CanvasGroup group, float alpha, bool interactable)
    {
        group.alpha = alpha;
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }
}