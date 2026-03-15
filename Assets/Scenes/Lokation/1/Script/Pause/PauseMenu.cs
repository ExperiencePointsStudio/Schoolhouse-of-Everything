using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused;

    [Header("UI")]
    public GameObject pauseRoot;

    bool isPaused;

    void Start()
    {
        IsPaused = false;
        isPaused = false;

        pauseRoot.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ВАЖНО: теперь кнопка 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("PAUSE BUTTON PRESSED");

            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        IsPaused = true;
        isPaused = true;

        pauseRoot.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        IsPaused = false;
        isPaused = false;

        pauseRoot.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenSettings()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene("SettingsScene");
    }

    public void ExitToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuScene");
    }
}
