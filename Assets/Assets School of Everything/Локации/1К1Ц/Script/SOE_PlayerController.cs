using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]

public class SOE_PlayerController : MonoBehaviour
{
    // -------- Movement ----------
    [Header("Movement")]
    public float walkSpeed = 8f;
    public float runSpeed = 10f;
    public float jumpHeight = 1f;
    public float gravity = -18f;
    public float acceleration = 45f;
    public float deceleration = 60f;
    [Range(0f, 1f)] public float airControl = 0.55f;

    // -------- Stamina ----------
    [Header("Stamina")]
    public float maxStamina = 200f;
    public float sprintDrainPerSec = 6f;
    [Range(0f, 100f)] public float jumpCostPercent = 20f;
    public float regenPerSec = 10f;
    public float regenDelay = 0.8f;
    [Range(0f, 100f)] public float runResumePercent = 15f;
    public Slider staminaBar;

    // -------- Health ----------
    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;
    public TMPro.TMP_Text healthText;

    // damage from morale system
    public float moraleZeroDamagePerMinute = 3f;
    public UnityEngine.UI.Slider healthBar;

    // -------- Camera / Look ----------
    [Header("Camera / Look")]
    public Transform cameraTransform;
    public float cameraHeight = 1.6f;
    public float lookSensitivity = 200f;
    [Range(0f, 0.2f)] public float lookSmoothTime = 0.02f;
    public float maxLookSpeed = 720f;

    // -------- Headbob ----------
    [Header("Headbob")]
    public float walkBobAmplitude = 0.05f;
    public float walkBobFrequency = 6f;
    public float runBobAmplitude = 0.1f;
    public float runBobFrequency = 10f;
    public float breathAmplitude = 0.01f;
    public float breathFrequency = 1f;

    // -------- UI Effects ----------
    [Header("UI Effects")]
    public Image darkOverlay;
    public float overlayFadeSpeed = 2f;

    // -------- Audio ----------
    [Header("Audio Clips")]
    public AudioClip walkStep;
    public AudioClip runStep;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip breatheClip;
    public float walkStepInterval = 0.6f;
    public float runStepInterval = 0.35f;

    // --- internals ---
    CharacterController cc;
    Vector3 horizontalVel;
    float verticalVel;
    float stamina;
    float regenTimer;
    bool exhausted;

    float yaw, pitch;
    float smX, smY, mouseXSmoothVel, mouseYSmoothVel;
    float bobTimer;

    // --- audio internals ---
    AudioSource audioSource;
    AudioSource breatheSource;
    float stepTimer;
    bool wasGrounded;
    bool breathing;

    // --- morale integration ---
    [Header("Morale integration")]
    public bool useMoraleIntegration = true;

    // store base values so we can apply multipliers (saved once)
    float baseWalkSpeed, baseRunSpeed, baseRegenPerSec, baseSprintDrainPerSec, baseJumpCostPercent;

