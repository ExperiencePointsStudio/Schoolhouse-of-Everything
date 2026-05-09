using UnityEngine;
using TMPro;
using System;

public class DialogueUI : MonoBehaviour
{
    public GameObject root;
    public TMP_Text dialogueText;
    public TMP_Text hintText;

    Action<bool> onAnswer;
    Action onFinish;

    bool waitingAnswer;
    bool waitingClose;

    SOE_PlayerController player;

    void Update()
    {
        if (!root.activeSelf) return;

        if (waitingAnswer)
        {
            if (Input.GetKeyDown(KeyCode.E))
                Answer(true);
            if (Input.GetKeyDown(KeyCode.Q))
                Answer(false);
        }
        else if (waitingClose && Input.GetKeyDown(KeyCode.Return))
        {
            Close();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Begin(string greeting, Action<bool> answer, Action finish)
    {
        root.SetActive(true);

        dialogueText.text = greeting;
        hintText.text = "[E] — Да    [Q] — Нет";

        waitingAnswer = true;
        waitingClose = false;

        onAnswer = answer;
        onFinish = finish;

        player = FindObjectOfType<SOE_PlayerController>();
        if (player) player.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Answer(bool positive)
    {
        waitingAnswer = false;
        onAnswer?.Invoke(positive);

        hintText.text = "Нажмите Enter, чтобы закончить диалог";
        waitingClose = true;
    }

    public void ShowNPCResponse(string response)
    {
        dialogueText.text = response;
    }

    void Close()
    {
        root.SetActive(false);

        if (player) player.enabled = true;

        onFinish?.Invoke();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
