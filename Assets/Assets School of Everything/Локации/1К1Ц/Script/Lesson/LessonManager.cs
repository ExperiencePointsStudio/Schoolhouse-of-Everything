using UnityEngine;
using TMPro;

public class LessonManager : MonoBehaviour
{
    public static LessonManager Instance;   // singleton
    public TMP_Text lessonsCounterText;

    private int completedLessons = 0;
    private int totalLessons = 5;   // можно менять

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddLesson()
    {
        completedLessons++;
        if (completedLessons > totalLessons)
            completedLessons = totalLessons;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (lessonsCounterText != null)
            lessonsCounterText.text = $"Lessons: {completedLessons}/{totalLessons}";
    }
}

