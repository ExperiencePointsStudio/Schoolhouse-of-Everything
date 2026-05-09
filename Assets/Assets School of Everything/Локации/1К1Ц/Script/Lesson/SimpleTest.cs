using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleTestV3 : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text headerText;
    public TMP_Text[] questionTexts;
    public TMP_InputField[] answerFields;
    public TMP_Text[] resultMarks;
    public TMP_Text finalResultText;
    public TMP_Text gradeText;
    public TMP_Text moralStatusText;
    public Button submitButton;

    [Header("External Links")]
    public SeatInteraction seat;

    private int[] correctAnswers = new int[4];
    private string[] operators = { "+", "-", "*", "/" };

    void Start()
    {
        GenerateQuestions();
        submitButton.onClick.AddListener(CheckAnswers);
    }

    void GenerateQuestions()
    {
        headerText.text = "Test #1 – Math";

        for (int i = 0; i < 4; i++)
        {
            int a = Random.Range(1, 11);
            int b = Random.Range(1, 11);
            string op = operators[Random.Range(0, operators.Length)];

            int correct = 0;
            switch (op)
            {
                case "+":
                    correct = a + b;
                    break;
                case "-":
                    correct = a - b;
                    break;
                case "*":
                    correct = a * b;
                    break;
                case "/":
                    correct = a * b;
                    questionTexts[i].text = $"{correct} ÷ {b} = ?";
                    correctAnswers[i] = a;
                    resultMarks[i].text = "";
                    continue;
            }

            questionTexts[i].text = $"{a} {op} {b} = ?";
            correctAnswers[i] = correct;
            resultMarks[i].text = "";
        }

        finalResultText.text = "";
        gradeText.text = "";
        moralStatusText.text = "";
    }

    void CheckAnswers()
    {
        int correctCount = 0;

        for (int i = 0; i < 4; i++)
        {
            int playerAnswer;
            if (int.TryParse(answerFields[i].text, out playerAnswer))
            {
                if (playerAnswer == correctAnswers[i])
                {
                    resultMarks[i].text = "Yes";
                    resultMarks[i].color = Color.green;
                    correctCount++;
                }
                else
                {
                    resultMarks[i].text = "No";
                    resultMarks[i].color = Color.red;
                }
            }
            else
            {
                resultMarks[i].text = "No";
                resultMarks[i].color = Color.red;
            }
        }

        // оценка
        float percent = (correctCount / 4f) * 100f;
        string grade = GetGrade(percent);

        finalResultText.text = $"Result: {correctCount}/4 ({percent:F0}%)";
        gradeText.text = $"Grade: {grade}";
        gradeText.color = GetGradeColor(grade);

        // === МОРАЛЬ ===
        if (MoraleSystem.Instance != null)
        {
            int wrongCount = 4 - correctCount;
            float moraleDelta = correctCount * 5f - wrongCount * 5f;

            if (percent > 50f) moraleDelta += 2f;
            else moraleDelta -= 2f;

            MoraleSystem.Instance.ChangeMorale(moraleDelta);

            moralStatusText.text = $"Morale changed: {moraleDelta:+#;-#;0}% → {MoraleSystem.Instance.morale:F0}% ({MoraleSystem.Instance.GetStateName()})";
        }

        submitButton.interactable = false;
        Invoke(nameof(FinishTest), 3f);
    }

    string GetGrade(float percent)
    {
        if (percent >= 90) return "A";
        if (percent >= 80) return "B";
        if (percent >= 70) return "C";
        if (percent >= 60) return "D";
        return "F";
    }

    Color GetGradeColor(string grade)
    {
        switch (grade)
        {
            case "A": return new Color(0.2f, 1f, 0.2f);
            case "B": return new Color(0.4f, 0.9f, 0.4f);
            case "C": return new Color(1f, 0.8f, 0.3f);
            case "D": return new Color(1f, 0.6f, 0.2f);
            default: return Color.red;
        }
    }

    void FinishTest()
    {
        seat.FinishLesson();
        LessonManager.Instance.AddLesson();
    }
}
