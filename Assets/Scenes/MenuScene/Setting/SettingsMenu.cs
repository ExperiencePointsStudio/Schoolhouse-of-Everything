using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsMenuOld : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject howToPlayPanel;
    public GameObject creditsPanel;

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    void Start()
    {
        // ---------- Разрешения ----------
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // ---------- КНОПКИ ----------

    public void Back()
    {
        if (!string.IsNullOrEmpty(SceneHistory.PreviousScene))
        {
            SceneManager.LoadScene(SceneHistory.PreviousScene);
        }
        else
        {
            // Фолбэк на главное меню
            SceneManager.LoadScene("MenuScene");
        }
    }

    public void OpenHowToPlay()
    {
        mainPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseSubmenu()
    {
        howToPlayPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // ---------- НАСТРОЙКИ ----------

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetMusicVolume(float value)
    {
        if (musicSource)
            musicSource.volume = value;
    }
}
