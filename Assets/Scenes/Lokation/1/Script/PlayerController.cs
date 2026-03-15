using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float baseCameraHeight = 1.7f;
    public float bobAmplitudeWalk = 0.05f;
    public float bobAmplitudeRun = 0.12f;
    public float bobFrequencyWalk = 5f;
    public float bobFrequencyRun = 9f;
    private float bobTimer;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaDrain = 0.7f;   // медленнее тратится
    public float staminaRegen = 0.8f;   // медленнее восстанавливается
    private float stamina;

    [Header("UI")]
    public Slider staminaBar;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = maxStamina;
        }
    }

    void Update()
    {
        MovePlayer();
        HandleCameraBob();
        // UpdateUI();
    }

    void MovePlayer()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        bool isMoving = move.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && stamina > 0 && isMoving;

        float speed = isRunning ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        // Стамина
        if (isRunning)
        {
            stamina -= staminaDrain * Time.deltaTime;
            if (stamina < 0) stamina = 0;
        }
        else if (!isMoving && stamina < maxStamina) // реген только в покое
        {
            stamina += staminaRegen * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCameraBob()
    {
        if (playerCamera == null) return;

        Vector3 camPos = playerCamera.localPosition;
        Vector3 flatVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        if (flatVelocity.magnitude > 0.1f && controller.isGrounded)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && stamina > 0;

            float amplitude = isRunning ? bobAmplitudeRun : bobAmplitudeWalk;
            float frequency = isRunning ? bobFrequencyRun : bobFrequencyWalk;

            bobTimer += Time.deltaTime * frequency;

            // Вверх-вниз + кач в стороны
            camPos.y = Mathf.Sin(bobTimer) * amplitude + baseCameraHeight;
            camPos.x = Mathf.Cos(bobTimer * 2) * amplitude * 0.6f;
        }
        else
        {
            bobTimer = 0;
            camPos = Vector3.Lerp(camPos, new Vector3(0, baseCameraHeight, 0), Time.deltaTime * 5f);
        }

        playerCamera.localPosition = camPos;
    }
}