    // cached multipliers
    float walkSpeedMultiplier = 1f;
    float runSpeedMultiplier = 1f;
    float regenMultiplier = 1f;
    float drainMultiplier = 1f;
    float jumpCostMultiplier = 1f;
    float extraOverlayFromMorale = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // extra audio source for breathing
        breatheSource = gameObject.AddComponent<AudioSource>();
        breatheSource.loop = true;
        breatheSource.clip = breatheClip;
        breatheSource.volume = 0f;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            cameraTransform.SetParent(transform, false);
            cameraTransform.localPosition = new Vector3(0f, cameraHeight, 0f);
            if (cameraTransform.GetComponent<AudioListener>() == null)
                cameraTransform.gameObject.AddComponent<AudioListener>();
        }

        stamina = maxStamina;
        if (staminaBar)
        {
            staminaBar.minValue = 0;
            staminaBar.maxValue = maxStamina;
            staminaBar.value = stamina;
        }

        health = maxHealth;

        if (healthBar)

        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        // store base values (take inspector values as base)
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
        baseRegenPerSec = regenPerSec;
        baseSprintDrainPerSec = sprintDrainPerSec;
        baseJumpCostPercent = jumpCostPercent;

        yaw = transform.eulerAngles.y;
        pitch = (cameraTransform != null) ? cameraTransform.localEulerAngles.x : 0f;

        LockCursor(true);
    }

    void Update()
    {
        if (PauseMenu.IsPaused)
            return;
        // apply morale before movement each frame
        HandleMoraleEffects();

        HandleLook();
        HandleMove();
        HandleStamina();
        HandleHealth();
        HandleHeadbob();
        HandleFootsteps();
        HandleBreathing();
        HandleLanding();
    }

    // ===========================
    // Cursor helper (fix for CS0103)
    // ===========================
    void OnApplicationFocus(bool focus) => LockCursor(focus);

    void LockCursor(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    // ---------- Camera Look ----------
    void HandleLook()
    {
        float rawX = Input.GetAxis("Mouse X") * lookSensitivity;
        float rawY = Input.GetAxis("Mouse Y") * lookSensitivity;

        smX = Mathf.SmoothDamp(smX, rawX, ref mouseXSmoothVel, lookSmoothTime);
        smY = Mathf.SmoothDamp(smY, rawY, ref mouseYSmoothVel, lookSmoothTime);

        smX = Mathf.Clamp(smX, -maxLookSpeed, maxLookSpeed);
        smY = Mathf.Clamp(smY, -maxLookSpeed, maxLookSpeed);

        yaw += smX * Time.deltaTime;
        pitch -= smY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        if (cameraTransform != null) cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    // ---------- Movement ----------
    void HandleMove()
    {
        bool grounded = cc.isGrounded;
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 inputDir = (transform.right * input.x + transform.forward * input.y).normalized;

        bool sprinting = Input.GetKey(KeyCode.LeftShift) && !exhausted && stamina > 0f && inputDir.magnitude > 0;
        float targetSpeed = sprinting ? runSpeed : walkSpeed;

        float accel = inputDir.magnitude > 0 ? acceleration : deceleration;
        float control = grounded ? 1f : airControl;
        Vector3 targetVel = inputDir * targetSpeed * inputDir.magnitude * control;

        horizontalVel = Vector3.MoveTowards(horizontalVel, targetVel, accel * Time.deltaTime);

        if (grounded && verticalVel < 0) verticalVel = -2f;

        if (grounded && Input.GetButtonDown("Jump") && stamina > maxStamina * (jumpCostPercent / 100f))
        {
            verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
            SpendStaminaPercent(jumpCostPercent);
            regenTimer = regenDelay;
            if (jumpClip) audioSource.PlayOneShot(jumpClip);
        }

        verticalVel += gravity * Time.deltaTime;
        cc.Move((horizontalVel + Vector3.up * verticalVel) * Time.deltaTime);
    }

    // ---------- Stamina ----------
    void HandleStamina()
    {
        bool moving = new Vector2(horizontalVel.x, horizontalVel.z).magnitude > 0.1f;
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && moving && !exhausted;

        if (sprinting && cc.isGrounded)
        {
            stamina -= sprintDrainPerSec * Time.deltaTime;
            if (stamina <= 0) { stamina = 0; exhausted = true; }
            regenTimer = regenDelay;
        }
        else
        {
            if (regenTimer > 0) regenTimer -= Time.deltaTime;
            else if (stamina < maxStamina)
            {
                bool standingStill = new Vector2(horizontalVel.x, horizontalVel.z).magnitude < 0.1f;
                if (standingStill)
                {
                    stamina += regenPerSec * Time.deltaTime;
                    if (stamina > maxStamina) stamina = maxStamina;
                }
            }
            if (exhausted && stamina >= maxStamina * (runResumePercent / 100f))
                exhausted = false;
        }

        if (staminaBar) staminaBar.value = stamina;

        if (darkOverlay)
        {
            Color c = darkOverlay.color;
            float baseTargetAlpha = exhausted ? 0.5f : 0f;
            float targetAlpha = Mathf.Clamp01(baseTargetAlpha + extraOverlayFromMorale * (exhausted ? 1f : 0.6f));
            c.a = Mathf.MoveTowards(c.a, targetAlpha, overlayFadeSpeed * Time.deltaTime);
            darkOverlay.color = c;
        }
    }

    //---------- Health ----------
    void HandleHealth()
    {
        if (MoraleSystem.Instance == null) return;

        if (MoraleSystem.Instance.GetMorale() <= 0f)
        {
            float damagePerSecond = moraleZeroDamagePerMinute / 60f;
            health -= damagePerSecond * Time.deltaTime;

            if (health < 0f)
                health = 0f;
        }

        if (healthBar)
            healthBar.value = health;

        if (healthText)
        {
            float percent = (health / maxHealth) * 100f;
            healthText.text = $"HP {percent:F0}%";
        }

    }


    // ---------- Headbob ----------
    void HandleHeadbob()
    {
        Vector3 basePos = new Vector3(0, cameraHeight, 0);
        float speed = new Vector2(horizontalVel.x, horizontalVel.z).magnitude;

        if (cc.isGrounded && speed > 0.1f)
        {
            bool running = Input.GetKey(KeyCode.LeftShift) && !exhausted && stamina > 0.05f;
            bobTimer += Time.deltaTime * (running ? runBobFrequency : walkBobFrequency);
            float amp = running ? runBobAmplitude : walkBobAmplitude;

            if (cameraTransform != null)
                cameraTransform.localPosition = basePos + new Vector3(
                    Mathf.Sin(bobTimer) * amp * 0.5f,
                    Mathf.Cos(bobTimer * 2f) * amp,
                    0f
                );
        }
        else if (cc.isGrounded)
        {
            float y = Mathf.Sin(Time.time * (Mathf.PI * 2f) * breathFrequency) * breathAmplitude;
            if (cameraTransform != null)
                cameraTransform.localPosition = basePos + new Vector3(0, y, 0);
        }
        else
        {
            if (cameraTransform != null)
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, basePos, 5f * Time.deltaTime);
        }
    }

    // ---------- Audio ----------
    void HandleFootsteps()
    {
        if (!cc.isGrounded) return;

        float speed = new Vector2(horizontalVel.x, horizontalVel.z).magnitude;
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && !exhausted && stamina > 0.05f;

        if (speed > 0.2f)
        {
            stepTimer -= Time.deltaTime;
            float interval = sprinting ? runStepInterval : walkStepInterval;
            if (stepTimer <= 0f)
            {
                AudioClip clip = sprinting ? runStep : walkStep;
                if (clip) audioSource.PlayOneShot(clip);
                stepTimer = interval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void HandleBreathing()
    {
        float staminaPercent = stamina / maxStamina;

        if (staminaPercent <= 0f && !breathing)
        {
            breathing = true;
            breatheSource.Play();
        }

        if (breathing)
        {
            float targetVolume = staminaPercent < 0.2f ? 1f : 0f;
            breatheSource.volume = Mathf.MoveTowards(breatheSource.volume, targetVolume, Time.deltaTime);
            if (breatheSource.volume <= 0.01f && staminaPercent > 0.2f)
            {
                breatheSource.Stop();
                breathing = false;
            }
        }
    }

    void HandleLanding()
    {
        if (!wasGrounded && cc.isGrounded)
        {
            if (landClip) audioSource.PlayOneShot(landClip);
        }
        wasGrounded = cc.isGrounded;
    }

    // ---------- Morale integration ----------
    // reads MoraleSystem.Instance.GetMorale() (0..100) and applies multipliers to base stats.
    void HandleMoraleEffects()
    {
        if (!useMoraleIntegration || MoraleSystem.Instance == null)
        {
            // restore base values
            walkSpeed = baseWalkSpeed;
            runSpeed = baseRunSpeed;
            regenPerSec = baseRegenPerSec;
            sprintDrainPerSec = baseSprintDrainPerSec;
            jumpCostPercent = baseJumpCostPercent;
            extraOverlayFromMorale = 0f;
            return;
        }

        float normalized = Mathf.Clamp01(MoraleSystem.Instance.GetMorale() / 100f);

        // multipliers (tweakable)
        walkSpeedMultiplier = 0.85f + normalized * 0.3f;   // 0.85 .. 1.15
        runSpeedMultiplier = 0.8f + normalized * 0.4f;   // 0.8  .. 1.2
        regenMultiplier = 0.6f + normalized * 0.6f;   // 0.6  .. 1.2
        drainMultiplier = 1.2f - normalized * 0.5f;   // 1.2  .. 0.7
        jumpCostMultiplier = 1f + (0.25f * (0.5f - normalized)); // ~0.875 .. 1.125

        extraOverlayFromMorale = (1f - normalized) * 0.6f; // 0..0.6

        // apply
        walkSpeed = baseWalkSpeed * walkSpeedMultiplier;
        runSpeed = baseRunSpeed * runSpeedMultiplier;
        regenPerSec = baseRegenPerSec * regenMultiplier;
        sprintDrainPerSec = baseSprintDrainPerSec * drainMultiplier;
        jumpCostPercent = baseJumpCostPercent * jumpCostMultiplier;
    }

    void SpendStaminaPercent(float percent)
    {
        stamina -= maxStamina * Mathf.Clamp01(percent / 100f);
        if (stamina < 0) stamina = 0;
    }
}
