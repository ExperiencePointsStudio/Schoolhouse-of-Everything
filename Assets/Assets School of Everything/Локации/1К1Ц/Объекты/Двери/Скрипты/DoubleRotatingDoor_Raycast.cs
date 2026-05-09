using UnityEngine;
using System.Collections;

public class DoubleRotatingDoor_Raycast : MonoBehaviour
{
    [Header("Doors")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Colliders")]
    public Collider leftDoorCollider;
    public Collider rightDoorCollider;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openSpeed = 6f;
    public float openTime = 2f;

    [Header("Interaction")]
    public float interactDistance = 3f;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;

    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private bool isMoving = false;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("Не назначены двери!");
            return;
        }

        if (leftDoorCollider == null)
            leftDoorCollider = leftDoor.GetComponent<Collider>();

        if (rightDoorCollider == null)
            rightDoorCollider = rightDoor.GetComponent<Collider>();

        // Сохраняем исходные повороты
        leftClosedRot = leftDoor.localRotation;
        rightClosedRot = rightDoor.localRotation;

        // ВАЖНО:
        // localRotation вместо rotation
        // чтобы каждая дверь крутилась относительно своей оси

        leftOpenRot = leftClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
        rightOpenRot = rightClosedRot * Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null)
                cam = Camera.main;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                bool clickedDoor =
                    hit.transform == leftDoor ||
                    hit.transform.IsChildOf(leftDoor) ||
                    hit.transform == rightDoor ||
                    hit.transform.IsChildOf(rightDoor);

                if (clickedDoor)
                {
                    float distance = Vector3.Distance(
                        cam.transform.position,
                        transform.position);

                    if (distance <= interactDistance)
                    {
                        StartCoroutine(OpenAndClose());
                    }
                    else
                    {
                        Debug.Log("Слишком далеко!");
                    }
                }
            }
        }
    }

    IEnumerator OpenAndClose()
    {
        isMoving = true;

        if (leftDoorCollider != null)
            leftDoorCollider.enabled = false;

        if (rightDoorCollider != null)
            rightDoorCollider.enabled = false;

        yield return RotateDoors(leftOpenRot, rightOpenRot);

        yield return new WaitForSeconds(openTime);

        yield return RotateDoors(leftClosedRot, rightClosedRot);

        if (leftDoorCollider != null)
            leftDoorCollider.enabled = true;

        if (rightDoorCollider != null)
            rightDoorCollider.enabled = true;

        isMoving = false;
    }

    IEnumerator RotateDoors(Quaternion leftTarget, Quaternion rightTarget)
    {
        while (
            Quaternion.Angle(leftDoor.localRotation, leftTarget) > 0.2f ||
            Quaternion.Angle(rightDoor.localRotation, rightTarget) > 0.2f)
        {
            leftDoor.localRotation = Quaternion.Slerp(
                leftDoor.localRotation,
                leftTarget,
                Time.deltaTime * openSpeed);

            rightDoor.localRotation = Quaternion.Slerp(
                rightDoor.localRotation,
                rightTarget,
                Time.deltaTime * openSpeed);

            yield return null;
        }

        leftDoor.localRotation = leftTarget;
        rightDoor.localRotation = rightTarget;
    }
}