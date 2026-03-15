using UnityEngine;
using TMPro;

public class MoraleSystem : MonoBehaviour
{
    public static MoraleSystem Instance;

    [Header("Settings")]
    [Range(0f, 100f)] public float morale = 50f;
    public TMP_Text moraleText;
    public UnityEngine.UI.Slider moraleSlider;

    private float idleTimer = 0f;
    private Vector3 lastPosition;
    private Transform playerTransform;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        var player = FindObjectOfType<SOE_PlayerController>();
        if (player != null) playerTransform = player.transform;
        lastPosition = playerTransform ? playerTransform.position : Vector3.zero;
        UpdateUI();
    }

    void Update()
    {
        HandleIdleDecay();
    }

    public void ChangeMorale(float amount)
    {
        morale = Mathf.Clamp(morale + amount, 0f, 100f);
        UpdateUI();
        Debug.Log($"[MoraleSystem] morale changed by {amount:F1} => {morale:F1}% ({GetStateName()})");
    }

    public void SetMorale(float value)
    {
        morale = Mathf.Clamp(value, 0f, 100f);
        UpdateUI();
    }

    public float GetMorale() => morale;

    public string GetStateName()
    {
        if (morale >= 90f) return "JOY";
        if (morale >= 75f) return "GOOD";
        if (morale >= 60f) return "OKAY";
        if (morale >= 45f) return "NORMAL";
        if (morale >= 35f) return "AVERAGE";
        if (morale >= 25f) return "SADNESS";
        if (morale >= 10f) return "SORROW";
        return "DESPAIR";
    }

    void UpdateUI()
    {
        if (moraleText != null)
            moraleText.text = $"Morale status: {morale:F0}% ({GetStateName()})";

        if (moraleSlider != null)
            moraleSlider.value = morale;
    }

    void HandleIdleDecay()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(lastPosition, playerTransform.position);

        if (distance < 0.1f)
            idleTimer += Time.deltaTime;
        else
            idleTimer = 0f;

        lastPosition = playerTransform.position;

        if (idleTimer >= 90f)
        {
            ChangeMorale(-2f);
            idleTimer = 0f;
        }
    }
}
