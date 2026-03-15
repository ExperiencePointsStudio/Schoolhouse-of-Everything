using System;

public static class GameProgress
{
    // Количество пройденных уроков
    public static int LessonsPassed = 0;

    // Общее количество уроков (можно менять)
    public static int TotalLessons = 5;

    // Событие для обновления UI
    public static event Action OnLessonsChanged;

    // Вызвать при прохождении урока
    public static void AddLesson(int amount = 1)
    {
        LessonsPassed += amount;
        OnLessonsChanged?.Invoke();
    }

    // Установить конкретное число (для теста/загрузки)
    public static void SetLessons(int value)
    {
        LessonsPassed = value;
        OnLessonsChanged?.Invoke();
    }
}
