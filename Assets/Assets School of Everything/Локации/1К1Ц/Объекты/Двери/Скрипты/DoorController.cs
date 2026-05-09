using UnityEngine;
using System.Collections;

public class RotatingDoor_Raycast : MonoBehaviour
{
    [Header("Assign")]
    public Transform door;
    public Collider doorCollider;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openSpeed = 6f;
    public float openTime = 2f;

    [Header("Interaction")]
    public float interactDistance = 3f; // <-- максимум, с какого расстояния можно открыть дверь

    Quaternion closedRot;
    Quaternion openRot;
    bool isMoving = false;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (door == null && transform.childCount > 0) door = transform.GetChild(0);
        if (door == null) Debug.LogError("RotatingDoor_Raycast: не назначен door (child) в инспекторе.");

        if (doorCollider == null && door != null)
            doorCollider = door.GetComponent<Collider>();

        closedRot = transform.rotation;
        openRot = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, openAngle, 0f));
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null) cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Проверка: попали в дверь и не слишком далеко
                if (door != null && (hit.transform == door || hit.transform.IsChildOf(door)))
                {
                    float distance = Vector3.Distance(cam.transform.position, door.position);
                    if (distance <= interactDistance) // <-- проверка дистанции
                    {
                        StartCoroutine(OpenAndClose());
                    }
                    else
                    {
                        Debug.Log("Слишком далеко, чтобы открыть дверь!");
                    }
                }
            }
        }
    }

    IEnumerator OpenAndClose()
    {
        isMoving = true;

        if (doorCollider != null) doorCollider.enabled = false;

        yield return RotateTo(openRot);
        yield return new WaitForSeconds(openTime);
        yield return RotateTo(closedRot);

        if (doorCollider != null) doorCollider.enabled = true;

        isMoving = false;
    }

    IEnumerator RotateTo(Quaternion target)
    {
        while (Quaternion.Angle(transform.rotation, target) > 0.2f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = target;
    }
}
