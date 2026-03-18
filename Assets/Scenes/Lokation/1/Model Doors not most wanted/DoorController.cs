using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    [Header("Sprites: 0 = closed, ... , last = closed-back")]
    public Sprite[] doorSprites; // ожидается 5 спрайтов, но поддерживаются и другие
    public float frameInterval = 0.08f;  // время между кадрами анимации
    public float openDuration = 8f;      // сколько держать открытой

    [Header("Audio (optional)")]
    public AudioClip openSfx;
    public AudioClip closeSfx;

    [Header("Pass sprite index (0-based)")]
    [Tooltip("Индекс спрайта при котором дверь пропускает (например 2 для третьего спрайта)")]
    public int passSpriteIndex = 2;

    SpriteRenderer sr;
    Collider col;
    AudioSource audioSource;

    bool isOpen = false;
    bool isAnimating = false;
    Coroutine autoCloseRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (doorSprites != null && doorSprites.Length > 0)
            sr.sprite = doorSprites[0];

        // по умолчанию закрыта — блокируем проход
        if (col != null) col.isTrigger = false;
    }

    void Update()
    {
        // Ловим клик ЛКМ — делаем raycast из камеры
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider == col)
                {
                    if (!isOpen)
                        StartOpening();
                }
            }
        }
    }

    void StartOpening()
    {
        if (isAnimating) return;
        StopAutoClose();
        StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence()
    {
        isAnimating = true;

        if (doorSprites == null || doorSprites.Length == 0)
        {
            // ничего не задано — просто откроем триггер
            if (col != null) col.isTrigger = true;
            isOpen = true;
            isAnimating = false;
            StartAutoClose();
            yield break;
        }

        for (int i = 0; i < doorSprites.Length; i++)
        {
            sr.sprite = doorSprites[i];

            // делаем проход возможным на нужном кадре (если индекс корректный)
            if (i == passSpriteIndex && col != null) col.isTrigger = true;

            yield return new WaitForSeconds(frameInterval);
        }

        // если последний кадр не был индексом прохода — всё равно оставляем проход открытым
        if (col != null) col.isTrigger = true;

        PlaySfx(openSfx);

        isOpen = true;
        isAnimating = false;

        StartAutoClose();
    }

    IEnumerator CloseSequence()
    {
        isAnimating = true;

        // сразу закрываем проход (чтобы игрок не застрял в закрывающейся двери)
        if (col != null) col.isTrigger = false;

        if (doorSprites == null || doorSprites.Length == 0)
        {
            isOpen = false;
            isAnimating = false;
            yield break;
        }

        for (int i = doorSprites.Length - 1; i >= 0; i--)
        {
            sr.sprite = doorSprites[i];
            yield return new WaitForSeconds(frameInterval);
        }

        PlaySfx(closeSfx);

        isOpen = false;
        isAnimating = false;
    }

    void StartAutoClose()
    {
        StopAutoClose();
        autoCloseRoutine = StartCoroutine(AutoClose());
    }

    void StopAutoClose()
    {
        if (autoCloseRoutine != null) { StopCoroutine(autoCloseRoutine); autoCloseRoutine = null; }
    }

    IEnumerator AutoClose()
    {
        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(CloseSequence());
    }

    void PlaySfx(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
